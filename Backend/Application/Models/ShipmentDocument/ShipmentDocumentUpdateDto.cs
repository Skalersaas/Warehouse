using System.ComponentModel.DataAnnotations;
using Domain.Models.Enums;
using Application.Models.ShipmentItem;
using Domain.Models.Interfaces;

namespace Application.Models.ShipmentDocument;

public class UpdateShipmentDocumentDto : IModel
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Client is required")]
    public int ClientId { get; set; }
    public ICollection<UpdateShipmentItemDto> Items { get; set; } = [];
}
