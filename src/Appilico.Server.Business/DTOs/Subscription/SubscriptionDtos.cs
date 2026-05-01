namespace Appilico.Server.Business.DTOs.Subscription;

/// <summary>DTO for a subscription plan tier.</summary>
public class SubscriptionPlanDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = "monthly";
    public List<string> Features { get; set; } = new();
    public bool IsPopular { get; set; }
}

/// <summary>DTO for a user's current subscription.</summary>
public class CurrentSubscriptionDto
{
    public string Plan { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = new();
}
