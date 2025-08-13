namespace Application.Models.Balance;

public class BalanceCreateDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public int Quantity { get; set; }
}
