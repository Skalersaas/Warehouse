using Domain.Models.Interfaces;

namespace Application.Models.Resources;

public class ResourceUpdateDto : IModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
