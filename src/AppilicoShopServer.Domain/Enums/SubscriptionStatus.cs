namespace AppilicoShopServer.Domain.Enums;

/// <summary>Status of a user subscription.</summary>
public enum SubscriptionStatus
{
    /// <summary>Subscription is active.</summary>
    Active = 0,

    /// <summary>Subscription has been cancelled.</summary>
    Cancelled = 1,

    /// <summary>Payment failed; subscription is past due.</summary>
    PastDue = 2,

    /// <summary>Subscription has been created but requires payment confirmation.</summary>
    Incomplete = 3
}
