using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking changes.
/// </summary>
public class AuditLog : BaseAuditableEntity
{
    /// <summary>Gets or sets the user ID who performed the action.</summary>
    public string? UserId { get; set; }

    /// <summary>Gets or sets the action performed.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity name affected.</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Gets or sets the entity ID affected.</summary>
    public string? EntityId { get; set; }

    /// <summary>Gets or sets the old values as JSON.</summary>
    public string? OldValues { get; set; }

    /// <summary>Gets or sets the new values as JSON.</summary>
    public string? NewValues { get; set; }

    /// <summary>Gets or sets when the action occurred.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Gets or sets the IP address of the client.</summary>
    public string? IpAddress { get; set; }
}
