namespace Application.DTOs.Unit;
public class UnitResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
