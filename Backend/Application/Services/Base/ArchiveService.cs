using Application.Interfaces;
using Domain.Models.Interfaces;
using Persistence.Data.Interfaces;

namespace Application.Services.Base;

public class ArchiveService<TModel, TCreate, TUpdate>(IRepository<TModel> _context)
    : ModelService<TModel, TCreate, TUpdate>(_context), IArchiveService<TModel, TCreate, TUpdate>
    where TModel : class, IArchivable, IModel, new()
{
    protected readonly IRepository<TModel> _context = _context;
    public virtual async Task<bool> ArchiveAsync(int id)
    {
        return await SetArchiveStatusAsync(id, true);
    }

    public virtual async Task<bool> UnarchiveAsync(int id)
    {
        return await SetArchiveStatusAsync(id, false);
    }

    private async Task<bool> SetArchiveStatusAsync(int id, bool isArchived)
    {
        var found = await _context.GetByIdAsync(id);
        if (found == null)
            return false;

        (found as IArchivable)!.IsArchived = isArchived;

        try
        {
            var updated = await _context.UpdateAsync(found);
            return updated?.IsArchived == isArchived;
        }
        catch 
        {
            return false;
        }
    }
}
