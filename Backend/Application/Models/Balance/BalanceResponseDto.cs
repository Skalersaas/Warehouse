namespace Application.Models.Balance;

public class BalanceResponseDto
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public string ResourceName { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; }
    public decimal Quantity { get; set; }
}
