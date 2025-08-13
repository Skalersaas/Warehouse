using Domain.Models.Interfaces;

namespace Application.Models.ReceiptItem;

public class ReceiptItemUpdateDto : IModel
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
}
