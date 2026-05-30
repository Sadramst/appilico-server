using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Business.Options;

/// <summary>Stripe configuration bound from the Stripe section.</summary>
public sealed class StripeOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Stripe";

    /// <summary>Whether Stripe-backed flows are intentionally enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Stripe secret key.</summary>
    public string? SecretKey { get; set; }

    /// <summary>Stripe publishable key.</summary>
    public string? PublishableKey { get; set; }

    /// <summary>Stripe webhook signing secret.</summary>
    public string? WebhookSecret { get; set; }

    /// <summary>Default payment currency.</summary>
    public string Currency { get; set; } = "aud";

    /// <summary>Stripe Price ID for the Starter subscription.</summary>
    public string? StarterPriceId { get; set; }

    /// <summary>Stripe Price ID for the Professional subscription.</summary>
    public string? ProfessionalPriceId { get; set; }

    /// <summary>Stripe Price ID for the Enterprise subscription.</summary>
    public string? EnterprisePriceId { get; set; }

    /// <summary>Allowed Stripe webhook clock tolerance in seconds.</summary>
    public int WebhookToleranceSeconds { get; set; } = 300;

    /// <summary>Checks whether required base Stripe settings are present when enabled.</summary>
    public bool HasRequiredSettings =>
        !Enabled ||
        (!IsPlaceholder(SecretKey)
         && !IsPlaceholder(PublishableKey)
         && !IsPlaceholder(WebhookSecret)
         && !string.IsNullOrWhiteSpace(Currency)
         && WebhookToleranceSeconds > 0);

    /// <summary>Checks whether paid subscription prices are configured when Stripe is enabled.</summary>
    public bool HasSubscriptionSettings =>
        !Enabled ||
        (HasRequiredSettings
         && !IsPlaceholder(StarterPriceId)
         && !IsPlaceholder(ProfessionalPriceId)
         && !IsPlaceholder(EnterprisePriceId));

    /// <summary>Gets the configured Stripe Price ID for a subscription tier.</summary>
    public string? GetPriceId(SubscriptionTier tier) => tier switch
    {
        SubscriptionTier.Starter => StarterPriceId,
        SubscriptionTier.Professional => ProfessionalPriceId,
        SubscriptionTier.Enterprise => EnterprisePriceId,
        _ => null
    };

    private static bool IsPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) ||
        value.Contains("will-be-overridden", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase);
}