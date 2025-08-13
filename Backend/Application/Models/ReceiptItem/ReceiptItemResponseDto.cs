namespace Application.Models.ReceiptItem;

public class ReceiptItemResponseDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
