using Appilico.Server.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appilico.Server.Business.Services;

/// <summary>
/// Stripe payment service stub.
/// TODO: Install Stripe.net package and replace stubs with real Stripe API calls.
/// Wire Stripe PaymentIntent before allowing paid upgrades.
/// </summary>
public class StripePaymentService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    /// <summary>Initialises the Stripe payment service.</summary>
    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, string orderId)
    {
        // TODO: Replace with StripeClient.PaymentIntents.CreateAsync
        _logger.LogWarning("[Stripe] CreatePaymentIntent stub: amount={Amount} {Currency} orderId={OrderId}", amount, currency, orderId);
        var stubClientSecret = $"pi_stub_{orderId}_secret_stub";
        var stubId = $"pi_stub_{orderId}";
        return Task.FromResult(new PaymentIntentResult(stubClientSecret, stubId));
    }

    /// <inheritdoc/>
    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        // TODO: Replace with Stripe.EventUtility.ConstructEvent (HMAC-SHA256 verification)
        _logger.LogWarning("[Stripe] VerifyWebhookSignature stub called");
        return !string.IsNullOrEmpty(signature);
    }
}
