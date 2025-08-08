using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.ReceiptItem;
public class UpdateReceiptItemDto
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Resource ID must be valid")]
    public int ResourceId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Unit ID must be valid")]
    public int UnitId { get; set; }

    [Required]
    [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
}
