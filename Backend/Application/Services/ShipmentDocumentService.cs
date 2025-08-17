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

            var model = Mapper.AutoMap<ShipmentDocument, CreateShipmentDocumentDto>(entity);

            var created = _context.ShipmentDocuments.Add(model);
            await _context.SaveChangesAsync();

            if (created.Entity == null)
                return Result<ShipmentDocument>.ErrorResult("Failed to create shipment document");
            
            return created.Entity == null
                ?  Result<ShipmentDocument>.ErrorResult("Failed to create shipment document")
                :  Result<ShipmentDocument>.SuccessResult(created.Entity, "Shipment document created successfully");
        }
        catch (DbUpdateException)
        {
            return Result<ShipmentDocument>.ErrorResult("Document number cannot be repetitive");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shipment document");
            return Result<ShipmentDocument>.ErrorResult("An error occurred while creating the shipment document");
        }
    }
    public override async Task<Result<ShipmentDocument>> UpdateAsync(UpdateShipmentDocumentDto entity)
    {
        try
        {
            // Consolidated validation
            var validationResult = ValidateUpdateRequest(entity);
            if (!validationResult.Success)
                return Result<ShipmentDocument>.ErrorResult(validationResult.Message);

            var existing = await _context.ShipmentDocuments
                .Include(r => r.Client)
                .Include(r => r.Items)
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existing == null)
            {
                return Result<ShipmentDocument>.ErrorResult($"Shipment document with ID {entity.Id} not found");
            }
            else if (existing.Status == ShipmentStatus.Signed)
                return Result<ShipmentDocument>.ErrorResult($"Signed shipment document cannot be changed");

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
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == entity.ClientId);
            existing.Client = client;
            Mapper.AutoMapToExisting(entity, existing);
            await _context.SaveChangesAsync();

            return Result<ShipmentDocument>.SuccessResult(existing, "Shipment document updated successfully");
        }
        catch (DbUpdateException)
        {
            return Result<ShipmentDocument>.ErrorResult("Cannot update this entity because it violates foreign key");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Shipment document with ID {Id}", entity?.Id);
            return Result<ShipmentDocument>.ErrorResult("An error occurred while updating the Shipment document");
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
            if (!doc.Success) return Result<ShipmentDocument>.ErrorResult(doc.Message, doc.Errors);


            var balanceUpdate = await ProcessShipmentBalanceChanges(doc.Data.Items);
            if (!balanceUpdate.Success) return Result<ShipmentDocument>.ErrorResult(balanceUpdate.Message, balanceUpdate.Errors);

            return await SignDocumentAndUpdate(doc.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing shipment document with ID {Id}", id);
            return Result<ShipmentDocument>.ErrorResult("An error occurred while signing the document");
        }
    }

    public override async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var doc = await _context.ShipmentDocuments.FirstOrDefaultAsync(x => x.Id == id);

            if (doc == null) return Result.ErrorResult("Shipment document not found");
            return (doc.Status == ShipmentStatus.Signed)
                ? Result.ErrorResult("Signed document cannot be deleted")
                : Result.SuccessResult("Shipment document deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shipment document with ID {Id}", id);
            return Result.ErrorResult("An error occurred while deleting the document");
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
    private static Result ValidateUpdateRequest(UpdateShipmentDocumentDto entity)
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

        var docResult = await GetByIdAsync(id, s => s.Include(d => d.Items));
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

        var docResult = await GetByIdAsync(id, s => s.Include(d => d.Items));
        if (!docResult.Success) return Result<ShipmentDocument>.ErrorResult($"Document {id} not found");

        var doc = docResult.Data;
        return doc.Status switch
        {
            ShipmentStatus.Draft => Result<ShipmentDocument>.ErrorResult("Cannot revoke unsigned document"),
            _ when doc.Items?.Count == 0 => Result<ShipmentDocument>.ErrorResult("No items to process"),
            _ => Result<ShipmentDocument>.SuccessResult(doc)
        };
    }
    private async Task<Result> ProcessShipmentBalanceChanges(ICollection<ShipmentItem> items)
    {
        var changes = items.Select(i => (i.ResourceId, i.UnitId, -i.Quantity)).ToList();
        var result = await _balance.BulkUpdateAsync(changes);

        return result.Success
            ? Result.SuccessResult()
            : Result.ErrorResult($"Failed to update balances: {result.Message}", result.Errors);
    }

    private async Task<Result> RestoreBalances(ICollection<ShipmentItem> items)
    {
        var restorations = items.Select(i => (i.ResourceId, i.UnitId, i.Quantity)).ToList();
        var result = await _balance.BulkUpdateAsync(restorations);

        return result.Success
            ? Result.SuccessResult()
            : Result.ErrorResult($"Failed to restore balances: {result.Message}", result.Errors);
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
        Mapper.AutoMapToExisting(doc, found);
        await _context.SaveChangesAsync();

        return Result<ShipmentDocument>.SuccessResult(found, successMessage);
    }
    #endregion
}