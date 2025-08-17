using System.ComponentModel.DataAnnotations;
using Application.Models.ShipmentItem;

namespace Application.Models.ShipmentDocument;

public class CreateShipmentDocumentDto
{
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage ="Client is required")]
    public int ClientId { get; set; }
    public ICollection<CreateShipmentItemDto> Items { get; set; } = [];
}
