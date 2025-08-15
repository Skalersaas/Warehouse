using Domain.Models.Interfaces;

namespace Application.Models.Balance;

public class BalanceUpdateDto : IModel
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
}
