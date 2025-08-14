using Application.Interfaces;
using Domain.Models.Interfaces;
using Persistence.Data.Interfaces;
using Utilities.Responses;
using Microsoft.Extensions.Logging;

namespace Application.Services.Base;

public class ArchiveService<TModel, TCreate, TUpdate>(IRepository<TModel> _context, ILogger<ArchiveService<TModel, TCreate, TUpdate>> logger)
    : ModelService<TModel, TCreate, TUpdate>(_context, logger), IArchiveService<TModel, TCreate, TUpdate>
    where TModel : class, IArchivable, IModel, new()
{
    protected readonly IRepository<TModel> _context = _context;

    public virtual async Task<Result> ArchiveAsync(int id)
    {
        return await SetArchiveStatusAsync(id, true, "Entity archived successfully");
    }

    public virtual async Task<Result> UnarchiveAsync(int id)
    {
        return await SetArchiveStatusAsync(id, false, "Entity unarchived successfully");
    }

    private async Task<Result> SetArchiveStatusAsync(int id, bool isArchived, string successMessage)
    {
        try
        {
            if (id <= 0)
            {
                return Result.ErrorResult("Invalid ID provided");
            }

            var found = await _context.GetByIdAsync(id);
            if (found == null)
            {
                return Result.ErrorResult($"Entity with ID {id} not found");
            }
            
            var foundArchivable = (found as IArchivable);
            if (foundArchivable.IsArchived == isArchived)
                return Result.ErrorResult($"Entity with ID {id} is already {(isArchived ? "archived" : "unarchived")}");


            foundArchivable!.IsArchived = isArchived;

            var updated = await _context.UpdateAsync(found);
            if (updated?.IsArchived != isArchived)
            {
                return Result.ErrorResult("Failed to update archive status");
            }

            return Result.SuccessResult(successMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting archive status for entity with ID {Id} of type {EntityType}", id, typeof(TModel).Name);
            return Result.ErrorResult("An error occurred while updating archive status");
        }
    }
}
