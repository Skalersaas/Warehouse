using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Resource;
public class UpdateResourceDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
}
