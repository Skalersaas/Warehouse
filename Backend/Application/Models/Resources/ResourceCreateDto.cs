using System.ComponentModel.DataAnnotations;

namespace Application.Models.Resources;

public class CreateResourceDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
