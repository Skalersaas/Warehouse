using Domain.Models.Interfaces;
using Persistence.Data.Interfaces;

namespace Persistence.Data.Repositories;

public class ArchivableRepository<T>(ApplicationContext _context) : Repository<T>(_context), IArchivableRepository<T>
    where T : class, IModel, IArchivable
{
    protected readonly ApplicationContext _context = _context;
    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null) return false;

        entity.IsArchived = true;
        _set.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> HardDeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null) return false;

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> RestoreAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null) return false;

        entity.IsArchived = false;
        _set.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
