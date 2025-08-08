using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class ReceiptDocument : IModel
{
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
}
