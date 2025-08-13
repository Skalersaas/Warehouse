namespace Application.Models.ReceiptItem;

public class ReceiptItemCreateDto
{
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public int Quantity { get; set; }
}
