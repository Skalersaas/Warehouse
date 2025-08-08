using Application.DTOs.ReceiptDocument;
using Utilities.DataManipulation;

namespace Application.Interfaces;

public interface IReceiptDocumentService
{
    Task<(bool Success, ReceiptDocumentResponseDto? Data, string? ErrorMessage)> CreateAsync(CreateReceiptDocumentDto dto);
    Task<(bool Success, ReceiptDocumentResponseDto? Data, string? ErrorMessage)> GetByIdAsync(int id);
    Task<(IEnumerable<ReceiptDocumentResponseDto> Data, int TotalCount)> GetAllAsync(DocumentFilterModel? filter = null);
    Task<(bool Success, ReceiptDocumentResponseDto? Data, string? ErrorMessage)> UpdateAsync(UpdateReceiptDocumentDto dto);
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNumberAsync(string number, int? excludeId = null);
}
