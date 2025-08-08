using Application.DTOs.Resource;
using Application.Interfaces;
using Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Interfaces;
using Utilities.DataManipulation;

namespace Application.Services;

public class ResourceService : IResourceService
{
    private readonly IArchivableRepository<Resource> _resourceRepository;
    private readonly ApplicationContext _context;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(IArchivableRepository<Resource> resourceRepository, ApplicationContext context, ILogger<ResourceService> logger)
    {
        _resourceRepository = resourceRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, ResourceResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateResourceDto dto)
    {
        try
        {
            // Business validation
            if (await ExistsByNameAsync(dto.Name))
            {
                return (false, null, "A resource with this name already exists");
            }

            var resource = new Resource
            {
                Name = dto.Name.Trim(),
                IsArchived = false
            };

            var created = await _resourceRepository.CreateAsync(resource);
            if (created == null)
            {
                return (false, null, "Failed to create resource");
            }

            var response = MapToResponseDto(created);
            _logger.LogInformation("Resource created successfully with ID: {ResourceId}", created.Id);
            
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource with name: {Name}", dto.Name);
            return (false, null, "An error occurred while creating the resource");
        }
    }

    public async Task<(bool Success, ResourceResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id)
    {
        try
        {
            var resource = await _resourceRepository.GetByIdAsync(id);
            if (resource == null)
            {
                return (false, null, "Resource not found");
            }

            var response = MapToResponseDto(resource);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resource with ID: {ResourceId}", id);
            return (false, null, "An error occurred while retrieving the resource");
        }
    }

    public async Task<(IEnumerable<ResourceResponseDto> Data, int TotalCount)> GetAllAsync(SearchModel? searchModel = null)
    {
        try
        {
            var (resources, totalCount) = await _resourceRepository.QueryBy(searchModel);
            var response = resources.Select(MapToResponseDto);
            return (response, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving resources");
            return (Enumerable.Empty<ResourceResponseDto>(), 0);
        }
    }

    public async Task<(bool Success, ResourceResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateResourceDto dto)
    {
        try
        {
            var existing = await _resourceRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return (false, null, "Resource not found");
            }

            // Business validation
            if (await ExistsByNameAsync(dto.Name, dto.Id))
            {
                return (false, null, "A resource with this name already exists");
            }

            existing.Name = dto.Name.Trim();
            existing.IsArchived = dto.IsArchived;
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _resourceRepository.UpdateAsync(existing);
            var response = MapToResponseDto(updated);
            
            _logger.LogInformation("Resource updated successfully with ID: {ResourceId}", dto.Id);
            return (true, response, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource with ID: {ResourceId}", dto.Id);
            return (false, null, "An error occurred while updating the resource");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        try
        {
            if (await IsInUseAsync(id))
            {
                return (false, "Cannot delete resource because it is being used in receipts or shipments");
            }

            var success = await _resourceRepository.DeleteAsync(id);
            if (!success)
            {
                return (false, "Resource not found");
            }

            _logger.LogInformation("Resource deleted successfully with ID: {ResourceId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource with ID: {ResourceId}", id);
            return (false, "An error occurred while deleting the resource");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> ArchiveAsync(int id)
    {
        try
        {
            var success = await _resourceRepository.ArchiveAsync(id);
            if (!success)
            {
                return (false, "Resource not found");
            }

            _logger.LogInformation("Resource archived successfully with ID: {ResourceId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving resource with ID: {ResourceId}", id);
            return (false, "An error occurred while archiving the resource");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> UnarchiveAsync(int id)
    {
        try
        {
            var success = await _resourceRepository.UnarchiveAsync(id);
            if (!success)
            {
                return (false, "Resource not found");
            }

            _logger.LogInformation("Resource unarchived successfully with ID: {ResourceId}", id);
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unarchiving resource with ID: {ResourceId}", id);
            return (false, "An error occurred while unarchiving the resource");
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _resourceRepository.GetCountAsync(r => r.Id == id) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if resource exists with ID: {ResourceId}", id);
            return false;
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        try
        {
            var normalizedName = name.Trim().ToLowerInvariant();
            return excludeId.HasValue
                ? await _context.Resources.IgnoreQueryFilters()
                    .AnyAsync(r => r.Name.ToLower() == normalizedName && r.Id != excludeId.Value)
                : await _context.Resources.IgnoreQueryFilters()
                    .AnyAsync(r => r.Name.ToLower() == normalizedName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if resource exists with name: {Name}", name);
            return false;
        }
    }

    public async Task<bool> IsInUseAsync(int id)
    {
        try
        {
            var hasReceiptItems = await _context.ReceiptItems.AnyAsync(ri => ri.ResourceId == id);
            var hasShipmentItems = await _context.ShipmentItems.AnyAsync(si => si.ResourceId == id);
            var hasBalances = await _context.Balances.AnyAsync(b => b.ResourceId == id);

            return hasReceiptItems || hasShipmentItems || hasBalances;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if resource is in use with ID: {ResourceId}", id);
            return true; // Assume it's in use if we can't check
        }
    }

    private static ResourceResponseDto MapToResponseDto(Resource resource)
    {
        return new ResourceResponseDto
        {
            Id = resource.Id,
            Name = resource.Name,
            IsArchived = resource.IsArchived,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt
        };
    }
}
