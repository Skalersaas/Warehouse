namespace Utilities.DataManipulation;

public class SearchFilterModel : SearchModel
{
    public Dictionary<string, string> Filters { get; set; } = [];
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
