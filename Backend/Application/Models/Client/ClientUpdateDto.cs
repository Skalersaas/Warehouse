using Domain.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Application.Models.Client;

public class UpdateClientDto : IModel
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
    
    public bool IsArchived { get; set; }
}
