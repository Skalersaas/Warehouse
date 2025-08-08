using System.ComponentModel.DataAnnotations;
using Application.DTOs.ShipmentItem;
using Domain.Models.Enums;

namespace Application.DTOs.ShipmentDocument;
public class UpdateShipmentDocumentDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Number is required")]
    [StringLength(50, ErrorMessage = "Number cannot exceed 50 characters")]
    public string Number { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Client ID must be valid")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    public ShipmentStatus Status { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Shipment document must contain at least one item")]
    public List<UpdateShipmentItemDto> Items { get; set; } = new List<UpdateShipmentItemDto>();
}
