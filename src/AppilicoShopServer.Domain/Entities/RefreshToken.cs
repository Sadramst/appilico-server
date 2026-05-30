using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a JWT refresh token for a user.
/// </summary>
public class RefreshToken : BaseAuditableEntity
{
    /// <summary>Gets or sets the token value.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Gets or sets the token expiry date.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Gets or sets the IP address that created this token.</summary>
    public string? CreatedByIp { get; set; }

    /// <summary>Gets or sets the date the token was revoked.</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Gets or sets the user ID (FK).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Navigation property for the user.</summary>
    public virtual AppUser User { get; set; } = null!;

    /// <summary>Indicates whether the token has expired.</summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>Indicates whether the token is active (not expired and not revoked).</summary>
    public bool IsActive => RevokedAt == null && !IsExpired;
}
