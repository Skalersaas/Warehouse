using Application.DTOs.Balance;
using Application.Interfaces;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class BalanceService : IBalanceService
{
    private readonly IRepository<Balance> _balanceRepository;
    private readonly ApplicationContext _context;
    private readonly ILogger<BalanceService> _logger;

    public BalanceService(IRepository<Balance> balanceRepository, ApplicationContext context, ILogger<BalanceService> logger)
    {
        _balanceRepository = balanceRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<(IEnumerable<BalanceResponseDto> Data, int TotalCount)> GetWarehouseBalanceAsync(WarehouseFilterModel? filter = null)
    {
        try
        {
            var query = _context.Balances
                .Include(b => b.Resource)
                .Include(b => b.Unit)
                .AsQueryable();

            // Apply filters
            if (filter != null)
            {
                if (filter.ResourceIds?.Any() == true)
                    query = query.Where(b => filter.ResourceIds.Contains(b.ResourceId));

                if (filter.UnitIds?.Any() == true)
                    query = query.Where(b => filter.UnitIds.Contains(b.UnitId));

                if (filter.MinQuantity.HasValue)
                    query = query.Where(b => b.Quantity >= filter.MinQuantity.Value);

                if (filter.MaxQuantity.HasValue)
                    query = query.Where(b => b.Quantity <= filter.MaxQuantity.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(b => 
                        b.Resource.Name.ToLower().Contains(searchTerm) ||
                        b.Unit.Name.ToLower().Contains(searchTerm));
                }

                // Apply sorting
                query = ApplySorting(query, filter);

                // Get total count before pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                if (filter.PaginationValid())
                {
                    query = query.Skip((filter.Page - 1) * filter.Size).Take(filter.Size);
                }

                var balances = await query.ToListAsync();
                var result = balances.Select(MapToResponseDto);

                return (result, totalCount);
            }

            var allBalances = await query.ToListAsync();
            var allResult = allBalances.Select(MapToResponseDto);
            return (allResult, allBalances.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse balance");
            return (Enumerable.Empty<BalanceResponseDto>(), 0);
        }
    }

    public async Task<bool> HasSufficientBalanceAsync(int resourceId, int unitId, decimal requiredQuantity)
    {
        try
        {
            var currentBalance = await GetCurrentBalanceAsync(resourceId, unitId);
            return currentBalance >= requiredQuantity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking balance for Resource: {ResourceId}, Unit: {UnitId}, Required: {Quantity}", 
                resourceId, unitId, requiredQuantity);
            return false;
        }
    }

    public async Task<decimal> GetCurrentBalanceAsync(int resourceId, int unitId)
    {
        try
        {
            var balance = await _context.Balances
                .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitId == unitId);

            return balance?.Quantity ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current balance for Resource: {ResourceId}, Unit: {UnitId}", 
                resourceId, unitId);
            return 0;
        }
    }

    public async Task UpdateBalanceOnReceiptAsync(ReceiptDocument receipt)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in receipt.Items)
            {
                await UpdateBalanceQuantity(item.ResourceId, item.UnitId, item.Quantity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Balance updated on receipt creation: {ReceiptId}", receipt.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating balance on receipt creation: {ReceiptId}", receipt.Id);
            throw;
        }
    }

    public async Task UpdateBalanceOnReceiptUpdateAsync(ReceiptDocument oldReceipt, ReceiptDocument newReceipt)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Revert old receipt balances
            foreach (var item in oldReceipt.Items)
            {
                await UpdateBalanceQuantity(item.ResourceId, item.UnitId, -item.Quantity);
            }

            // Apply new receipt balances
            foreach (var item in newReceipt.Items)
            {
                await UpdateBalanceQuantity(item.ResourceId, item.UnitId, item.Quantity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Balance updated on receipt update: {ReceiptId}", newReceipt.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating balance on receipt update: {ReceiptId}", newReceipt.Id);
            throw;
        }
    }

    public async Task UpdateBalanceOnReceiptDeleteAsync(ReceiptDocument receipt)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in receipt.Items)
            {
                await UpdateBalanceQuantity(item.ResourceId, item.UnitId, -item.Quantity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Balance updated on receipt deletion: {ReceiptId}", receipt.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating balance on receipt deletion: {ReceiptId}", receipt.Id);
            throw;
        }
    }

    public async Task UpdateBalanceOnShipmentSignAsync(ShipmentDocument shipment)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Validate balance before updating
            await ValidateShipmentBalanceAsync(shipment);

            foreach (var item in shipment.Items)
            {
                await UpdateBalanceQuantity(item.ResourceId, item.UnitId, -item.Quantity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Balance updated on shipment signing: {ShipmentId}", shipment.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating balance on shipment signing: {ShipmentId}", shipment.Id);
            throw;
        }
    }

    public async Task UpdateBalanceOnShipmentRevokeAsync(ShipmentDocument shipment)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in shipment.Items)
            {
                await UpdateBalanceQuantity(item.ResourceId, item.UnitId, item.Quantity);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Balance updated on shipment revoke: {ShipmentId}", shipment.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating balance on shipment revoke: {ShipmentId}", shipment.Id);
            throw;
        }
    }

    public async Task ValidateShipmentBalanceAsync(ShipmentDocument shipment)
    {
        var insufficientItems = new List<string>();

        foreach (var item in shipment.Items)
        {
            var currentBalance = await GetCurrentBalanceAsync(item.ResourceId, item.UnitId);
            if (currentBalance < item.Quantity)
            {
                var resource = await _context.Resources.FindAsync(item.ResourceId);
                var unit = await _context.Units.FindAsync(item.UnitId);
                insufficientItems.Add($"{resource?.Name} ({unit?.Name}): required {item.Quantity}, available {currentBalance}");
            }
        }

        if (insufficientItems.Any())
        {
            var errorMessage = $"Insufficient balance for items: {string.Join(", ", insufficientItems)}";
            _logger.LogWarning("Balance validation failed for shipment {ShipmentId}: {Error}", shipment.Id, errorMessage);
            throw new InvalidOperationException(errorMessage);
        }
    }

    private async Task UpdateBalanceQuantity(int resourceId, int unitId, decimal quantityChange)
    {
        var balance = await _context.Balances
            .FirstOrDefaultAsync(b => b.ResourceId == resourceId && b.UnitId == unitId);

        if (balance == null)
        {
            // Create new balance if it doesn't exist
            balance = new Balance
            {
                ResourceId = resourceId,
                UnitId = unitId,
                Quantity = Math.Max(0, quantityChange) // Ensure no negative balances
            };
            _context.Balances.Add(balance);
        }
        else
        {
            balance.Quantity += quantityChange;
            
            // Ensure balance never goes negative
            if (balance.Quantity < 0)
            {
                var resource = await _context.Resources.FindAsync(resourceId);
                var unit = await _context.Units.FindAsync(unitId);
                throw new InvalidOperationException(
                    $"Insufficient balance for {resource?.Name} ({unit?.Name}). " +
                    $"Current: {balance.Quantity - quantityChange}, Required: {Math.Abs(quantityChange)}");
            }

            // Remove balance record if quantity becomes zero
            if (balance.Quantity == 0)
            {
                _context.Balances.Remove(balance);
            }
        }
    }

    private static IQueryable<Balance> ApplySorting(IQueryable<Balance> query, WarehouseFilterModel filter)
    {
        return filter.SortedField?.ToLower() switch
        {
            "resourcename" => filter.IsAscending 
                ? query.OrderBy(b => b.Resource.Name) 
                : query.OrderByDescending(b => b.Resource.Name),
            "unitname" => filter.IsAscending 
                ? query.OrderBy(b => b.Unit.Name) 
                : query.OrderByDescending(b => b.Unit.Name),
            "quantity" => filter.IsAscending 
                ? query.OrderBy(b => b.Quantity) 
                : query.OrderByDescending(b => b.Quantity),
            _ => filter.IsAscending 
                ? query.OrderBy(b => b.Id) 
                : query.OrderByDescending(b => b.Id)
        };
    }

    private static BalanceResponseDto MapToResponseDto(Balance balance)
    {
        return new BalanceResponseDto
        {
            Id = balance.Id,
            ResourceId = balance.ResourceId,
            ResourceName = balance.Resource?.Name ?? string.Empty,
            UnitId = balance.UnitId,
            UnitName = balance.Unit?.Name ?? string.Empty,
            Quantity = balance.Quantity
        };
    }
}
