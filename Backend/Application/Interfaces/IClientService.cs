using Application.DTOs.Client;
using Utilities.DataManipulation;

namespace Application.Interfaces;

public interface IClientService
{
    Task<(bool Success, ClientResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateClientDto dto);
    Task<(bool Success, ClientResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id);
    Task<(IEnumerable<ClientResponseDto> Data, int TotalCount)> GetAllAsync(SearchModel? searchModel = null);
    Task<(bool Success, ClientResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateClientDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<(bool Success, string? ErrorMessage)> ArchiveAsync(int id);
    Task<(bool Success, string? ErrorMessage)> UnarchiveAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
}
