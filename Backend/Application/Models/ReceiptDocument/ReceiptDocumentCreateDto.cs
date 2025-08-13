using System.ComponentModel.DataAnnotations;
using Application.Models.ReceiptItem;

namespace Application.Models.ReceiptDocument;

public class CreateReceiptDocumentDto
{
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    public ICollection<CreateReceiptItemDto>? Items { get; set; }
}
