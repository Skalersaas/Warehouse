using Domain.Models.Interfaces;
using System.Linq.Expressions;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Interfaces;

public interface IModelService<TModel, TCreate, TUpdate>
    where TModel : class, IModel
{
    Task<Result<TModel>> CreateAsync(TCreate entity);
    Task<Result<TModel>> GetByIdAsync(int id);
    Task<Result<TModel>> GetByIdAsync(int id, params Expression<Func<TModel, object>>[] includes);
    Task<Result<(IEnumerable<TModel> list, int count)>> QueryBy(SearchModel model, Func<IQueryable<TModel>, IQueryable<TModel>>? includeConfig = null);
    Task<Result<TModel>> UpdateAsync(TUpdate entity);
    Task<Result> DeleteAsync(int id);
}
