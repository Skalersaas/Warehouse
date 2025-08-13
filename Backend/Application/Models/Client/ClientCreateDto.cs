using System.ComponentModel.DataAnnotations;

namespace Application.Models.Client;

public class CreateClientDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Address { get; set; } = string.Empty;
}
