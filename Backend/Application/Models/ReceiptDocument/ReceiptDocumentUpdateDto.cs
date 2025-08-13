using Application.Models.ReceiptItem;
using Domain.Models.Interfaces;

namespace Application.Models.ReceiptDocument;

public class ReceiptDocumentUpdateDto : IModel
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public IEnumerable<ReceiptItemUpdateDto> Items { get; set; } = [];
}
