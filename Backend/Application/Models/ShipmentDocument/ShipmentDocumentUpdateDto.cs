using Domain.Models.Interfaces;

namespace Application.Models.ShipmentDocument;

public class ShipmentDocumentUpdateDto : IModel
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public int Quantity { get; set; }
}
