using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>Represents a newsletter subscriber.</summary>
public class NewsletterSubscriber : BaseAuditableEntity
{
    /// <summary>Gets or sets the subscriber email (unique).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the source (e.g., "footer", "blog", "popup").</summary>
    public string? Source { get; set; }

    /// <summary>Gets or sets whether the subscriber is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets when the subscriber subscribed.</summary>
    public DateTime SubscribedAt { get; set; }

    /// <summary>Gets or sets when the subscriber unsubscribed.</summary>
    public DateTime? UnsubscribedAt { get; set; }
}
