using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>Represents a waitlist entry.</summary>
public class WaitlistEntry : BaseAuditableEntity
{
    /// <summary>Gets or sets the subscriber email.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the company name.</summary>
    public string? Company { get; set; }

    /// <summary>Gets or sets the subscriber's role.</summary>
    public string? Role { get; set; }

    /// <summary>Gets or sets the desired plan.</summary>
    public string? Plan { get; set; }

    /// <summary>Gets or sets the position in the waitlist.</summary>
    public int Position { get; set; }
}
