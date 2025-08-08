using Application.DTOs.Unit;
using Utilities.DataManipulation;

namespace Application.Interfaces;

public interface IUnitService
{
    Task<(bool Success, UnitResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateUnitDto dto);
    Task<(bool Success, UnitResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id);
    Task<(IEnumerable<UnitResponseDto> Data, int TotalCount)> GetAllAsync(SearchModel? searchModel = null);
    Task<(bool Success, UnitResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateUnitDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<(bool Success, string? ErrorMessage)> ArchiveAsync(int id);
    Task<(bool Success, string? ErrorMessage)> UnarchiveAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> IsInUseAsync(int id);
}
