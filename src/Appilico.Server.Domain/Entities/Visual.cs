using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>Represents a Power BI custom visual offering.</summary>
public class Visual : BaseAuditableEntity
{
    /// <summary>Gets or sets the visual name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL-friendly slug (unique).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the short description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the full/detailed description.</summary>
    public string? FullDescription { get; set; }

    /// <summary>Gets or sets the category (enum: Production/Equipment/Safety/Quality/Finance/AI).</summary>
    public VisualCategory Category { get; set; }

    /// <summary>Gets or sets the visual type string (legacy — e.g., Gantt, Heatmap, KPI).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the thumbnail image URL.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Gets or sets the preview image URL (legacy alias).</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the preview image URLs as a JSON array.</summary>
    public string PreviewImageUrls { get; set; } = "[]";

    /// <summary>Gets or sets the minimum subscription tier required to download.</summary>
    public SubscriptionTier RequiredPlan { get; set; } = SubscriptionTier.Free;

    /// <summary>Gets or sets the total download count.</summary>
    public int DownloadCount { get; set; }

    /// <summary>Gets or sets the tags as a JSON array.</summary>
    public string Tags { get; set; } = "[]";

    /// <summary>Gets or sets the technical specifications.</summary>
    public string? TechnicalSpecs { get; set; }

    /// <summary>Gets or sets data requirements description.</summary>
    public string? DataRequirements { get; set; }

    /// <summary>Gets or sets the required Power BI version.</summary>
    public string? PowerBIVersion { get; set; }

    /// <summary>Gets or sets the demo/sample URL.</summary>
    public string? DemoUrl { get; set; }

    /// <summary>Gets or sets the display sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets whether this visual is active/visible.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property for download records.</summary>
    public virtual ICollection<VisualDownload> Downloads { get; set; } = new List<VisualDownload>();
}
