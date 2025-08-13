using Domain.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Application.Models.Units;

public class UpdateUnitDto : IModel
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsArchived { get; set; }
}
