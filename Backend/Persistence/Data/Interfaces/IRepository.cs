using Domain.Models.Interfaces;
using System.Linq.Expressions;
using Utilities.DataManipulation;

namespace Persistence.Data.Interfaces;

public interface IRepository<T> where T : class, IModel
{
    Task<T?> CreateAsync(T entity);

    Task<T?> GetByIdAsync(int id);

    Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes);

    Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);
    Task<(IEnumerable<T> Data, int TotalCount)> QueryBy(
        SearchModel? model = null,
        params Expression<Func<T, object>>[] includes);

    Task<int> GetCountAsync(Expression<Func<T, bool>>? predicate = null);

    Task<T> UpdateAsync(T entity);

    Task<bool> DeleteAsync(int id);

    void Detach(T entity);
}