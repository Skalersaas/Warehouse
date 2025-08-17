using Application.Models.ReceiptDocument;
using Application.Models.ReceiptItem;
using Application.Services.Base;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Services;

public class ReceiptDocumentService(ApplicationContext docs, BalanceService balance, ILogger<ReceiptDocumentService> logger)
    : ModelService<ReceiptDocument, CreateReceiptDocumentDto, UpdateReceiptDocumentDto>(docs, logger)
{
    private readonly BalanceService _balance = balance;

    public override async Task<Result<ReceiptDocument>> CreateAsync(CreateReceiptDocumentDto entity)
    {
        try
        {
            // Consolidated validation
            var validationResult = ValidateCreateRequest(entity);
            if (!validationResult.Success)
                return Result<ReceiptDocument>.ErrorResult(validationResult.Message);

            // Validate items using helper (no client validation for receipts)
            var itemsValidation = await DocumentValidationHelper.ValidateItemsAsync(
                _context,
                entity.Items,
                item => item.ResourceId,
                item => item.UnitId,
                item => item.Quantity
            );
            if (!itemsValidation.Success)
                return Result<ReceiptDocument>.ErrorResult(itemsValidation.Message);

            var model = Mapper.AutoMap<ReceiptDocument, CreateReceiptDocumentDto>(entity);
            var created = _context.ReceiptDocuments.Add(model);
            await _context.SaveChangesAsync();

            if (created.Entity == null)
            {
                return Result<ReceiptDocument>.ErrorResult("Failed to create receipt document");
            }

            // Update balances for all items
            var items = entity.Items.Select(b => (b.ResourceId, b.UnitId, b.Quantity)).ToList();
            var balanceResult = await _balance.BulkUpdateAsync(items);

            if (!balanceResult.Success)
            {
                _logger.LogWarning("Receipt document created but balance update failed: {Error}", balanceResult.Message);
                return Result<ReceiptDocument>.SuccessResult(created.Entity,
                    "Receipt document was not created: " + balanceResult.Message);
            }

            return Result<ReceiptDocument>.SuccessResult(created.Entity, "Receipt document created and balances updated successfully");
        }

        catch (DbUpdateException)
        {
            return Result<ReceiptDocument>.ErrorResult("Document number cannot be repetitive");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receipt document");
            return Result<ReceiptDocument>.ErrorResult("An error occurred while creating the receipt document");
        }
    }

    public override async Task<Result<ReceiptDocument>> UpdateAsync(UpdateReceiptDocumentDto entity)
    {
        try
        {
            // Consolidated validation
            var validationResult = ValidateUpdateRequest(entity);
            if (!validationResult.Success)
                return Result<ReceiptDocument>.ErrorResult(validationResult.Message);

            var existing = await _context.ReceiptDocuments
                .Include(r => r.Items)
                .FirstOrDefaultAsync(x => x.Id == entity.Id);
            if (existing == null)
            {
                return Result<ReceiptDocument>.ErrorResult($"Receipt document with ID {entity.Id} not found");
            }

            // Validate items using helper
            var itemsValidation = await DocumentValidationHelper.ValidateItemsAsync(
                _context,
                entity.Items,
                item => item.ResourceId,
                item => item.UnitId,
                item => item.Quantity
            );
            if (!itemsValidation.Success)
                return Result<ReceiptDocument>.ErrorResult(itemsValidation.Message);

            // Calculate balance changes
            var netChanges = CalculateBalanceChanges(existing.Items, entity.Items);


            // Apply balance changes if any
            if (netChanges.Count != 0)
            {
                var balanceResult = await _balance.BulkUpdateAsync(netChanges);
                if (!balanceResult.Success)
                {
                    _logger.LogWarning("Balance update failed: {Error}", balanceResult.Message);
                    return Result<ReceiptDocument>.ErrorResult("Balance update had issues: " + balanceResult.Message);
                }
            }

            Mapper.AutoMapToExisting(entity, existing);
            await _context.SaveChangesAsync();
            return Result<ReceiptDocument>.SuccessResult(existing, "Receipt document updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt document with ID {Id}", entity?.Id);
            return Result<ReceiptDocument>.ErrorResult("An error occurred while updating the receipt document");
        }
    }
    public override async Task<Result> DeleteAsync(int id)
    {
        try
        {
            if (id <= 0)
            {
                return Result.ErrorResult("Invalid ID provided");
            }

            var existing = await _context.ReceiptDocuments
                .Include(r => r.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
            {
                return Result.ErrorResult($"Receipt document with ID {id} not found");
            }

            // Check if we have sufficient stock to reverse the operations
            var netChanges = existing.Items.Select(x => (x.ResourceId, x.UnitId, -x.Quantity)).ToList();
            var stockValidation = await _balance.BulkUpdateAsync(netChanges);
            if (!stockValidation.Success)
                return stockValidation;

            _context.ReceiptDocuments.Remove(existing);
            await _context.SaveChangesAsync();

            return Result.SuccessResult("Receipt document deleted and balances updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt document with ID {Id}", id);
            return Result.ErrorResult("An error occurred while deleting the receipt document");
        }
    }

    #region Private Methods
    private static Result ValidateCreateRequest(CreateReceiptDocumentDto entity)
    {
        return entity switch
        {
            null => Result.ErrorResult("Receipt document data cannot be null"),
            { Items: null or { Count: 0 } } => Result.ErrorResult("Receipt document must contain at least one item"),
            _ => Result.SuccessResult()
        };
    }

    private static Result ValidateUpdateRequest(UpdateReceiptDocumentDto entity)
    {
        return entity switch
        {
            null => Result.ErrorResult("Receipt document data cannot be null"),
            { Id: <= 0 } => Result.ErrorResult("Invalid receipt document ID"),
            { Items: null or { Count : 0 } } => Result.ErrorResult("Receipt document must contain at least one item"),
            _ => Result.SuccessResult()
        };
    }

    private static List<(int ResourceId, int UnitId, decimal Quantity)> CalculateBalanceChanges(
        ICollection<ReceiptItem> existingItems,
        IEnumerable<UpdateReceiptItemDto> newItems)
    {
        var existingBalances = existingItems
            .GroupBy(item => new { item.ResourceId, item.UnitId })
            .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

        var newBalances = newItems
            .GroupBy(item => new { item.ResourceId, item.UnitId })
            .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

        var allKeys = existingBalances.Keys.Union(newBalances.Keys);

        return allKeys
            .Select(key =>
            {
                var existingQty = existingBalances.GetValueOrDefault(key, 0);
                var newQty = newBalances.GetValueOrDefault(key, 0);
                var netChange = newQty - existingQty;
                return (key.ResourceId, key.UnitId, netChange);
            })
            .Where(change => change.netChange != 0)
            .ToList();
    }

    #endregion
}