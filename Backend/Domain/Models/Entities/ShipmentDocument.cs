using Domain.Models.Enums;
using Domain.Models.Interfaces;

namespace Domain.Models.Entities;

public class ShipmentDocument : IModel
{
    public int Id { get; set; }
    public string Number { get; set; }
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Client Client { get; set; }
    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
}
