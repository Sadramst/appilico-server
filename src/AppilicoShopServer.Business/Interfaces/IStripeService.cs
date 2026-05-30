using System.Text.Json;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Request for creating a Stripe PaymentIntent.</summary>
public sealed record StripePaymentIntentRequest(
    decimal Amount,
    string Currency,
    string Description,
    string IdempotencyKey,
    IReadOnlyDictionary<string, string> Metadata);

/// <summary>Result of creating a Stripe PaymentIntent.</summary>
public sealed record PaymentIntentResult(string ClientSecret, string PaymentIntentId, string Status);

/// <summary>Request for creating a Stripe refund.</summary>
public sealed record StripeRefundRequest(
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    string? Reason,
    string IdempotencyKey);

/// <summary>Result of creating a Stripe refund.</summary>
public sealed record StripeRefundResult(string RefundId, string Status, decimal Amount);

/// <summary>Request for creating a Stripe subscription.</summary>
public sealed record StripeSubscriptionRequest(
    string UserId,
    string Email,
    string PlanName,
    string PriceId,
    string? ExistingCustomerId,
    string IdempotencyKey);

/// <summary>Result of creating or updating a Stripe subscription.</summary>
public sealed record StripeSubscriptionResult(
    string CustomerId,
    string SubscriptionId,
    string PriceId,
    string Status,
    string? ClientSecret,
    DateTime? CurrentPeriodEnd);

/// <summary>Result of cancelling a Stripe subscription.</summary>
public sealed record StripeCancelSubscriptionResult(string SubscriptionId, string Status, DateTime? CurrentPeriodEnd);

/// <summary>Verified Stripe webhook event with provider-neutral payload data.</summary>
public sealed record StripeWebhookEvent(string Id, string Type, string PayloadHash, JsonElement DataObject);

/// <summary>Stripe payment service abstraction.</summary>
public interface IStripeService
{
    /// <summary>Creates a Stripe PaymentIntent for the given amount.</summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(StripePaymentIntentRequest request, CancellationToken cancellationToken = default);

    /// <summary>Creates a Stripe refund for a PaymentIntent.</summary>
    Task<StripeRefundResult> CreateRefundAsync(StripeRefundRequest request, CancellationToken cancellationToken = default);

    /// <summary>Creates an incomplete Stripe subscription and returns any required client secret.</summary>
    Task<StripeSubscriptionResult> CreateSubscriptionAsync(StripeSubscriptionRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cancels a Stripe subscription.</summary>
    Task<StripeCancelSubscriptionResult> CancelSubscriptionAsync(string subscriptionId, string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>Verifies the Stripe-Signature header using the webhook secret.</summary>
    bool VerifyWebhookSignature(string payload, string signature, string secret);

    /// <summary>Constructs a verified webhook event from the raw payload and signature.</summary>
    StripeWebhookEvent ConstructWebhookEvent(string payload, string signature);
}
