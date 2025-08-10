namespace Utilities.DataManipulation;

public class SearchFilterModel : SearchModel
{
    public Dictionary<string, string> Filters { get; set; } = [];
}
