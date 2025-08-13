using Application.Models.ReceiptDocument;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class ReceiptDocumentService(IRepository<ReceiptDocument> docs, BalanceService balance) : ModelService<ReceiptDocument, CreateReceiptDocumentDto, UpdateReceiptDocumentDto>(docs)
{
    public async override Task<(bool, ReceiptDocument?)> CreateAsync(CreateReceiptDocumentDto entity)
    {
        var model = Mapper.FromDTO<ReceiptDocument, CreateReceiptDocumentDto>(entity);
        var created = await repo.CreateAsync(model);

        if (created == null)
            return (false, null);

        var items = entity.Items.Select(b => (b.ResourceId, b.UnitId, b.Quantity));

        await balance.BulkUpdateBalancesAsync(items);

        return (true, (created));
    }
    public async override Task<(bool, ReceiptDocument?)> UpdateAsync(UpdateReceiptDocumentDto entity)
    {
        var existing = await repo.GetByIdAsync(entity.Id);
        if (existing == null)
            return (false, null);

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
            return (false, null);

        if (netChanges.Count != 0)
        {
            await balance.BulkUpdateBalancesAsync(netChanges);
        }

        return (true, (updated));
    }
    public async override Task<bool> DeleteAsync(int id)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null)
            return false;

        foreach (var item in existing.Items)
        {
            var hasStock = await balance.HasSufficientStockAsync(item.ResourceId, item.UnitId, item.Quantity);
            if (!hasStock)
            {
                return false;
            }
        }

        var deleted = await repo.DeleteAsync(id);
        if (!deleted)
            return false;

        var balanceReversals = existing.Items.Select(item => (item.ResourceId, item.UnitId, -item.Quantity));
        await balance.BulkUpdateBalancesAsync(balanceReversals);

        return true;
    }
}
