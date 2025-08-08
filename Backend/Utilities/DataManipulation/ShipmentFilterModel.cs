using Domain.Models.Enums;

namespace Utilities.DataManipulation;

public class ShipmentFilterModel : DocumentFilterModel
{
    public List<int>? ClientIds { get; set; }
    public List<ShipmentStatus>? Statuses { get; set; }
}
