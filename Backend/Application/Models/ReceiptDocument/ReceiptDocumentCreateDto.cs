using System.ComponentModel.DataAnnotations;
using Application.Models.ReceiptItem;

namespace Application.Models.ReceiptDocument;

public class CreateReceiptDocumentDto
{
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    [Required]
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31", ErrorMessage = "Date must be a valid date between 1900 and 2100")]
    public DateTime Date { get; set; }
    public ICollection<CreateReceiptItemDto>? Items { get; set; }
}
