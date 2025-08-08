using Application.DTOs.ReceiptDocument;
using Application.DTOs.ReceiptItem;
using Application.Interfaces;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class ReceiptDocumentService : IReceiptDocumentService
{
    private readonly IRepository<ReceiptDocument> _receiptRepository;
    private readonly IBalanceService _balanceService;
    private readonly ApplicationContext _context;
    private readonly ILogger<ReceiptDocumentService> _logger;

    public ReceiptDocumentService(
        IRepository<ReceiptDocument> receiptRepository,
        IBalanceService balanceService,
        ApplicationContext context,
        ILogger<ReceiptDocumentService> logger)
    {
        _receiptRepository = receiptRepository;
        _balanceService = balanceService;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, ReceiptDocumentResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateReceiptDocumentDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Business validation
            if (await ExistsByNumberAsync(dto.Number))
            {
                return (false, null, "A receipt document with this number already exists");
            }

            // Validate resources and units exist
            var validationError = await ValidateItems(dto.Items);
            if (!string.IsNullOrEmpty(validationError))
            {
                return (false, null, validationError);
            }

            var receipt = new ReceiptDocument
            {
                Number = dto.Number.Trim(),
                Date = dto.Date,
                Items = dto.Items.Select(item => new ReceiptItem
                {
                    ResourceId = item.ResourceId,
                    UnitId = item.UnitId,
                    Quantity = item.Quantity
                }).ToList()
            };

            var created = await _receiptRepository.CreateAsync(receipt);
            if (created == null)
            {
                await transaction.RollbackAsync();
                return (false, null, "Failed to create receipt document");
            }

            // Update balance
            await _balanceService.UpdateBalanceOnReceiptAsync(created);

            await transaction.CommitAsync();

            var response = await MapToResponseDto(created);
            _logger.LogInformation("Receipt document created successfully with ID: {ReceiptId}", created.Id);
            
            return (true, response, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating receipt document with number: {Number}", dto.Number);
            return (false, null, "An error occurred while creating the receipt document");
        }
    }

    public async Task<(bool Success, ReceiptDocumentResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id)
    {
        try
        {
            var receipt = await _context.ReceiptDocuments
                .Include(r => r.Items)
                .ThenInclude(i => i.Resource)
                .Include(r => r.Items)
                .ThenInclude(i => i.Unit)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null)
            {
                return (false, null, "Receipt document not found");
            }

            var response = await MapToResponseDto(receipt);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receipt document with ID: {ReceiptId}", id);
            return (false, null, "An error occurred while retrieving the receipt document");
        }
    }

    public async Task<(IEnumerable<ReceiptDocumentResponseDto> Data, int TotalCount)> GetAllAsync(DocumentFilterModel? filter = null)
    {
        try
        {
            var query = _context.ReceiptDocuments
                .Include(r => r.Items)
                .ThenInclude(i => i.Resource)
                .Include(r => r.Items)
                .ThenInclude(i => i.Unit)
                .AsQueryable();

            // Apply filters
            if (filter != null)
            {
                if (filter.DateFrom.HasValue)
                    query = query.Where(r => r.Date >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    query = query.Where(r => r.Date <= filter.DateTo.Value);

                if (filter.DocumentNumbers?.Any() == true)
                    query = query.Where(r => filter.DocumentNumbers.Contains(r.Number));

                if (filter.ResourceIds?.Any() == true)
                    query = query.Where(r => r.Items.Any(i => filter.ResourceIds.Contains(i.ResourceId)));

                if (filter.UnitIds?.Any() == true)
                    query = query.Where(r => r.Items.Any(i => filter.UnitIds.Contains(i.UnitId)));

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchTerm = filter.SearchTerm.ToLower();
                    query = query.Where(r => 
                        r.Number.ToLower().Contains(searchTerm) ||
                        r.Items.Any(i => i.Resource.Name.ToLower().Contains(searchTerm)));
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

                var receipts = await query.ToListAsync();
                var result = new List<ReceiptDocumentResponseDto>();

                foreach (var receipt in receipts)
                {
                    result.Add(await MapToResponseDto(receipt));
                }

                return (result, totalCount);
            }

            var allReceipts = await query.ToListAsync();
            var allResult = new List<ReceiptDocumentResponseDto>();

            foreach (var receipt in allReceipts)
            {
                allResult.Add(await MapToResponseDto(receipt));
            }

            return (allResult, allReceipts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving receipt documents");
            return (Enumerable.Empty<ReceiptDocumentResponseDto>(), 0);
        }
    }

    public async Task<(bool Success, ReceiptDocumentResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateReceiptDocumentDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.ReceiptDocuments
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (existing == null)
            {
                await transaction.RollbackAsync();
                return (false, null, "Receipt document not found");
            }

            // Business validation
            if (await ExistsByNumberAsync(dto.Number, dto.Id))
            {
                await transaction.RollbackAsync();
                return (false, null, "A receipt document with this number already exists");
            }

            // Validate resources and units exist
            var validationError = await ValidateItemsForUpdate(dto.Items);
            if (!string.IsNullOrEmpty(validationError))
            {
                await transaction.RollbackAsync();
                return (false, null, validationError);
            }

            // Store old receipt for balance calculation
            var oldReceipt = new ReceiptDocument
            {
                Id = existing.Id,
                Items = existing.Items.ToList()
            };

            // Update basic properties
            existing.Number = dto.Number.Trim();
            existing.Date = dto.Date;
            existing.UpdatedAt = DateTime.UtcNow;

            // Update items
            existing.Items.Clear();
            foreach (var itemDto in dto.Items)
            {
                existing.Items.Add(new ReceiptItem
                {
                    Id = itemDto.Id,
                    DocumentId = existing.Id,
                    ResourceId = itemDto.ResourceId,
                    UnitId = itemDto.UnitId,
                    Quantity = itemDto.Quantity
                });
            }

            var updated = await _receiptRepository.UpdateAsync(existing);

            // Update balance
            await _balanceService.UpdateBalanceOnReceiptUpdateAsync(oldReceipt, updated);

            await transaction.CommitAsync();

            var response = await MapToResponseDto(updated);
            _logger.LogInformation("Receipt document updated successfully with ID: {ReceiptId}", dto.Id);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating receipt document with ID: {ReceiptId}", dto.Id);
            return (false, null, "An error occurred while updating the receipt document");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var receipt = await _context.ReceiptDocuments
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receipt == null)
            {
                await transaction.RollbackAsync();
                return (false, "Receipt document not found");
            }

            // Check if we have sufficient balance to revert this receipt
            foreach (var item in receipt.Items)
            {
                var currentBalance = await _balanceService.GetCurrentBalanceAsync(item.ResourceId, item.UnitId);
                if (currentBalance < item.Quantity)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Cannot delete receipt: insufficient current balance for {item.Resource?.Name ?? "resource"}");
                }
            }

            // Update balance first
            await _balanceService.UpdateBalanceOnReceiptDeleteAsync(receipt);

            // Then delete the receipt
            var success = await _receiptRepository.DeleteAsync(id);
            if (!success)
            {
                await transaction.RollbackAsync();
                return (false, "Failed to delete receipt document");
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Receipt document deleted successfully with ID: {ReceiptId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error deleting receipt document with ID: {ReceiptId}", id);
            return (false, "An error occurred while deleting the receipt document");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _receiptRepository.GetCountAsync(r => r.Id == id) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if receipt document exists with ID: {ReceiptId}", id);
            return false;
        }
    }

    public async Task<bool> ExistsByNumberAsync(string number, int? excludeId = null)
    {
        try
        {
            var normalizedNumber = number.Trim().ToLowerInvariant();
            return excludeId.HasValue
                ? await _context.ReceiptDocuments.AnyAsync(r => r.Number.ToLower() == normalizedNumber && r.Id != excludeId.Value)
                : await _context.ReceiptDocuments.AnyAsync(r => r.Number.ToLower() == normalizedNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if receipt document exists with number: {Number}", number);
            return false;
        }
    }

    private async Task<string?> ValidateItems(List<CreateReceiptItemDto> items)
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

    private async Task<string?> ValidateItemsForUpdate(List<UpdateReceiptItemDto> items)
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

    private static IQueryable<ReceiptDocument> ApplySorting(IQueryable<ReceiptDocument> query, DocumentFilterModel filter)
    {
        return filter.SortedField?.ToLower() switch
        {
            "number" => filter.IsAscending 
                ? query.OrderBy(r => r.Number) 
                : query.OrderByDescending(r => r.Number),
            "date" => filter.IsAscending 
                ? query.OrderBy(r => r.Date) 
                : query.OrderByDescending(r => r.Date),
            "createdat" => filter.IsAscending 
                ? query.OrderBy(r => r.CreatedAt) 
                : query.OrderByDescending(r => r.CreatedAt),
            _ => filter.IsAscending 
                ? query.OrderBy(r => r.Id) 
                : query.OrderByDescending(r => r.Id)
        };
    }

    private async Task<ReceiptDocumentResponseDto> MapToResponseDto(ReceiptDocument receipt)
    {
        var items = new List<ReceiptItemResponseDto>();

        foreach (var item in receipt.Items)
        {
            items.Add(new ReceiptItemResponseDto
            {
                Id = item.Id,
                ResourceId = item.ResourceId,
                ResourceName = item.Resource?.Name ?? (await _context.Resources.FindAsync(item.ResourceId))?.Name ?? string.Empty,
                UnitId = item.UnitId,
                UnitName = item.Unit?.Name ?? (await _context.Units.FindAsync(item.UnitId))?.Name ?? string.Empty,
                Quantity = item.Quantity
            });
        }

        return new ReceiptDocumentResponseDto
        {
            Id = receipt.Id,
            Number = receipt.Number,
            Date = receipt.Date,
            CreatedAt = receipt.CreatedAt,
            UpdatedAt = receipt.UpdatedAt,
            Items = items
        };
    }
}
