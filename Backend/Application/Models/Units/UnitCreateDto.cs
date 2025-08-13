using System.ComponentModel.DataAnnotations;

namespace Application.Models.Units;

public class CreateUnitDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}
