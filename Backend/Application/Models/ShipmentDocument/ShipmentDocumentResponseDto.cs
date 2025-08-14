using Domain.Models.Enums;
using Application.Models.ShipmentItem;

namespace Application.Models.ShipmentDocument;

public class ShipmentDocumentResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public DateTime Date { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public IEnumerable<ShipmentItemResponseDto>? Items { get; set; }
}
