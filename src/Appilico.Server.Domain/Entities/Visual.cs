using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>Represents a Power BI custom visual offering.</summary>
public class Visual : BaseAuditableEntity
{
    /// <summary>Gets or sets the visual name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the category (e.g., Mining, Operations).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the visual type (e.g., Gantt, Heatmap, KPI).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the preview image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the demo/sample URL.</summary>
    public string? DemoUrl { get; set; }

    /// <summary>Gets or sets the display sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets whether this visual is active/visible.</summary>
    public bool IsActive { get; set; } = true;
}
