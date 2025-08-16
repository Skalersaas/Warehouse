using Application.Models.Resources;
using Application.Models.Units;
using Domain.Models.Entities;

namespace Application.Models.ReceiptItem;

public class ReceiptItemResponseDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }

    public string ResourceName { get; set; }
    public string UnitName { get; set; }
}
