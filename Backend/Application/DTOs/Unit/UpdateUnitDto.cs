using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Unit;
public class UpdateUnitDto
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    public bool IsArchived { get; set; }
}
