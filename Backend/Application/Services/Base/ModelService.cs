using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using System.ComponentModel.DataAnnotations;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Services.Base
{
	public class ModelService<TModel, TCreate, TUpdate>(
		ApplicationContext context,
		ILogger<ModelService<TModel, TCreate, TUpdate>> logger)
		: IModelService<TModel, TCreate, TUpdate>
		where TModel : class, IModel, new()
		where TUpdate : IModel
	{
		protected readonly DbSet<TModel> repo = context.Set<TModel>();
		protected readonly ILogger<ModelService<TModel, TCreate, TUpdate>> _logger = logger;
		protected readonly ApplicationContext _context = context;
		protected string UniqueFieldName { get; set; } = "number";
		public virtual async Task<Result<TModel>> CreateAsync(TCreate entity)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var (IsValid, ErrorMessages) = ValidateEntity(entity);
				if (!IsValid)
				{
					return Result<TModel>.ErrorResult(string.Join(", ", ErrorMessages));
				}

				var model = Mapper.AutoMap<TModel, TCreate>(entity);
				if (model == null)
				{
					_logger.LogWarning("Failed to map entity of type {CreateType} to {ModelType}",
						typeof(TCreate).Name, typeof(TModel).Name);
					return Result<TModel>.ErrorResult("Failed to process entity data");
				}

				var created = (await repo.AddAsync(model)).Entity;
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Successfully created entity of type {EntityType} with ID {Id}",
					typeof(TModel).Name, created.Id);

				return Result<TModel>.SuccessResult(created, "Entity created successfully");
			}
			catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
			{
				await transaction.RollbackAsync();
				_logger.LogWarning("Unique constraint violation when creating {EntityType}: {Message}",
					typeof(TModel).Name, GetSafeErrorMessage(ex));
				return Result<TModel>.ErrorResult($"Entity with such {UniqueFieldName} already exists");
			}
			catch (DbUpdateException ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Database error creating entity of type {EntityType}", typeof(TModel).Name);
				return Result<TModel>.ErrorResult("Database error occurred while creating the entity");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Unexpected error creating entity of type {EntityType}", typeof(TModel).Name);
				return Result<TModel>.ErrorResult("An unexpected error occurred while creating the entity");
			}
		}

		public virtual async Task<Result> DeleteAsync(int id)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				if (id <= 0)
				{
					return Result.ErrorResult("Invalid ID provided");
				}

				var found = await repo.FirstOrDefaultAsync(x => x.Id == id);
				if (found == null)
				{
					return Result.ErrorResult("Entity not found");
				}

				repo.Remove(found);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Successfully deleted entity of type {EntityType} with ID {Id}",
					typeof(TModel).Name, id);

				return Result.SuccessResult("Entity deleted successfully");
			}
			catch (DbUpdateException ex) when (IsForeignKeyConstraintViolation(ex))
			{
				await transaction.RollbackAsync();
				_logger.LogWarning("Foreign key constraint violation when deleting {EntityType} with ID {Id}",
					typeof(TModel).Name, id);
				return Result.ErrorResult("Cannot delete this entity because it is referenced by other records");
			}
			catch (DbUpdateException ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Database error deleting entity {EntityType} with ID {Id}",
					typeof(TModel).Name, id);
				return Result.ErrorResult("Database error occurred while deleting the entity");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Unexpected error deleting entity {EntityType} with ID {Id}",
					typeof(TModel).Name, id);
				return Result.ErrorResult("An unexpected error occurred while deleting the entity");
			}
		}

		public virtual async Task<Result<TModel>> GetByIdAsync(int id)
		{
			return await GetByIdAsync(id, includeConfig: null);
		}

		public virtual async Task<Result<TModel>> GetByIdAsync(int id, Func<IQueryable<TModel>, IQueryable<TModel>>? includeConfig = null)
		{
			try
			{
				if (id <= 0)
				{
					return Result<TModel>.ErrorResult("Invalid ID provided");
				}

				var query = includeConfig?.Invoke(repo.AsNoTracking()) ?? repo.AsNoTracking();
				var found = await query.FirstOrDefaultAsync(x => x.Id == id);

				if (found == null)
				{
					_logger.LogInformation("Entity of type {EntityType} with ID {Id} not found",
						typeof(TModel).Name, id);
					return Result<TModel>.ErrorResult("Entity not found");
				}

				return Result<TModel>.SuccessResult(found);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving entity {EntityType} with ID {Id}",
					typeof(TModel).Name, id);
				return Result<TModel>.ErrorResult("An error occurred while retrieving the entity");
			}
		}

		public virtual async Task<Result<(IEnumerable<TModel> list, int count)>> QueryBy(
			SearchModel model,
			Func<IQueryable<TModel>, IQueryable<TModel>>? includeConfig = null)
		{
			try
			{
				if (model == null)
				{
					return Result<(IEnumerable<TModel>, int)>.ErrorResult("Search model cannot be null");
				}

				var (data, fullCount) = await QueryHelper(model, includeConfig);

				_logger.LogDebug("Query returned {Count} entities of type {EntityType}",
					data.Count(), typeof(TModel).Name);

				return Result<(IEnumerable<TModel>, int)>.SuccessResult((data, fullCount), count: fullCount);
			}
			catch (InvalidOperationException ex)
			{
				_logger.LogWarning("Invalid query parameters for {EntityType}: {Message}",
					typeof(TModel).Name, GetSafeErrorMessage(ex));
				return Result<(IEnumerable<TModel>, int)>.ErrorResult("Invalid filter parameters");
			}
			catch (ArgumentException ex)
			{
				_logger.LogWarning("Invalid query parameters for {EntityType}: {Message}",
					typeof(TModel).Name, GetSafeErrorMessage(ex));
				return Result<(IEnumerable<TModel>, int)>.ErrorResult("Invalid search parameters");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error querying entities of type {EntityType}", typeof(TModel).Name);
				return Result<(IEnumerable<TModel>, int)>.ErrorResult("An error occurred while searching entities");
			}
		}

		private async Task<(IEnumerable<TModel> Data, int TotalCount)> QueryHelper(
			SearchModel? model = null,
			Func<IQueryable<TModel>, IQueryable<TModel>>? includeConfig = null)
		{
			IQueryable<TModel> baseQuery = repo.AsNoTracking().AsQueryable();

			if (model is not null)
			{
				baseQuery = model switch
				{
					SearchFilterModel filterModel =>
						QueryMaster<TModel>.ApplyFilters(baseQuery, filterModel.Filters),
					_ => baseQuery
				};

				var total = await baseQuery.CountAsync();
				var dataQuery = includeConfig?.Invoke(baseQuery) ?? baseQuery;

                if (string.IsNullOrEmpty(model.SortedField))
                    model.SortedField = "Id";

				dataQuery = QueryMaster<TModel>.ApplyOrdering(dataQuery, model.SortedField, model.IsDescending);

				if (model.PaginationValid())
				{
					dataQuery = dataQuery.Skip((model.Page - 1) * model.Size).Take(model.Size);
				}

				var data = await dataQuery.ToListAsync();
				return (data, total);
			}

			var allData = await (includeConfig?.Invoke(baseQuery) ?? baseQuery).ToListAsync();
			return (allData, allData.Count);
		}

		public virtual async Task<Result<TModel>> UpdateAsync(TUpdate entity)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();
			try
			{
				var (IsValid, ErrorMessages) = ValidateEntity(entity);
				if (!IsValid)
				{
					return Result<TModel>.ErrorResult(string.Join(", ", ErrorMessages));
				}

				var found = await repo.FirstOrDefaultAsync(x => x.Id == entity.Id);
				if (found == null)
				{
					return Result<TModel>.ErrorResult("Entity not found");
				}

				Mapper.AutoMapToExisting(entity, found);
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("Successfully updated entity of type {EntityType} with ID {Id}",
					typeof(TModel).Name, entity.Id);

				return Result<TModel>.SuccessResult(found, "Entity updated successfully");
			}
			catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
			{
				await transaction.RollbackAsync();
				_logger.LogWarning("Unique constraint violation when updating {EntityType} with ID {Id}",
					typeof(TModel).Name, entity.Id);
				return Result<TModel>.ErrorResult($"Entity with such {UniqueFieldName} already exists");
			}
			catch (DbUpdateException ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Database error updating entity {EntityType} with ID {Id}",
					typeof(TModel).Name, entity.Id);
				return Result<TModel>.ErrorResult("Database error occurred while updating the entity");
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Unexpected error updating entity {EntityType} with ID {Id}",
					typeof(TModel).Name, entity.Id);
				return Result<TModel>.ErrorResult("An unexpected error occurred while updating the entity");
			}
		}

		#region Helper Methods

		private static (bool IsValid, List<string> ErrorMessages) ValidateEntity<T>(T entity)
		{
			if (entity == null)
			{
				return (false, new List<string> { "Entity cannot be null" });
			}

			var context = new ValidationContext(entity);
			var results = new List<ValidationResult>();
			var isValid = Validator.TryValidateObject(entity, context, results, true);

			return (isValid, results.Select(r => r.ErrorMessage ?? "Validation error").ToList());
		}

		private static bool IsUniqueConstraintViolation(DbUpdateException ex)
		{
			return ex.InnerException?.Message?.Contains("UNIQUE constraint", StringComparison.OrdinalIgnoreCase) == true ||
				   ex.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
				   ex.InnerException?.Message?.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) == true;
		}

		private static bool IsForeignKeyConstraintViolation(DbUpdateException ex)
		{
			return ex.InnerException?.Message?.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase) == true ||
				   ex.InnerException?.Message?.Contains("foreign key constraint", StringComparison.OrdinalIgnoreCase) == true;
		}

		private static string GetSafeErrorMessage(Exception ex)
		{
			return ex.GetType().Name;
		}

		#endregion
	}
}