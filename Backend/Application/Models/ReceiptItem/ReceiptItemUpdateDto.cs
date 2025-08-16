using System.ComponentModel.DataAnnotations;

namespace Application.Models.ReceiptItem;

public class UpdateReceiptItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ResourceId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int UnitId { get; set; }
    
    [Required]
    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
}
