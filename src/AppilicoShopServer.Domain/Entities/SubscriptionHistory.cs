using AppilicoShopServer.Domain.Common;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>Records every tier change for a user's subscription.</summary>
public class SubscriptionHistory : BaseAuditableEntity
{
    /// <summary>Gets or sets the user ID (FK).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the subscription ID (FK).</summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>Gets or sets the previous tier.</summary>
    public SubscriptionTier FromTier { get; set; }

    /// <summary>Gets or sets the new tier.</summary>
    public SubscriptionTier ToTier { get; set; }

    /// <summary>Gets or sets when the change occurred.</summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>Gets or sets the reason for the change.</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets who made the change (userId or "system").</summary>
    public string? ChangedBy { get; set; }

    /// <summary>Navigation property for the subscription.</summary>
    public virtual Subscription? Subscription { get; set; }
}
