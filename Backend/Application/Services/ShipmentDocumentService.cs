using Application.Models.ReceiptDocument;
using Application.Models.ShipmentDocument;
using Application.Services.Base;
using Domain.Models.Entities;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Services;

public class ShipmentDocumentService(IRepository<ShipmentDocument> repo, BalanceService balance) : ModelService<ShipmentDocument, CreateShipmentDocumentDto, UpdateShipmentDocumentDto>(repo)
{
    public async override Task<(bool, ShipmentDocument?)> CreateAsync(CreateShipmentDocumentDto entity)
    {
        var model = Mapper.FromDTO<ShipmentDocument, CreateShipmentDocumentDto>(entity);
        var created = await repo.CreateAsync(model);

        return created == null
            ? (false, null)
            : (true, created);
    }
    public virtual async Task<(bool, string)> Sign(int id)
    {
        var (success, doc) = await GetByIdAsync(id, includes: s => s.Items);

        if (!success)
            return (false, "Not found");


        var checks = await Task.WhenAll(doc.Items.Select(async item => new
        {
            Item = item,
            HasStock = await balance.HasSufficientStockAsync(item.ResourceId, item.UnitId, item.Quantity)
        }));

        var insufficient = checks.FirstOrDefault(c => !c.HasStock);
        if (insufficient != null)
            return (false, $"Insufficient stock for resource {insufficient.Item.ResourceId} in unit {insufficient.Item.UnitId}.");

        await balance.BulkUpdateBalancesAsync(checks.Select(i => (i.Item.ResourceId, i.Item.UnitId, -i.Item.Quantity)));

        return (true, "Document signed successfully.");
    }
    public virtual async Task<(bool, string)> Revoke(int id)
    {
        var (success, doc) = await GetByIdAsync(id, includes: s => s.Items);

        if (success)
            return (false, "Not found");

        await balance.BulkUpdateBalancesAsync(doc.Items.Select(i => (i.ResourceId, i.UnitId, i.Quantity)));

        return (true, "Document signed successfully.");
    }
}
