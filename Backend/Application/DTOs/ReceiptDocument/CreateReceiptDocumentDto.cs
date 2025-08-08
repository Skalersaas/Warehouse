using System.ComponentModel.DataAnnotations;
using Application.DTOs.ReceiptItem;

namespace Application.DTOs.ReceiptDocument;
public class CreateReceiptDocumentDto
{
    [Required(ErrorMessage = "Number is required")]
    [StringLength(50, ErrorMessage = "Number cannot exceed 50 characters")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    public List<CreateReceiptItemDto> Items { get; set; } = new List<CreateReceiptItemDto>();
}
