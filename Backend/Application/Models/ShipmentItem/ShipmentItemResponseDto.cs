namespace Application.Models.ShipmentItem;

public class ShipmentItemResponseDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }

    public string ResourceName { get; set; }
    public string UnitName { get; set; }
}
