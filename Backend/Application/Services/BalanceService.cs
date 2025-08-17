using Application.Models.Balance;
using Application.Services.Base;
using Domain.Models.Entities;
using Utilities.Responses;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class BalanceService(ApplicationContext context, ILogger<BalanceService> logger)
    : ModelService<Balance, BalanceCreateDto, BalanceUpdateDto>(context, logger)
{
    /// <summary>
    /// Gets balance for a specific resource and unit combination.
    /// </summary>
    public async Task<Result<Balance?>> GetBalanceAsync(int resourceId, int unitId)
    {
        try
        {
            if (resourceId <= 0 || unitId <= 0)
            {
                return Result<Balance?>.ErrorResult("ResourceId and UnitId must be positive values");
            }

            var balance = await repo.AsNoTracking()
                .Include(x => x.Resource)
                .Include(x => x.Unit)
                .FirstOrDefaultAsync(x => x.ResourceId == resourceId && x.UnitId == unitId);

            return Result<Balance?>.SuccessResult(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance for ResourceId {ResourceId} and UnitId {UnitId}",
                resourceId, unitId);
            return Result<Balance?>.ErrorResult("Failed to retrieve balance information");
        }
    }

    /// <summary>
    /// Validates if sufficient stock is available for export.
    /// </summary>
    public async Task<Result<(Balance balance, bool isAvailable)>> ValidateStockAsync(int resourceId, int unitId, decimal requiredQuantity)
    {
        try
        {
            if (resourceId <= 0 || unitId <= 0)
                return Result<(Balance, bool)>.ErrorResult("ResourceId and UnitId must be positive values");

            if (requiredQuantity <= 0)
                return Result<(Balance, bool)>.ErrorResult("Required quantity must be positive");

            var balanceResult = await GetBalanceAsync(resourceId, unitId);

            Balance balance;
            bool isAvailable;
            string message;

            if (balanceResult.Data == null)
            {
                balance = new Balance
                {
                    ResourceId = resourceId,
                    UnitId = unitId,
                    Quantity = 0,
                    Resource = await _context.Resources.FindAsync(resourceId)!,
                    Unit = await _context.Units.FindAsync(unitId)!
                };
                isAvailable = false;
                message = $"Insufficient stock - Available: 0, Required: {requiredQuantity}";
            }
            else
            {
                balance = balanceResult.Data;
                isAvailable = balance.Quantity >= requiredQuantity;
                message = isAvailable
                    ? "Sufficient stock available"
                    : $"Insufficient stock - Available: {balance.Quantity}, Required: {requiredQuantity}";
            }

            return Result<(Balance, bool)>.SuccessResult((balance, isAvailable), message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating stock for ResourceId {ResourceId}, UnitId {UnitId}, Required {RequiredQuantity}",
                resourceId, unitId, requiredQuantity);
            return Result<(Balance, bool)>.ErrorResult("Failed to validate stock availability");
        }
    }
    public async Task<Result> BulkUpdateAsync(List<(int ResourceId, int UnitId, decimal Quantity)> items)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (items.Count == 0)
            {
                return Result.ErrorResult("Document items cannot be empty");
            }

            // Validate all items first
            var validationErrors = new List<string>();
            foreach (var (resourceId, unitId, quantity) in items)
            {
                if (quantity >= 0)
                    continue;

                var validation = await ValidateStockAsync(resourceId, unitId, -quantity);

                if ((validation.Success && !validation.Data.isAvailable) || !validation.Success)

                    validationErrors.Add($"Resource: {validation.Data.balance.Resource.Name}, Unit: {validation.Data.balance.Unit.Name}: {validation.Message}");
            }

            if (validationErrors.Count != 0)
            {
                await transaction.RollbackAsync();
                return Result.ErrorResult("Stock validation failed", validationErrors);
            }


            foreach (var (resourceId, unitId, quantity) in items)
            {
                await UpdateBalanceAsync(resourceId, unitId, quantity);
            }

            await transaction.CommitAsync();
            return Result.SuccessResult($"Successfully processed {items.Count} items");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing document");
            return Result.ErrorResult("Failed to process document");
        }
    }
    private async Task<Result> UpdateBalanceAsync(int resourceId, int unitId, decimal quantity)
    {
        var existingBalance = await repo
            .FirstOrDefaultAsync(x => x.ResourceId == resourceId && x.UnitId == unitId);

        if (existingBalance != null)
        {
            existingBalance.Quantity += quantity;
        }
        else
        {
            var newBalance = new Balance
            {
                ResourceId = resourceId,
                UnitId = unitId,
                Quantity = quantity
            };
            await repo.AddAsync(newBalance);
        }

        await _context.SaveChangesAsync();
        return Result.SuccessResult("Balance increased successfully");
    }
}