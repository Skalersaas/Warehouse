namespace Application.Models.Balance;

public class BalanceUpdateDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
}
