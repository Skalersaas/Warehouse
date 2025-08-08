using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class Balance : IModel
{
    public int Id { get; set; }
    public int ResourceId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }

    public virtual Resource Resource { get; set; } = null!;
    public virtual Unit Unit { get; set; } = null!;
}
