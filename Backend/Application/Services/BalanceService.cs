using Application.Models.Balance;
using Application.Services.Base;
using Domain.Models.Entities;
using Utilities.DataManipulation;
using Utilities.Responses;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class BalanceService(ApplicationContext repository, ILogger<BalanceService> logger) 
    : ModelService<Balance, BalanceCreateDto, BalanceUpdateDto>(repository, logger)
{
    /// <summary>
    /// Gets balance for a specific resource and unit combination.
    /// </summary>
    /// <param name="resourceId">The resource ID.</param>
    /// <param name="unitId">The unit ID.</param>
    /// <returns>The balance entity or null if not found.</returns>
    public async Task<Result<Balance>> GetBalanceAsync(int resourceId, int unitId)
    {
        try
        {
            var queryResult = await QueryBy(new SearchFilterModel
            {
                Filters = new Dictionary<string, string>
                {
                    { nameof(Balance.ResourceId), resourceId.ToString() },
                    { nameof(Balance.UnitId), unitId.ToString() }
                }
            },
            x => x.Include(x=>x.Resource).Include(x=>x.Unit));

            if (!queryResult.Success)
            {
                return Result<Balance>.ErrorResult("Error querying balance", queryResult.Errors);
            }

            var balance = queryResult.Data.list.FirstOrDefault();
            if (balance == null)
            {
                return Result<Balance>.ErrorResult($"Balance not found for ResourceId {resourceId} and UnitId {unitId}");
            }

            return Result<Balance>.SuccessResult(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for ResourceId {ResourceId} and UnitId {UnitId}", resourceId, unitId);
            return Result<Balance>.ErrorResult("An error occurred while retrieving balance");
        }
    }

    /// <summary>
    /// Checks if there is sufficient stock for a given resource and unit.
    /// </summary>
    /// <param name="resourceId">The resource ID.</param>
    /// <param name="unitId">The unit ID.</param>
    /// <param name="requiredQuantity">The required quantity.</param>
    /// <returns>True if sufficient stock exists, false otherwise.</returns>
    public async Task<Result<bool>> HasSufficientStockAsync(int resourceId, int unitId, decimal requiredQuantity)
    {
        try
        {
            var balanceResult = await GetBalanceAsync(resourceId, unitId);
            if (!balanceResult.Success)
            {
                return Result<bool>.SuccessResult(false, "No balance found - insufficient stock");
            }

            var hasSufficientStock = balanceResult.Data.Quantity >= requiredQuantity;
            return Result<bool>.SuccessResult(hasSufficientStock,
                hasSufficientStock ? "Sufficient stock available" : "Insufficient stock");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock for ResourceId {ResourceId}, UnitId {UnitId}, Required {RequiredQuantity}",
                resourceId, unitId, requiredQuantity);
            return Result<bool>.ErrorResult("An error occurred while checking stock");
        }
    }
    /// <summary>
    /// Checks if there is sufficient stock for a given resource and unit.
    /// </summary>
    /// <param name="resourceId">The resource ID.</param>
    /// <param name="unitId">The unit ID.</param>
    /// <param name="requiredQuantity">The required quantity.</param>
    /// <returns>True if sufficient stock exists, false otherwise.</returns>
    public async Task<Result<Balance>> CheckStockAsync(int resourceId, int unitId, decimal requiredQuantity)
    {
        try
        {
            var balanceResult = await GetBalanceAsync(resourceId, unitId);
            if (!balanceResult.Success)
            {
                return Result<Balance>.SuccessResult(null, "No balance found - insufficient stock");
            }

            var hasSufficientStock = balanceResult.Data.Quantity >= requiredQuantity;
            return hasSufficientStock
                ? Result<Balance>.SuccessResult(balanceResult.Data, "Sufficient stock available")
                : Result<Balance>.ErrorResult("Insufficient stock");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stock for ResourceId {ResourceId}, UnitId {UnitId}, Required {RequiredQuantity}",
                resourceId, unitId, requiredQuantity);
            return Result<Balance>.ErrorResult("An error occurred while checking stock");
        }
    }

    /// <summary>
    /// Updates the balance for a specific resource and unit.
    /// </summary>
    /// <param name="resourceId">The resource ID.</param>
    /// <param name="unitId">The unit ID.</param>
    /// <param name="quantityChange">The quantity change (positive for increase, negative for decrease).</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> UpdateBalanceAsync(int resourceId, int unitId, decimal quantityChange)
    {
        try
        {
            var balanceResult = await GetBalanceAsync(resourceId, unitId);

            if (balanceResult.Success)
            {
                // Update existing balance
                var existingBalance = balanceResult.Data;
                existingBalance.Quantity += quantityChange;

                if (existingBalance.Quantity <= 0)
                {
                    var deleteResult = await DeleteAsync(existingBalance.Id);
                    return deleteResult.Success 
                        ? Result.SuccessResult("Balance deleted as quantity reached zero")
                        : Result.ErrorResult("Failed to delete zero balance");
                }
                else
                {
                    var updateResult = await UpdateAsync(Mapper.AutoMap<BalanceUpdateDto, Balance>(existingBalance));
                    return updateResult.Success 
                        ? Result.SuccessResult("Balance updated successfully")
                        : Result.ErrorResult("Failed to update balance");
                }
            }
            else if (quantityChange > 0)
            {
                // Create new balance for positive quantity change
                var newBalance = new Balance
                {
                    ResourceId = resourceId,
                    UnitId = unitId,
                    Quantity = quantityChange
                };
                var created = _context.Balances.Add(newBalance);
                await _context.SaveChangesAsync();
                return created != null 
                    ? Result.SuccessResult("New balance created successfully")
                    : Result.ErrorResult("Failed to create new balance");
            }
            else
            {
                return Result.ErrorResult("Cannot create negative balance for non-existing record");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for ResourceId {ResourceId}, UnitId {UnitId}, Change {QuantityChange}", 
                resourceId, unitId, quantityChange);
            return Result.ErrorResult("An error occurred while updating balance");
        }
    }

    /// <summary>
    /// Updates multiple balances in a batch operation.
    /// </summary>
    /// <param name="changes">Collection of balance changes.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result> BulkUpdateBalancesAsync(IEnumerable<(int ResourceId, int UnitId, decimal QuantityChange)> changes)
    {
        try
        {
            var errors = new List<string>();
            var successCount = 0;

            foreach (var (ResourceId, UnitId, QuantityChange) in changes)
            {
                var result = await UpdateBalanceAsync(ResourceId, UnitId, QuantityChange);
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    errors.Add($"ResourceId {ResourceId}, UnitId {UnitId}: {result.Message}");
                }
            }

            if (errors.Count != 0)
            {
                return Result.ErrorResult($"Bulk update completed with {errors.Count} errors", errors);
            }

            return Result.SuccessResult($"Successfully updated {successCount} balances");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk balance update");
            return Result.ErrorResult("An error occurred during bulk balance update");
        }
    }
}
