namespace Utilities.DataManipulation;

public class DocumentFilterModel : SearchModel
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<string>? DocumentNumbers { get; set; }
    public List<int>? ResourceIds { get; set; }
    public List<int>? UnitIds { get; set; }
}
