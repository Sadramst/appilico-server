namespace Appilico.Server.Business.Interfaces;

/// <summary>Result of creating a Stripe PaymentIntent.</summary>
public record PaymentIntentResult(string ClientSecret, string PaymentIntentId);

/// <summary>Stripe payment service abstraction.</summary>
public interface IStripeService
{
    /// <summary>Creates a Stripe PaymentIntent for the given amount.</summary>
    Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, string orderId);

    /// <summary>Verifies the Stripe-Signature header using the webhook secret.</summary>
    bool VerifyWebhookSignature(string payload, string signature, string secret);
}
