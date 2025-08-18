using System.ComponentModel.DataAnnotations;
using Application.Models.ReceiptItem;
using Domain.Models.Interfaces;

namespace Application.Models.ReceiptDocument;

public class UpdateReceiptDocumentDto : IModel
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;
    [Required]
    [Range(typeof(DateTime), "1900-01-01", "2100-12-31", ErrorMessage = "Date must be a valid date between 1900 and 2100")]
    public DateTime Date { get; set; }

    public ICollection<UpdateReceiptItemDto> Items { get; set; }
}
