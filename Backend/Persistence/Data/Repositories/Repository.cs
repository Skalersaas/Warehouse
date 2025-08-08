using Domain.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Data.Interfaces;
using System.Linq.Expressions;
using Utilities.DataManipulation;

namespace Persistence.Data.Repositories;

public class Repository<T>(ApplicationContext _context) : IRepository<T>
    where T : class, IModel
{
    protected readonly DbSet<T> _set = _context.Set<T>();

    public virtual async Task<T?> CreateAsync(T entity)
    {
        await _set.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _set.FindAsync(id);
    }

    public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set;
        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<T?> GetFirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set;
        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task<(IEnumerable<T> Data, int TotalCount)> QueryBy(
        SearchModel? model = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _set;
        int total = 0;

        if (model is not null)
        {
            if (model is SearchFilterModel filterModel)
                query = QueryMaster<T>.FilterByFields(query, filterModel.Filters);
            query = QueryMaster<T>.OrderByField(query, model.SortedField, model.IsAscending);
            total = await query.CountAsync();
        
            if (model.PaginationValid())
                query = query.Skip((model.Page - 1) * model.Size).Take(model.Size);
        }

        foreach (var include in includes)
            query = query.Include(include);

        var data = await query.ToListAsync();
        return (data, total);
    }
    public async Task<int> GetCountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate is null
            ? await _set.CountAsync()
            : await _set.CountAsync(predicate);
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null)
            return false;

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public void Detach(T entity) => _context.Entry(entity).State = EntityState.Detached;
}
