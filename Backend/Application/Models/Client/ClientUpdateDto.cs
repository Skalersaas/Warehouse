using Domain.Models.Interfaces;

namespace Application.Models.Client;

public class ClientUpdateDto : IModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
