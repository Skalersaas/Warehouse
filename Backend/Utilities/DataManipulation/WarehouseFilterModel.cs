namespace Utilities.DataManipulation;

public class WarehouseFilterModel : SearchModel
{
    public List<int>? ResourceIds { get; set; }
    public List<int>? UnitIds { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
}
