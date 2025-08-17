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

    public ICollection<UpdateReceiptItemDto> Items { get; set; }
}
