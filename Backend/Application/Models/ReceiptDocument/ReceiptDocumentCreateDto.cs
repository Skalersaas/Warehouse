using Application.Models.ReceiptItem;

namespace Application.Models.ReceiptDocument;

public class ReceiptDocumentCreateDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public IEnumerable<ReceiptItemCreateDto> Items { get; set; } = [];
}
