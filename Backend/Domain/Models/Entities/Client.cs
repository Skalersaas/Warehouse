using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class Client : IModel, IArchivable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ShipmentDocument> ShipmentDocuments { get; set; } = new List<ShipmentDocument>();
}
