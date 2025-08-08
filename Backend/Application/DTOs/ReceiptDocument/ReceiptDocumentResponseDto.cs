using Application.DTOs.ReceiptItem;

namespace Application.DTOs.ReceiptDocument;
public class ReceiptDocumentResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ReceiptItemResponseDto> Items { get; set; } = new List<ReceiptItemResponseDto>();
}
