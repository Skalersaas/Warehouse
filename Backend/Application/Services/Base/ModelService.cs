using Application.Interfaces;
using Domain.Models.Interfaces;
using Persistence.Data.Interfaces;
using System.Linq.Expressions;
using Utilities.DataManipulation;

namespace Application.Services.Base
{
    public class ModelService<TModel, TCreate, TUpdate>(IRepository<TModel> context) : IModelService<TModel, TCreate, TUpdate>
        where TModel : class, IModel, new()
    {
        protected readonly IRepository<TModel> repo = context;
        public virtual async Task<(bool, TModel?)> CreateAsync(TCreate entity)
        {
            var model = Mapper.FromDTO<TModel, TCreate>(entity);
            var created = await repo.CreateAsync(model);

            return created == null
                ? (false, null)
                : (true, created);
        }

        public virtual async Task<bool> DeleteAsync(int id) => await repo.DeleteAsync(id);
        public virtual async Task<(bool, TModel?)> GetByIdAsync(int id)
        {
            var model = await repo.GetByIdAsync(id);

            return model == null
                ? (false, null)
                : (true, model);
        }
        public async Task<(bool, TModel?)> GetByIdAsync(int id, params Expression<Func<TModel, object>>[] includes)
        {
            var model = await repo.GetByIdAsync(id, includes);
            return model == null
                ? (false, null)
                : (true, model);
        }
        public virtual async Task<(IEnumerable<TModel>, int)> QueryBy(SearchModel model)
        {
            var (data, fullCount) = await repo.QueryBy(model);

            return (data, fullCount);
        }

        public virtual async Task<(bool, TModel?)> UpdateAsync(TUpdate entity)
        {
            var model = Mapper.FromDTO<TModel, TUpdate>(entity);

            var updated = await repo.UpdateAsync(model);

            return updated == null
                ? (false, null)
                : (true, updated);

        }
    }
}