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
    [Range(1, int.MaxValue)]
    public int ClientId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public ShipmentStatus Status { get; set; }
    
    [Required]
    [MinLength(1)]
    public ICollection<UpdateShipmentItemDto> Items { get; set; } = [];
}
