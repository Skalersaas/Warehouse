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

    [Required]
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31", ErrorMessage = "Date must be a valid date between 1900 and 2100")]
    public DateTime Date { get; set; }
    public ICollection<CreateShipmentItemDto> Items { get; set; } = [];
}
