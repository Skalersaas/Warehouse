using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Persistence.Data;
using System.Collections.Generic;
using System.Linq.Expressions;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Services.Base
{
    public class ModelService<TModel, TCreate, TUpdate>(ApplicationContext context, ILogger<ModelService<TModel, TCreate, TUpdate>> logger) : IModelService<TModel, TCreate, TUpdate>
        where TModel : class, IModel, new()
        where TUpdate : IModel
    {
        protected readonly DbSet<TModel> repo = context.Set<TModel>();
        protected readonly ILogger<ModelService<TModel, TCreate, TUpdate>> _logger = logger;
        protected readonly ApplicationContext _context = context;

        public virtual async Task<Result<TModel>> CreateAsync(TCreate entity)
        {
            try
            {
                if (entity == null)
                {
                    return Result<TModel>.ErrorResult("Entity cannot be null");
                }

                var model = Mapper.AutoMap<TModel, TCreate>(entity);
                var created = (await repo.AddAsync(model)).Entity;
                await context.SaveChangesAsync();

                if (created == null)
                {
                    return Result<TModel>.ErrorResult("Entity with such name/number exists");
                }

                return Result<TModel>.SuccessResult(created, "Entity created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity of type {EntityType}", typeof(TModel).Name);
                return Result<TModel>.ErrorResult("An error occurred while creating the entity");
            }
        }

        public virtual async Task<Result> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return Result.ErrorResult("Invalid ID provided");
                }
                var found = await repo.FirstOrDefaultAsync(x => x.Id == id);

                if (found == null)
                    return Result.ErrorResult("Entity not found");

                repo.Remove(found);
                await context.SaveChangesAsync();

                return Result.SuccessResult("Entity deleted successfully");
            }
            catch (DbUpdateException)
            {
                return Result.ErrorResult("Cannot delete this entity because it is referenced by other records.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity with ID {Id} of type {EntityType}", id, typeof(TModel).Name);
                return Result.ErrorResult("An error occurred while deleting the entity");
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
                // Input validation
                if (id <= 0)
                {
                    return Result<TModel>.ErrorResult("Invalid ID provided");
                }

                // Build and execute query
                var query = includeConfig?.Invoke(repo.AsNoTracking()) ?? repo.AsNoTracking();
                var found = await query.FirstOrDefaultAsync(x => x.Id == id);

                // Return result
                return found == null
                    ? Result<TModel>.ErrorResult("Entity not found")
                    : Result<TModel>.SuccessResult(found);
            }
            catch (Exception ex)
            {
                var logMessage = includeConfig != null
                    ? "Error getting entity by ID {Id} with includes of type {EntityType}"
                    : "Error getting entity by ID {Id} of type {EntityType}";

                _logger.LogError(ex, logMessage, id, typeof(TModel).Name);
                return Result<TModel>.ErrorResult("An error occurred while retrieving the entity");
            }
        }
        public virtual async Task<Result<(IEnumerable<TModel> list, int count)>> QueryBy(SearchModel model, 
            Func<IQueryable<TModel>, IQueryable<TModel>>? includeConfig = null)
        {
            try
            {
                if (model == null)
                {
                    return Result<(IEnumerable<TModel>, int)>.ErrorResult("Search model cannot be null");
                }

                var (data, fullCount) = await QueryHelper(model, includeConfig);

                return Result<(IEnumerable<TModel>, int)>.SuccessResult((data, fullCount),
                    count: fullCount);
            }
            catch (ArgumentException arg)
            {
                _logger.LogError(arg, message: arg.Message);
                return Result<(IEnumerable<TModel>, int)>.ErrorResult(arg.Message);
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
            int total = 0;

            if (model is not null)
            {

                baseQuery = model switch
                {
                    SearchFilterModelDates dates => QueryMaster<TModel>.FilterByFieldsAndDate(baseQuery, dates.Filters, "CreatedAt", dates.DateFrom, dates.DateTo),
                    SearchFilterModel filterModel => QueryMaster<TModel>.FilterByFields(baseQuery, filterModel.Filters),
                    _ => baseQuery
                };

                total = await baseQuery.CountAsync();

                var dataQuery = includeConfig?.Invoke(baseQuery) ?? baseQuery;

                dataQuery = QueryMaster<TModel>.OrderByField(dataQuery, model.SortedField, model.IsDescending);

                if (model.PaginationValid())
                    dataQuery = dataQuery.Skip((model.Page - 1) * model.Size).Take(model.Size);

                var data = await dataQuery.ToListAsync();
                return (data, total);
            }

            var allData = await (includeConfig?.Invoke(baseQuery) ?? baseQuery).ToListAsync();
            return (allData, allData.Count);
        }
        public virtual async Task<Result<TModel>> UpdateAsync(TUpdate entity)
        {
            try
            {
                if (entity == null)
                {
                    return Result<TModel>.ErrorResult("Entity cannot be null");
                }
                var found = await repo.FirstOrDefaultAsync(x => x.Id == entity.Id);

                if (found == null)
                {
                    return Result<TModel>.ErrorResult("Entity not found");
                }
                Mapper.AutoMapToExisting(entity, found);
                await context.SaveChangesAsync();

                return Result<TModel>.SuccessResult(found, "Entity updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(TModel).Name);
                return Result<TModel>.ErrorResult("An error occurred while updating the entity");
            }
        }
    }
}
