using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class Resource : IModel, IArchivable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
    public virtual ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    public virtual ICollection<Balance> Balances { get; set; } = new List<Balance>();
}
