using Application.Models.ShipmentDocument;
using Application.Services.Base;
using Domain.Models.Entities;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Utilities.DataManipulation;
using Utilities.Responses;

namespace Application.Services;

public class ShipmentDocumentService(ApplicationContext repo, BalanceService balance, ClientService clients, ILogger<ShipmentDocumentService> logger)
    : ModelService<ShipmentDocument, CreateShipmentDocumentDto, UpdateShipmentDocumentDto>(repo, logger)
{
    private readonly BalanceService _balance = balance;

    public override async Task<Result<ShipmentDocument>> CreateAsync(CreateShipmentDocumentDto entity)
    {
        try
        {
            // Consolidated validation
            var validationResult = ValidateCreateRequest(entity);
            if (!validationResult.Success)
                return Result<ShipmentDocument>.ErrorResult(validationResult.Message);

            // Validate client using helper
            var clientValidation = await DocumentValidationHelper.ValidateClientAsync(_context, entity.ClientId);
            if (!clientValidation.Success)
                return Result<ShipmentDocument>.ErrorResult(clientValidation.Message);

            // Validate items using helper
            var itemsValidation = await DocumentValidationHelper.ValidateItemsAsync(
                _context,
                entity.Items,
                item => item.ResourceId,
                item => item.UnitId,
                item => item.Quantity
            );
            if (!itemsValidation.Success)
                return Result<ShipmentDocument>.ErrorResult(itemsValidation.Message);

            var result = await base.CreateAsync(entity);

            return result.Success
                ? Result<ShipmentDocument>.SuccessResult(result.Data, "Shipment document created successfully")
                : Result<ShipmentDocument>.ErrorResult("Failed to create shipment document");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment document");
            return Result<ShipmentDocument>.ErrorResult("An error occurred while creating the shipment document");
        }
    }

    /// <summary>
    /// Signs a shipment document, checking stock availability and updating balances.
    /// </summary>
    public virtual async Task<Result<ShipmentDocument>> Sign(int id)
    {
        try
        {
            // Chain operations with early returns
            var doc = await ValidateAndGetDocumentForSigning(id);
            if (!doc.Success) return Result<ShipmentDocument>.ErrorResult(doc.Message);

            var stockValidation = await ValidateStockAvailability(doc.Data.Items);
            if (!stockValidation.Success) return Result<ShipmentDocument>.ErrorResult(stockValidation.Message);

            var balanceUpdate = await ProcessShipmentBalanceChanges(doc.Data.Items);
            if (!balanceUpdate.Success) return Result<ShipmentDocument>.ErrorResult(balanceUpdate.Message);

            return await SignDocumentAndUpdate(doc.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing shipment document with ID {Id}", id);
            return Result<ShipmentDocument>.ErrorResult("An error occurred while signing the document");
        }
    }

    /// <summary>
    /// Revokes a signed shipment document, restoring the balances.
    /// </summary>
    public virtual async Task<Result<ShipmentDocument>> Revoke(int id)
    {
        try
        {
            var doc = await ValidateAndGetDocumentForRevocation(id);
            if (!doc.Success) return Result<ShipmentDocument>.ErrorResult(doc.Message);

            var balanceRestore = await RestoreBalances(doc.Data.Items);
            if (!balanceRestore.Success) return Result<ShipmentDocument>.ErrorResult(balanceRestore.Message);

            return await RevokeDocumentAndUpdate(doc.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking shipment document with ID {Id}", id);
            return Result<ShipmentDocument>.ErrorResult("An error occurred while revoking the document");
        }
    }

    #region Private Methods
    private static Result ValidateCreateRequest(CreateShipmentDocumentDto entity)
    {
        return entity switch
        {
            null => Result.ErrorResult("Shipment document data cannot be null"),
            { Items: null or { Count: 0 } } => Result.ErrorResult("Shipment document must contain at least one item"),
            _ => Result.SuccessResult()
        };
    }

    private async Task<Result<ShipmentDocument>> ValidateAndGetDocumentForSigning(int id)
    {
        if (id <= 0) return Result<ShipmentDocument>.ErrorResult("Invalid document ID");

        var docResult = await GetByIdAsync(id, s => s.Items);
        if (!docResult.Success) return Result<ShipmentDocument>.ErrorResult($"Document {id} not found");

        var doc = docResult.Data;
        return doc.Status switch
        {
            ShipmentStatus.Signed => Result<ShipmentDocument>.ErrorResult("Document already signed"),
            _ when doc.Items?.Count == 0 => Result<ShipmentDocument>.ErrorResult("No items to process"),
            _ => Result<ShipmentDocument>.SuccessResult(doc)
        };
    }

    private async Task<Result<ShipmentDocument>> ValidateAndGetDocumentForRevocation(int id)
    {
        if (id <= 0) return Result<ShipmentDocument>.ErrorResult("Invalid document ID");

        var docResult = await GetByIdAsync(id, s => s.Items);
        if (!docResult.Success) return Result<ShipmentDocument>.ErrorResult($"Document {id} not found");

        var doc = docResult.Data;
        return doc.Status switch
        {
            ShipmentStatus.Draft => Result<ShipmentDocument>.ErrorResult("Cannot revoke unsigned document"),
            _ when doc.Items?.Count == 0 => Result<ShipmentDocument>.ErrorResult("No items to process"),
            _ => Result<ShipmentDocument>.SuccessResult(doc)
        };
    }

    private async Task<Result> ValidateStockAvailability(ICollection<ShipmentItem> items)
    {
        // Use parallel processing for stock checks if items are many
        if (items.Count > 10)
        {
            var stockTasks = items.Select(async item =>
            {
                var hasStock = await _balance.HasSufficientStockAsync(item.ResourceId, item.UnitId, item.Quantity);
                return new { item, hasStock };
            });

            var results = await Task.WhenAll(stockTasks);
            var insufficient = results.FirstOrDefault(r => !r.hasStock.Success || !r.hasStock.Data);

            return insufficient != null
                ? Result.ErrorResult($"Insufficient stock: Resource {insufficient.item.ResourceId}, Unit {insufficient.item.UnitId}, Required: {insufficient.item.Quantity}")
                : Result.SuccessResult();
        }

        // Sequential processing for smaller collections
        foreach (var item in items)
        {
            var hasStock = await _balance.HasSufficientStockAsync(item.ResourceId, item.UnitId, item.Quantity);
            if (!hasStock.Success || !hasStock.Data)
                return Result.ErrorResult($"Insufficient stock: Resource {item.ResourceId}, Unit {item.UnitId}, Required: {item.Quantity}");
        }

        return Result.SuccessResult();
    }

    private async Task<Result> ProcessShipmentBalanceChanges(ICollection<ShipmentItem> items)
    {
        var changes = items.Select(i => (i.ResourceId, i.UnitId, -i.Quantity));
        var result = await _balance.BulkUpdateBalancesAsync(changes);

        return result.Success
            ? Result.SuccessResult()
            : Result.ErrorResult($"Failed to update balances: {result.Message}");
    }

    private async Task<Result> RestoreBalances(ICollection<ShipmentItem> items)
    {
        var restorations = items.Select(i => (i.ResourceId, i.UnitId, i.Quantity));
        var result = await _balance.BulkUpdateBalancesAsync(restorations);

        return result.Success
            ? Result.SuccessResult()
            : Result.ErrorResult($"Failed to restore balances: {result.Message}");
    }

    private async Task<Result<ShipmentDocument>> SignDocumentAndUpdate(ShipmentDocument doc)
    {
        return await UpdateDocumentStatus(doc, ShipmentStatus.Signed, "Document signed successfully");
    }

    private async Task<Result<ShipmentDocument>> RevokeDocumentAndUpdate(ShipmentDocument doc)
    {
        return await UpdateDocumentStatus(doc, ShipmentStatus.Draft, "Document revoked successfully");
    }

    private async Task<Result<ShipmentDocument>> UpdateDocumentStatus(ShipmentDocument doc, ShipmentStatus status, string successMessage)
    {
        doc.Status = status;
        var found = await repo.FirstOrDefaultAsync(x => x.Id == doc.Id);

        if (found == null)
        {
            return Result<ShipmentDocument>.ErrorResult("Entity not found");
        }
        Mapper.MapToExisting(doc, found);
        await _context.SaveChangesAsync();

        return Result<ShipmentDocument>.SuccessResult(found, successMessage);
    }
    #endregion
}