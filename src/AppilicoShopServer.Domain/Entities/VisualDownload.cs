using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>Records a visual download event.</summary>
public class VisualDownload : BaseAuditableEntity
{
    /// <summary>Gets or sets the visual ID (FK).</summary>
    public Guid VisualId { get; set; }

    /// <summary>Gets or sets the user ID (FK).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets when the download occurred.</summary>
    public DateTime DownloadedAt { get; set; }

    /// <summary>Gets or sets the IP address of the downloader.</summary>
    public string? IPAddress { get; set; }

    /// <summary>Navigation property for the visual.</summary>
    public virtual Visual? Visual { get; set; }

    /// <summary>Navigation property for the user.</summary>
    public virtual AppUser? User { get; set; }
}
