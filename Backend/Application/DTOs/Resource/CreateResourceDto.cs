using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Resource;
public class CreateResourceDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
}
