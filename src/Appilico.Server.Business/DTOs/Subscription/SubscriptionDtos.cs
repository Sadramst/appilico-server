namespace Appilico.Server.Business.DTOs.Subscription;

/// <summary>DTO for a subscription plan tier.</summary>
public class SubscriptionPlanDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    /// <summary>Legacy monthly price field.</summary>
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = "monthly";
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; }
}

/// <summary>DTO for a user's current subscription.</summary>
public class CurrentSubscriptionDto
{
    public string Tier { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? NextBillingAt { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = new();
    public bool RequiresPayment { get; set; }
    public string? PaymentClientSecret { get; set; }
    public string? PendingTier { get; set; }
    public string? ProviderStatus { get; set; }
    public string? ProviderSubscriptionId { get; set; }
}

/// <summary>Request DTO for upgrading a subscription.</summary>
public class UpgradeSubscriptionRequest
{
    public string Plan { get; set; } = string.Empty;
}

/// <summary>Request DTO for cancelling a subscription.</summary>
public class CancelSubscriptionRequest
{
    public string? Reason { get; set; }
}
