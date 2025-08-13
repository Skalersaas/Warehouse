using System.ComponentModel.DataAnnotations;
using Application.Models.ShipmentItem;

namespace Application.Models.ShipmentDocument;

public class CreateShipmentDocumentDto
{
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [MinLength(1)]
    public ICollection<CreateShipmentItemDto> Items { get; set; } = [];
}
