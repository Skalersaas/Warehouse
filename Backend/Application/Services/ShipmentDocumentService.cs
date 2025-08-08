using Application.DTOs.ShipmentDocument;
using Application.DTOs.ShipmentItem;
using Application.Interfaces;
using Domain.Models.Entities;
using Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class ShipmentDocumentService : IShipmentDocumentService
{
    private readonly IRepository<ShipmentDocument> _shipmentRepository;
    private readonly IBalanceService _balanceService;
    private readonly ApplicationContext _context;
    private readonly ILogger<ShipmentDocumentService> _logger;

    public ShipmentDocumentService(
        IRepository<ShipmentDocument> shipmentRepository,
        IBalanceService balanceService,
        ApplicationContext context,
        ILogger<ShipmentDocumentService> logger)
    {
        _shipmentRepository = shipmentRepository;
        _balanceService = balanceService;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, ShipmentDocumentResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateShipmentDocumentDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Business validation
            if (await ExistsByNumberAsync(dto.Number))
            {
                return (false, null, "A shipment document with this number already exists");
            }

            // Validate client exists
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
            if (!clientExists)
            {
                return (false, null, "Client not found");
            }

            // Validate items - shipment cannot be empty
            if (!dto.Items.Any())
            {
                return (false, null, "Shipment document must contain at least one item");
            }

            // Validate resources and units exist
            var validationError = await ValidateItems(dto.Items);
            if (!string.IsNullOrEmpty(validationError))
            {
                return (false, null, validationError);
            }

            var shipment = new ShipmentDocument
            {
                Number = dto.Number.Trim(),
                ClientId = dto.ClientId,
                Date = dto.Date,
                Status = ShipmentStatus.Draft, // Always start as draft
                Items = dto.Items.Select(item => new ShipmentItem
                {
                    ResourceId = item.ResourceId,
                    UnitId = item.UnitId,
                    Quantity = item.Quantity
                }).ToList()
            };

            var created = await _shipmentRepository.CreateAsync(shipment);
            if (created == null)
            {
                await transaction.RollbackAsync();
                return (false, null, "Failed to create shipment document");
            }

            // NOTE: No balance update on creation - only on signing
            await transaction.CommitAsync();

            var response = await MapToResponseDto(created);
            _logger.LogInformation("Shipment document created successfully with ID: {ShipmentId}", created.Id);
            
            return (true, response, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating shipment document with number: {Number}", dto.Number);
            return (false, null, "An error occurred while creating the shipment document");
        }
    }

    public async Task<(bool Success, ShipmentDocumentResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id)
    {
        try
        {
            var shipment = await _context.ShipmentDocuments
                .Include(s => s.Client)
                .Include(s => s.Items)
                .ThenInclude(i => i.Resource)
                .Include(s => s.Items)
                .ThenInclude(i => i.Unit)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null)
            {
                return (false, null, "Shipment document not found");
            }

            var response = await MapToResponseDto(shipment);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment document with ID: {ShipmentId}", id);
            return (false, null, "An error occurred while retrieving the shipment document");
        }
    }

    public async Task<(IEnumerable<ShipmentDocumentResponseDto> Data, int TotalCount)> GetAllAsync(ShipmentFilterModel? filter = null)
    {
        try
        {
            var query = _context.ShipmentDocuments
                .Include(s => s.Client)
                .Include(s => s.Items)
                .ThenInclude(i => i.Resource)
                .Include(s => s.Items)
                .ThenInclude(i => i.Unit)
                .AsQueryable();

            // Apply filters
            if (filter != null)
            {
                if (filter.DateFrom.HasValue)
                    query = query.Where(s => s.Date >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    query = query.Where(s => s.Date <= filter.DateTo.Value);

                if (filter.DocumentNumbers?.Any() == true)
                    query = query.Where(s => filter.DocumentNumbers.Contains(s.Number));

                if (filter.ClientIds?.Any() == true)
                    query = query.Where(s => filter.ClientIds.Contains(s.ClientId));

                if (filter.Statuses?.Any() == true)
                    query = query.Where(s => filter.Statuses.Contains(s.Status));

                if (filter.ResourceIds?.Any() == true)
                    query = query.Where(s => s.Items.Any(i => filter.ResourceIds.Contains(i.ResourceId)));

                if (filter.UnitIds?.Any() == true)
                    query = query.Where(s => s.Items.Any(i => filter.UnitIds.Contains(i.UnitId)));

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(s => 
                        s.Number.ToLower().Contains(searchTerm) ||
                        s.Client.Name.ToLower().Contains(searchTerm) ||
                        s.Items.Any(i => i.Resource.Name.ToLower().Contains(searchTerm)));
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

                var shipments = await query.ToListAsync();
                var result = new List<ShipmentDocumentResponseDto>();

                foreach (var shipment in shipments)
                {
                    result.Add(await MapToResponseDto(shipment));
                }

                return (result, totalCount);
            }

            var allShipments = await query.ToListAsync();
            var allResult = new List<ShipmentDocumentResponseDto>();

            foreach (var shipment in allShipments)
            {
                allResult.Add(await MapToResponseDto(shipment));
            }

            return (allResult, allShipments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shipment documents");
            return (Enumerable.Empty<ShipmentDocumentResponseDto>(), 0);
        }
    }

    public async Task<(bool Success, ShipmentDocumentResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateShipmentDocumentDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.ShipmentDocuments
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (existing == null)
            {
                await transaction.RollbackAsync();
                return (false, null, "Shipment document not found");
            }

            // Cannot edit signed shipments
            if (existing.Status == ShipmentStatus.Signed)
            {
                await transaction.RollbackAsync();
                return (false, null, "Cannot edit signed shipment documents");
            }

            // Business validation
            if (await ExistsByNumberAsync(dto.Number, dto.Id))
            {
                await transaction.RollbackAsync();
                return (false, null, "A shipment document with this number already exists");
            }

            // Validate client exists
            var clientExists = await _context.Clients.AnyAsync(c => c.Id == dto.ClientId);
            if (!clientExists)
            {
                await transaction.RollbackAsync();
                return (false, null, "Client not found");
            }

            // Validate items - shipment cannot be empty
            if (!dto.Items.Any())
            {
                await transaction.RollbackAsync();
                return (false, null, "Shipment document must contain at least one item");
            }

            // Validate resources and units exist
            var validationError = await ValidateItemsForUpdate(dto.Items);
            if (!string.IsNullOrEmpty(validationError))
            {
                await transaction.RollbackAsync();
                return (false, null, validationError);
            }

            // Update basic properties
            existing.Number = dto.Number.Trim();
            existing.ClientId = dto.ClientId;
            existing.Date = dto.Date;
            existing.UpdatedAt = DateTime.UtcNow;

            // Update items
            existing.Items.Clear();
            foreach (var itemDto in dto.Items)
            {
                existing.Items.Add(new ShipmentItem
                {
                    Id = itemDto.Id,
                    DocumentId = existing.Id,
                    ResourceId = itemDto.ResourceId,
                    UnitId = itemDto.UnitId,
                    Quantity = itemDto.Quantity
                });
            }

            var updated = await _shipmentRepository.UpdateAsync(existing);

            // NOTE: No balance update on edit - only on signing/revoking
            await transaction.CommitAsync();

            var response = await MapToResponseDto(updated);
            _logger.LogInformation("Shipment document updated successfully with ID: {ShipmentId}", dto.Id);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating shipment document with ID: {ShipmentId}", dto.Id);
            return (false, null, "An error occurred while updating the shipment document");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var shipment = await _context.ShipmentDocuments
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null)
            {
                await transaction.RollbackAsync();
                return (false, "Shipment document not found");
            }

            // Cannot delete signed shipments
            if (shipment.Status == ShipmentStatus.Signed)
            {
                await transaction.RollbackAsync();
                return (false, "Cannot delete signed shipment documents");
            }

            var success = await _shipmentRepository.DeleteAsync(id);
            if (!success)
            {
                await transaction.RollbackAsync();
                return (false, "Failed to delete shipment document");
            }

            // NOTE: No balance update needed for draft/revoked shipments
            await transaction.CommitAsync();

            _logger.LogInformation("Shipment document deleted successfully with ID: {ShipmentId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting shipment document with ID: {ShipmentId}", id);
            return (false, "An error occurred while deleting the shipment document");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> SignAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var shipment = await _context.ShipmentDocuments
                .Include(s => s.Items)
                .ThenInclude(i => i.Resource)
                .Include(s => s.Items)
                .ThenInclude(i => i.Unit)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null)
            {
                await transaction.RollbackAsync();
                return (false, "Shipment document not found");
            }

            if (shipment.Status == ShipmentStatus.Signed)
            {
                await transaction.RollbackAsync();
                return (false, "Shipment document is already signed");
            }

            // Validate balance before signing
            try
            {
                await _balanceService.ValidateShipmentBalanceAsync(shipment);
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message);
            }

            // Update status
            shipment.Status = ShipmentStatus.Signed;
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            // Update balance
            await _balanceService.UpdateBalanceOnShipmentSignAsync(shipment);

            await transaction.CommitAsync();

            _logger.LogInformation("Shipment document signed successfully with ID: {ShipmentId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error signing shipment document with ID: {ShipmentId}", id);
            return (false, "An error occurred while signing the shipment document");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RevokeAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var shipment = await _context.ShipmentDocuments
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shipment == null)
            {
                await transaction.RollbackAsync();
                return (false, "Shipment document not found");
            }

            if (shipment.Status != ShipmentStatus.Signed)
            {
                await transaction.RollbackAsync();
                return (false, "Only signed shipment documents can be revoked");
            }

            // Update status
            shipment.Status = ShipmentStatus.Revoked;
            shipment.UpdatedAt = DateTime.UtcNow;

            await _shipmentRepository.UpdateAsync(shipment);

            // Restore balance
            await _balanceService.UpdateBalanceOnShipmentRevokeAsync(shipment);

            await transaction.CommitAsync();

            _logger.LogInformation("Shipment document revoked successfully with ID: {ShipmentId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error revoking shipment document with ID: {ShipmentId}", id);
            return (false, "An error occurred while revoking the shipment document");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _shipmentRepository.GetCountAsync(s => s.Id == id) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if shipment document exists with ID: {ShipmentId}", id);
            return false;
        }
    }

    public async Task<bool> ExistsByNumberAsync(string number, int? excludeId = null)
    {
        try
        {
            var normalizedNumber = number.Trim().ToLowerInvariant();
            return excludeId.HasValue
                ? await _context.ShipmentDocuments.AnyAsync(s => s.Number.ToLower() == normalizedNumber && s.Id != excludeId.Value)
                : await _context.ShipmentDocuments.AnyAsync(s => s.Number.ToLower() == normalizedNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if shipment document exists with number: {Number}", number);
            return false;
        }
    }

    private async Task<string?> ValidateItems(List<CreateShipmentItemDto> items)
    {
        foreach (var item in items)
        {
            var resource = await _context.Resources.FindAsync(item.ResourceId);
            if (resource == null)
            {
                return $"Resource with ID {item.ResourceId} not found";
            }

            var unit = await _context.Units.FindAsync(item.UnitId);
            if (unit == null)
            {
                return $"Unit with ID {item.UnitId} not found";
            }
        }

        return null;
    }

    private async Task<string?> ValidateItemsForUpdate(List<UpdateShipmentItemDto> items)
    {
        foreach (var item in items)
        {
            var resource = await _context.Resources.FindAsync(item.ResourceId);
            if (resource == null)
            {
                return $"Resource with ID {item.ResourceId} not found";
            }

            var unit = await _context.Units.FindAsync(item.UnitId);
            if (unit == null)
            {
                return $"Unit with ID {item.UnitId} not found";
            }
        }

        return null;
    }

    private static IQueryable<ShipmentDocument> ApplySorting(IQueryable<ShipmentDocument> query, ShipmentFilterModel filter)
    {
        return filter.SortedField?.ToLower() switch
        {
            "number" => filter.IsAscending 
                ? query.OrderBy(s => s.Number) 
                : query.OrderByDescending(s => s.Number),
            "clientname" => filter.IsAscending 
                ? query.OrderBy(s => s.Client.Name) 
                : query.OrderByDescending(s => s.Client.Name),
            "date" => filter.IsAscending 
                ? query.OrderBy(s => s.Date) 
                : query.OrderByDescending(s => s.Date),
            "status" => filter.IsAscending 
                ? query.OrderBy(s => s.Status) 
                : query.OrderByDescending(s => s.Status),
            "createdat" => filter.IsAscending 
                ? query.OrderBy(s => s.CreatedAt) 
                : query.OrderByDescending(s => s.CreatedAt),
            _ => filter.IsAscending 
                ? query.OrderBy(s => s.Id) 
                : query.OrderByDescending(s => s.Id)
        };
    }

    private async Task<ShipmentDocumentResponseDto> MapToResponseDto(ShipmentDocument shipment)
    {
        var items = new List<ShipmentItemResponseDto>();

        foreach (var item in shipment.Items)
        {
            items.Add(new ShipmentItemResponseDto
            {
                Id = item.Id,
                ResourceId = item.ResourceId,
                ResourceName = item.Resource?.Name ?? (await _context.Resources.FindAsync(item.ResourceId))?.Name ?? string.Empty,
                UnitId = item.UnitId,
                UnitName = item.Unit?.Name ?? (await _context.Units.FindAsync(item.UnitId))?.Name ?? string.Empty,
                Quantity = item.Quantity
            });
        }

        return new ShipmentDocumentResponseDto
        {
            Id = shipment.Id,
            Number = shipment.Number,
            ClientId = shipment.ClientId,
            ClientName = shipment.Client?.Name ?? (await _context.Clients.FindAsync(shipment.ClientId))?.Name ?? string.Empty,
            Date = shipment.Date,
            Status = shipment.Status,
            StatusName = shipment.Status.ToString(),
            CreatedAt = shipment.CreatedAt,
            UpdatedAt = shipment.UpdatedAt,
            Items = items
        };
    }
}
