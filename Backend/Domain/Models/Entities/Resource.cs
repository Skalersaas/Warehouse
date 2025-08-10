using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class Resource : IModel, IArchivable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; }
    public virtual ICollection<ShipmentItem> ShipmentItems { get; set; }
    public virtual ICollection<Balance> Balances { get; set; }
}
