using Application.DTOs.ShipmentItem;
using Domain.Models.Enums;

namespace Application.DTOs.ShipmentDocument;
public class ShipmentDocumentResponseDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public ShipmentStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ShipmentItemResponseDto> Items { get; set; } = new List<ShipmentItemResponseDto>();
}
