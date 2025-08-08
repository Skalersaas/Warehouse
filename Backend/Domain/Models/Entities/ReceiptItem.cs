using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class ReceiptItem : IModel
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }

    public virtual ReceiptDocument Document { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
    public virtual Unit Unit { get; set; } = null!;
}
