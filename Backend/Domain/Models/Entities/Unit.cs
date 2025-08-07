using Domain.Models.Interfaces;

namespace Domain.Models.Entities;

public class Unit : IModel, IArchivable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; }
}
