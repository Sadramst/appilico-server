using AppilicoShopServer.Domain.Common;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>Represents a user's subscription record.</summary>
public class Subscription : BaseAuditableEntity
{
    /// <summary>Gets or sets the user ID (FK).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the subscription tier.</summary>
    public SubscriptionTier Tier { get; set; }

    /// <summary>Gets or sets the subscription status.</summary>
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    /// <summary>Gets or sets when the subscription started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets the next billing date.</summary>
    public DateTime? NextBillingAt { get; set; }

    /// <summary>Gets or sets when the subscription was cancelled.</summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>Gets or sets the Stripe customer ID.</summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>Gets or sets the Stripe subscription ID.</summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>Gets or sets the Stripe price ID.</summary>
    public string? StripePriceId { get; set; }

    /// <summary>Navigation property for the user.</summary>
    public virtual AppUser? User { get; set; }

    /// <summary>Navigation property for subscription history.</summary>
    public virtual ICollection<SubscriptionHistory> History { get; set; } = new List<SubscriptionHistory>();
}
