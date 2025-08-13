using Application.Models.ReceiptItem;

namespace Application.Models.ReceiptDocument;

public class ReceiptDocumentResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<ReceiptItemResponseDto>? Items { get; set; }
}
