using Application.DTOs.Resource;
using Utilities.DataManipulation;

namespace Application.Interfaces;

public interface IResourceService
{
    Task<(bool Success, ResourceResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateResourceDto dto);
    Task<(bool Success, ResourceResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id);
    Task<(IEnumerable<ResourceResponseDto> Data, int TotalCount)> GetAllAsync(SearchModel? searchModel = null);
    Task<(bool Success, ResourceResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateResourceDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<(bool Success, string? ErrorMessage)> ArchiveAsync(int id);
    Task<(bool Success, string? ErrorMessage)> UnarchiveAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> IsInUseAsync(int id);
}
