using Domain.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Application.Models.Resources;

public class UpdateResourceDto : IModel
{
    [Required]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public bool IsArchived { get; set; }
}
