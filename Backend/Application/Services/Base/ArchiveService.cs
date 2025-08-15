using Application.Interfaces;
using Domain.Models.Interfaces;
using Utilities.Responses;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Base;

public class ArchiveService<TModel, TCreate, TUpdate>(ApplicationContext _context, ILogger<ArchiveService<TModel, TCreate, TUpdate>> logger)
    : ModelService<TModel, TCreate, TUpdate>(_context, logger), IArchiveService<TModel, TCreate, TUpdate>
    where TModel : class, IArchivable, IModel, new()
    where TUpdate : IModel
{
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

            var entity = await repo.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
            {
                return Result.ErrorResult($"Entity with ID {id} not found");
            }

            if (entity is not IArchivable archivableEntity)
            {
                return Result.ErrorResult("Entity does not support archiving");
            }

            if (archivableEntity.IsArchived == isArchived)
            {
                var currentStatus = isArchived ? "archived" : "active";
                return Result.ErrorResult($"Entity is already {currentStatus}");
            }

            archivableEntity.IsArchived = isArchived;

            repo.Update(entity);
            await _context.SaveChangesAsync();

            return Result.SuccessResult(successMessage);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating archive status for entity {EntityType} with ID {Id}",
                typeof(TModel).Name, id);
            return Result.ErrorResult("Database error occurred while updating archive status");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting archive status for entity {EntityType} with ID {Id} to {IsArchived}",
                typeof(TModel).Name, id, isArchived);
            return Result.ErrorResult("An error occurred while updating archive status");
        }
    }
}
