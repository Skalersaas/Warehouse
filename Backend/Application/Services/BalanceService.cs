using Application.Models.Balance;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class BalanceService(IRepository<Balance> repository) : ModelService<Balance, BalanceCreateDto, BalanceUpdateDto>(repository)
{
    private readonly IRepository<Balance> _repository = repository;

    public async Task<Balance?> GetBalanceAsync(int resourceId, int unitId)
    {
        var (balances, _) = await QueryBy(new SearchFilterModel
        {
            Filters = new Dictionary<string, string>
            {
                { nameof(Balance.ResourceId), resourceId.ToString() },
                { nameof(Balance.UnitId), unitId.ToString() }
            }
        });
        return balances.FirstOrDefault() ?? null;
    }
    public async Task<bool> HasSufficientStockAsync(int resourceId, int unitId, decimal requiredQuantity)
    {
        var balance = await GetBalanceAsync(resourceId, unitId);
        if (balance == null)
            return false;

        return balance.Quantity >= requiredQuantity;
    }

    public async Task UpdateBalanceAsync(int resourceId, int unitId, decimal quantityChange)
    {
        var existingBalance = await GetBalanceAsync(resourceId, unitId);

        if (existingBalance != null)
        {
            existingBalance.Quantity += quantityChange;
            
            if (existingBalance.Quantity <= 0)
            {
                await DeleteAsync(existingBalance.Id);
            }
            else
            {
                _repository.Detach(existingBalance);
                await UpdateAsync(Mapper.FromDTO<BalanceUpdateDto, Balance> (existingBalance));
            }
        }
        else if (quantityChange > 0)
        {
            var newBalance = new Balance
            {
                ResourceId = resourceId,
                UnitId = unitId,
                Quantity = quantityChange
            };
            await _repository.CreateAsync(newBalance);
        }
    }
    public async Task BulkUpdateBalancesAsync(IEnumerable<(int ResourceId, int UnitId, decimal QuantityChange)> changes)
    {
        foreach (var (ResourceId, UnitId, QuantityChange) in changes)
        {
            await UpdateBalanceAsync(ResourceId, UnitId, QuantityChange);
        }
    }
}