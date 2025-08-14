using Application.Interfaces;
using Domain.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Persistence.Data.Interfaces;
using System.Linq.Expressions;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Services.Base
{
    public class ModelService<TModel, TCreate, TUpdate>(IRepository<TModel> context, ILogger<ModelService<TModel, TCreate, TUpdate>> logger) : IModelService<TModel, TCreate, TUpdate>
        where TModel : class, IModel, new()
    {
        protected readonly IRepository<TModel> repo = context;
        protected readonly ILogger<ModelService<TModel, TCreate, TUpdate>> _logger = logger;

        public virtual async Task<Result<TModel>> CreateAsync(TCreate entity)
        {
            try
            {
                if (entity == null)
                {
                    return Result<TModel>.ErrorResult("Entity cannot be null");
                }

                var model = Mapper.FromDTO<TModel, TCreate>(entity);
                var created = await repo.CreateAsync(model);

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

                var success = await repo.DeleteAsync(id);
                
                if (!success)
                {
                    return Result.ErrorResult("Entity not found or could not be deleted");
                }

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
            try
            {
                if (id <= 0)
                {
                    return Result<TModel>.ErrorResult("Invalid ID provided");
                }

                var model = await repo.GetByIdAsync(id);

                if (model == null)
                {
                    return Result<TModel>.ErrorResult($"Entity with ID {id} not found");
                }

                return Result<TModel>.SuccessResult(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID {Id} of type {EntityType}", id, typeof(TModel).Name);
                return Result<TModel>.ErrorResult("An error occurred while retrieving the entity");
            }
        }

        public async Task<Result<TModel>> GetByIdAsync(int id, params Expression<Func<TModel, object>>[] includes)
        {
            try
            {
                if (id <= 0)
                {
                    return Result<TModel>.ErrorResult("Invalid ID provided");
                }

                var model = await repo.GetByIdAsync(id, includes);
                
                if (model == null)
                {
                    return Result<TModel>.ErrorResult($"Entity with ID {id} not found");
                }

                return Result<TModel>.SuccessResult(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity by ID {Id} with includes of type {EntityType}", id, typeof(TModel).Name);
                return Result<TModel>.ErrorResult("An error occurred while retrieving the entity");
            }
        }

        public virtual async Task<Result<(IEnumerable<TModel>, int)>> QueryBy(SearchModel model)
        {
            try
            {
                if (model == null)
                {
                    return Result<(IEnumerable<TModel>, int)>.ErrorResult("Search model cannot be null");
                }

                var (data, fullCount) = await repo.QueryBy(model);

                return Result<(IEnumerable<TModel>, int)>.SuccessResult((data, fullCount),
                    count: fullCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying entities of type {EntityType}", typeof(TModel).Name);
                return Result<(IEnumerable<TModel>, int)>.ErrorResult("An error occurred while searching entities");
            }
        }

        public virtual async Task<Result<TModel>> UpdateAsync(TUpdate entity)
        {
            try
            {
                if (entity == null)
                {
                    return Result<TModel>.ErrorResult("Entity cannot be null");
                }

                var model = Mapper.FromDTO<TModel, TUpdate>(entity);
                var updated = await repo.UpdateAsync(model);

                if (updated == null)
                {
                    return Result<TModel>.ErrorResult("Entity not found or could not be updated");
                }

                return Result<TModel>.SuccessResult(updated, "Entity updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(TModel).Name);
                return Result<TModel>.ErrorResult("An error occurred while updating the entity");
            }
        }
    }
}
