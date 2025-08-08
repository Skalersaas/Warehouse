using Domain.Models.Enums;
using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class ShipmentDocument : IModel
{
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Client Client { get; set; } = null!;
    public virtual ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    
    // Business rule: shipment cannot be empty
    public bool HasItems => Items?.Any() == true;
}
