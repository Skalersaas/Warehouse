namespace Utilities.DataManipulation;

public class SearchModel
{
    public string? SearchTerm { get; set; }
    public string SortedField { get; set; } = "Id";
    public bool IsAscending { get; set; } = true;
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;

    public bool PaginationValid() => Page > 0 && Size > 0;
}
