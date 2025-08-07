using Domain.Models.Interfaces;

namespace Domain.Models.Entities;

public class ReceiptDocument : IModel
{
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Date { get; set; }

    public ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();

}
