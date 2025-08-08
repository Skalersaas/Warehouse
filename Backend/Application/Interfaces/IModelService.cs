using Domain.Models.Interfaces;
using Utilities.DataManipulation;
namespace Application.Interfaces;

public interface IModelService<TModel, TCreate, TUpdate, TResponse>
       where TModel : class, IModel
{
    Task<(bool, TResponse?)> CreateAsync(TCreate entity);
    Task<(bool, TResponse?)> GetByIdAsync(int id);
    Task<(TResponse[], int)> QueryBy(SearchModel model);
    Task<(bool, TResponse?)> UpdateAsync(TUpdate entity);
    Task<bool> DeleteAsync(int id);
}