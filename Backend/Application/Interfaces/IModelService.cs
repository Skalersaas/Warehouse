using Domain.Models.Interfaces;
using System.Linq.Expressions;
using Utilities.DataManipulation;
namespace Application.Interfaces;

public interface IModelService<TModel, TCreate, TUpdate>
       where TModel : class, IModel
{
    Task<(bool, TModel?)> CreateAsync(TCreate entity);
    Task<(bool, TModel?)> GetByIdAsync(int id);
    Task<(bool, TModel?)> GetByIdAsync(int id, params Expression<Func<TModel, object>>[] includes);
    Task<(IEnumerable<TModel>, int)> QueryBy(SearchModel model);
    Task<(bool, TModel?)> UpdateAsync(TUpdate entity);
    Task<bool> DeleteAsync(int id);
}