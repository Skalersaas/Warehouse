using Application.DTOs.Unit;
using Application.Interfaces;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class UnitService : IUnitService
{
    private readonly IArchivableRepository<Unit> _unitRepository;
    private readonly ApplicationContext _context;
    private readonly ILogger<UnitService> _logger;

    public UnitService(IArchivableRepository<Unit> unitRepository, ApplicationContext context, ILogger<UnitService> logger)
    {
        _unitRepository = unitRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, UnitResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateUnitDto dto)
    {
        try
        {
            // Business validation
            if (await ExistsByNameAsync(dto.Name))
            {
                return (false, null, "A unit with this name already exists");
            }

            var unit = new Unit
            {
                Name = dto.Name.Trim(),
                IsArchived = false
            };

            var created = await _unitRepository.CreateAsync(unit);
            if (created == null)
            {
                return (false, null, "Failed to create unit");
            }

            var response = MapToResponseDto(created);
            _logger.LogInformation("Unit created successfully with ID: {UnitId}", created.Id);
            
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit with name: {Name}", dto.Name);
            return (false, null, "An error occurred while creating the unit");
        }
    }

    public async Task<(bool Success, UnitResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id)
    {
        try
        {
            var unit = await _unitRepository.GetByIdAsync(id);
            if (unit == null)
            {
                return (false, null, "Unit not found");
            }

            var response = MapToResponseDto(unit);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving unit with ID: {UnitId}", id);
            return (false, null, "An error occurred while retrieving the unit");
        }
    }

    public async Task<(IEnumerable<UnitResponseDto> Data, int TotalCount)> GetAllAsync(SearchModel? searchModel = null)
    {
        try
        {
            var (units, totalCount) = await _unitRepository.QueryBy(searchModel);
            var response = units.Select(MapToResponseDto);
            return (response, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving units");
            return (Enumerable.Empty<UnitResponseDto>(), 0);
        }
    }

    public async Task<(bool Success, UnitResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateUnitDto dto)
    {
        try
        {
            var existing = await _unitRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return (false, null, "Unit not found");
            }

            // Business validation
            if (await ExistsByNameAsync(dto.Name, dto.Id))
            {
                return (false, null, "A unit with this name already exists");
            }

            existing.Name = dto.Name.Trim();
            existing.IsArchived = dto.IsArchived;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _unitRepository.UpdateAsync(existing);
            var response = MapToResponseDto(updated);
            
            _logger.LogInformation("Unit updated successfully with ID: {UnitId}", dto.Id);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit with ID: {UnitId}", dto.Id);
            return (false, null, "An error occurred while updating the unit");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        try
        {
            if (await IsInUseAsync(id))
            {
                return (false, "Cannot delete unit because it is being used in receipts or shipments");
            }

            var success = await _unitRepository.DeleteAsync(id);
            if (!success)
            {
                return (false, "Unit not found");
            }

            _logger.LogInformation("Unit deleted successfully with ID: {UnitId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit with ID: {UnitId}", id);
            return (false, "An error occurred while deleting the unit");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ArchiveAsync(int id)
    {
        try
        {
            var success = await _unitRepository.ArchiveAsync(id);
            if (!success)
            {
                return (false, "Unit not found");
            }

            _logger.LogInformation("Unit archived successfully with ID: {UnitId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving unit with ID: {UnitId}", id);
            return (false, "An error occurred while archiving the unit");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UnarchiveAsync(int id)
    {
        try
        {
            var success = await _unitRepository.UnarchiveAsync(id);
            if (!success)
            {
                return (false, "Unit not found");
            }

            _logger.LogInformation("Unit unarchived successfully with ID: {UnitId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unarchiving unit with ID: {UnitId}", id);
            return (false, "An error occurred while unarchiving the unit");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _unitRepository.GetCountAsync(u => u.Id == id) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if unit exists with ID: {UnitId}", id);
            return false;
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        try
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            return excludeId.HasValue
                ? await _context.Units.IgnoreQueryFilters()
                    .AnyAsync(u => u.Name.ToLower() == normalizedName && u.Id != excludeId.Value)
                : await _context.Units.IgnoreQueryFilters()
                    .AnyAsync(u => u.Name.ToLower() == normalizedName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if unit exists with name: {Name}", name);
            return false;
        }
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        try
        {
            var hasReceiptItems = await _context.ReceiptItems.AnyAsync(ri => ri.UnitId == id);
            var hasShipmentItems = await _context.ShipmentItems.AnyAsync(si => si.UnitId == id);
            var hasBalances = await _context.Balances.AnyAsync(b => b.UnitId == id);

            return hasReceiptItems || hasShipmentItems || hasBalances;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if unit is in use with ID: {UnitId}", id);
            return true; // Assume it's in use if we can't check
        }
    }

    private static UnitResponseDto MapToResponseDto(Unit unit)
    {
        return new UnitResponseDto
        {
            Id = unit.Id,
            Name = unit.Name,
            IsArchived = unit.IsArchived,
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt
        };
    }
}
