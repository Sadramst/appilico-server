namespace Appilico.Server.Business.DTOs.Visual;

/// <summary>DTO for a Power BI custom visual.</summary>
public class VisualDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
}
