using Domain.Models.Interfaces;

namespace Application.Models.Units;

public class UnitUpdateDto : IModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
