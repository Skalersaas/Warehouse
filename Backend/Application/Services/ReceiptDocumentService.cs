using Application.Models.ReceiptDocument;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;
using Utilities.Responses;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ReceiptDocumentService(IRepository<ReceiptDocument> docs, BalanceService balance, ILogger<ReceiptDocumentService> logger) 
    : ModelService<ReceiptDocument, CreateReceiptDocumentDto, UpdateReceiptDocumentDto>(docs, logger)
{
    private readonly BalanceService _balance = balance;

    public override async Task<Result<ReceiptDocument>> CreateAsync(CreateReceiptDocumentDto entity)
    {
        try
        {
            if (entity == null)
            {
                return Result<ReceiptDocument>.ErrorResult("Receipt document data cannot be null");
            }

            if (entity.Items == null || !entity.Items.Any())
            {
                return Result<ReceiptDocument>.ErrorResult("Receipt document must contain at least one item");
            }

            var model = Mapper.FromDTO<ReceiptDocument, CreateReceiptDocumentDto>(entity);
            var created = await repo.CreateAsync(model);

            if (created == null)
            {
                return Result<ReceiptDocument>.ErrorResult("Failed to create receipt document");
            }

            // Update balances for all items
            var items = entity.Items.Select(b => (b.ResourceId, b.UnitId, b.Quantity));
            var balanceResult = await _balance.BulkUpdateBalancesAsync(items);

            if (!balanceResult.Success)
            {
                _logger.LogWarning("Receipt document created but balance update failed: {Error}", balanceResult.Message);
                return Result<ReceiptDocument>.SuccessResult(created, 
                    "Receipt document created but balance update had issues: " + balanceResult.Message);
            }

            return Result<ReceiptDocument>.SuccessResult(created, "Receipt document created and balances updated successfully");
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
            if (entity == null)
            {
                return Result<ReceiptDocument>.ErrorResult("Receipt document data cannot be null");
            }

            var existing = await repo.GetByIdAsync(entity.Id);
            if (existing == null)
            {
                return Result<ReceiptDocument>.ErrorResult($"Receipt document with ID {entity.Id} not found");
            }

            // Calculate balance changes
            var existingBalances = existing.Items
                .GroupBy(item => new { item.ResourceId, item.UnitId })
                .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

            var newBalances = entity.Items
                .GroupBy(item => new { item.ResourceId, item.UnitId })
                .ToDictionary(g => g.Key, g => g.Sum(item => item.Quantity));

            var allKeys = existingBalances.Keys.Union(newBalances.Keys);
            var netChanges = allKeys
                .Select(key =>
                {
                    var existingQty = existingBalances.GetValueOrDefault(key, 0);
                    var newQty = newBalances.GetValueOrDefault(key, 0);
                    var netChange = newQty - existingQty;
                    return (key.ResourceId, key.UnitId, netChange);
                })
                .Where(change => change.netChange != 0)
                .ToList();

            var model = Mapper.FromDTO<ReceiptDocument, UpdateReceiptDocumentDto>(entity);
            repo.Detach(model);
            var updated = await repo.UpdateAsync(model);

            if (updated == null)
            {
                return Result<ReceiptDocument>.ErrorResult("Failed to update receipt document");
            }

            // Apply balance changes if any
            if (netChanges.Count != 0)
            {
                var balanceResult = await _balance.BulkUpdateBalancesAsync(netChanges);
                if (!balanceResult.Success)
                {
                    _logger.LogWarning("Receipt document updated but balance update failed: {Error}", balanceResult.Message);
                    return Result<ReceiptDocument>.SuccessResult(updated, 
                        "Receipt document updated but balance update had issues: " + balanceResult.Message);
                }
            }

            return Result<ReceiptDocument>.SuccessResult(updated, "Receipt document updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receipt document with ID {Id}", entity?.Id);
            return Result<ReceiptDocument>.ErrorResult("An error occurred while updating the receipt document");
        }
    }
    public override async Task<Result<(IEnumerable<ReceiptDocument>, int)>> QueryBy(SearchModel model)
    {
        try
        {
            if (model == null)
            {
                return Result<(IEnumerable<ReceiptDocument>, int)>.ErrorResult("Search model cannot be null");
            }

            var (data, fullCount) = await repo.QueryBy(model, i => i.Items);

            return Result<(IEnumerable<ReceiptDocument>, int)>.SuccessResult((data, fullCount),
                count: fullCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying receiptDocuments");
            return Result<(IEnumerable<ReceiptDocument>, int)>.ErrorResult("An error occurred while searching entities");
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

            var existing = await repo.GetByIdAsync(id);
            if (existing == null)
            {
                return Result.ErrorResult($"Receipt document with ID {id} not found");
            }

            // Check if we have sufficient stock to reverse the operations
            foreach (var item in existing.Items)
            {
                var stockResult = await _balance.HasSufficientStockAsync(item.ResourceId, item.UnitId, item.Quantity);
                if (!stockResult.Success || !stockResult.Data)
                {
                    return Result.ErrorResult($"Insufficient stock to reverse item: ResourceId {item.ResourceId}, UnitId {item.UnitId}. Cannot delete receipt document.");
                }
            }

            var deleted = await repo.DeleteAsync(id);
            if (!deleted)
            {
                return Result.ErrorResult("Failed to delete receipt document");
            }

            // Reverse the balance changes
            var balanceReversals = existing.Items.Select(item => (item.ResourceId, item.UnitId, -item.Quantity));
            var balanceResult = await _balance.BulkUpdateBalancesAsync(balanceReversals);

            if (!balanceResult.Success)
            {
                _logger.LogError("Receipt document deleted but balance reversal failed: {Error}", balanceResult.Message);
                return Result.ErrorResult("Receipt document deleted but balance reversal failed");
            }

            return Result.SuccessResult("Receipt document deleted and balances updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receipt document with ID {Id}", id);
            return Result.ErrorResult("An error occurred while deleting the receipt document");
        }
    }
}
