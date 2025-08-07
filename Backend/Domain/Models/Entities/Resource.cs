using Domain.Models.Interfaces;

namespace Domain.Models.Entities;
public class Resource : IModel, IArchivable
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsArchived { get; set; }
}
