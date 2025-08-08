using Application.DTOs.ShipmentDocument;
using Domain.Models.Enums;
using Utilities.DataManipulation;

namespace Application.Interfaces;

public interface IShipmentDocumentService
{
    Task<(bool Success, ShipmentDocumentResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateShipmentDocumentDto dto);
    Task<(bool Success, ShipmentDocumentResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id);
    Task<(IEnumerable<ShipmentDocumentResponseDto> Data, int TotalCount)> GetAllAsync(ShipmentFilterModel? filter = null);
    Task<(bool Success, ShipmentDocumentResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateShipmentDocumentDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<(bool Success, string? ErrorMessage)> SignAsync(int id);
    Task<(bool Success, string? ErrorMessage)> RevokeAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNumberAsync(string number, int? excludeId = null);
}
