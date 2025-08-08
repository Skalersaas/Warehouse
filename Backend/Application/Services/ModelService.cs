using Application.Interfaces;
using Domain.Models.Interfaces;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services
{
    public class ModelService<TModel, TCreate, TUpdate, TResponse>(IRepository<TModel> context) : IModelService<TModel, TCreate, TUpdate, TResponse>
        where TModel : class, IModel, new()
        where TResponse : class, new()
    {
        protected readonly IRepository<TModel> repo = context;
        public virtual async Task<(bool, TResponse?)> CreateAsync(TCreate entity)
        {
            var model = Mapper.FromDTO<TModel, TCreate>(entity);
            var created = await repo.CreateAsync(model);

            return created == null
                ? (false, null)
                : (true, Mapper.FromDTO<TResponse, TModel>(created));
        }

        public virtual async Task<bool> DeleteAsync(int id) => await repo.DeleteAsync(id);
        public virtual async Task<(bool, TResponse?)> GetByIdAsync(int id)
        {
            var model = await repo.GetByIdAsync(id);

            return model == null
                ? (false, null)
                : (true, Mapper.FromDTO<TResponse, TModel>(model));
        }
        public virtual async Task<(TResponse[], int)> QueryBy(SearchModel model)
        {
            var (data, fullCount) = await repo.QueryBy(model);

            var responseList = data.Select(Mapper.FromDTO<TResponse, TModel>).ToArray();

            return (responseList, fullCount);
        }

        public virtual async Task<(bool, TResponse?)> UpdateAsync(TUpdate entity)
        {
            var model = Mapper.FromDTO<TModel, TUpdate>(entity);
            var updated = await repo.UpdateAsync(model);

            return updated == null
                ? (false, null)
                : (true, Mapper.FromDTO<TResponse, TModel>(updated));

        }
    }
}