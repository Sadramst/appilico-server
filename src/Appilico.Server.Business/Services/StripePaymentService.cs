using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Appilico.Server.Business.Exceptions;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Appilico.Server.Business.Services;

/// <summary>Stripe-backed payment service.</summary>
public class StripePaymentService : IStripeService
{
    private readonly StripeOptions _options;
    private readonly ILogger<StripePaymentService> _logger;

    /// <summary>Initialises the Stripe payment service.</summary>
    public StripePaymentService(IOptions<StripeOptions> options, ILogger<StripePaymentService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(StripePaymentIntentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var paymentIntentOptions = new PaymentIntentCreateOptions
        {
            Amount = ToMinorUnits(request.Amount, request.Currency),
            Currency = request.Currency.ToLowerInvariant(),
            Description = request.Description,
            Metadata = request.Metadata.ToDictionary(pair => pair.Key, pair => pair.Value),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
        };

        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(
                paymentIntentOptions,
                BuildRequestOptions(request.IdempotencyKey),
                cancellationToken);

            return new PaymentIntentResult(paymentIntent.ClientSecret, paymentIntent.Id, paymentIntent.Status);
        }
        catch (StripeException ex)
        {
            throw MapStripeException(ex, "Payment provider could not create the payment intent.");
        }
    }

    /// <inheritdoc/>
    public async Task<StripeRefundResult> CreateRefundAsync(StripeRefundRequest request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var refundOptions = new RefundCreateOptions
        {
            PaymentIntent = request.PaymentIntentId,
            Amount = ToMinorUnits(request.Amount, request.Currency),
            Reason = MapRefundReason(request.Reason),
            Metadata = new Dictionary<string, string>
            {
                ["reason"] = request.Reason ?? "unspecified"
            }
        };

        try
        {
            var service = new RefundService();
            var refund = await service.CreateAsync(
                refundOptions,
                BuildRequestOptions(request.IdempotencyKey),
                cancellationToken);

            return new StripeRefundResult(refund.Id, refund.Status, request.Amount);
        }
        catch (StripeException ex)
        {
            throw MapStripeException(ex, "Payment provider could not create the refund.");
        }
    }

    /// <inheritdoc/>
    public async Task<StripeSubscriptionResult> CreateSubscriptionAsync(StripeSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            var customerId = request.ExistingCustomerId;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                var customerService = new Stripe.CustomerService();
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = request.Email,
                    Metadata = new Dictionary<string, string>
                    {
                        ["userId"] = request.UserId
                    }
                }, BuildRequestOptions($"customer:{request.UserId}"), cancellationToken);

                customerId = customer.Id;
            }

            var subscriptionService = new Stripe.SubscriptionService();
            var subscription = await subscriptionService.CreateAsync(new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new() { Price = request.PriceId }
                },
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription"
                },
                Expand = new List<string> { "latest_invoice.payment_intent" },
                Metadata = new Dictionary<string, string>
                {
                    ["userId"] = request.UserId,
                    ["plan"] = request.PlanName
                }
            }, BuildRequestOptions(request.IdempotencyKey), cancellationToken);

            return new StripeSubscriptionResult(
                customerId,
                subscription.Id,
                request.PriceId,
                subscription.Status,
                TryGetNestedClientSecret(subscription),
                TryGetDateTime(subscription, "CurrentPeriodEnd"));
        }
        catch (StripeException ex)
        {
            throw MapStripeException(ex, "Payment provider could not create the subscription.");
        }
    }

    /// <inheritdoc/>
    public async Task<StripeCancelSubscriptionResult> CancelSubscriptionAsync(string subscriptionId, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        try
        {
            var service = new Stripe.SubscriptionService();
            var subscription = await service.CancelAsync(subscriptionId, requestOptions: BuildRequestOptions(idempotencyKey), cancellationToken: cancellationToken);
            return new StripeCancelSubscriptionResult(subscription.Id, subscription.Status, TryGetDateTime(subscription, "CurrentPeriodEnd"));
        }
        catch (StripeException ex)
        {
            throw MapStripeException(ex, "Payment provider could not cancel the subscription.");
        }
    }

    /// <inheritdoc/>
    public bool VerifyWebhookSignature(string payload, string signature, string secret)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(secret))
            return false;

        try
        {
            EventUtility.ConstructEvent(payload, signature, secret, tolerance: _options.WebhookToleranceSeconds);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return false;
        }
    }

    /// <inheritdoc/>
    public StripeWebhookEvent ConstructWebhookEvent(string payload, string signature)
    {
        EnsureConfigured();

        try
        {
            EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, tolerance: _options.WebhookToleranceSeconds);

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var id = root.GetProperty("id").GetString();
            var type = root.GetProperty("type").GetString();
            var dataObject = root.GetProperty("data").GetProperty("object").Clone();

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(type))
                throw new PaymentProviderException("Webhook payload is missing required event fields.");

            return new StripeWebhookEvent(id, type, ComputeSha256(payload), dataObject);
        }
        catch (StripeException ex)
        {
            throw MapStripeException(ex, "Webhook signature verification failed.");
        }
        catch (JsonException ex)
        {
            throw new PaymentProviderException("Webhook payload is not valid JSON.", innerException: ex);
        }
    }

    private void EnsureConfigured()
    {
        if (!_options.Enabled || !_options.HasRequiredSettings)
            throw new PaymentProviderException("Stripe is not configured for this environment.");
    }

    private RequestOptions BuildRequestOptions(string idempotencyKey) => new()
    {
        ApiKey = _options.SecretKey,
        IdempotencyKey = idempotencyKey
    };

    private static long ToMinorUnits(decimal amount, string currency)
    {
        var multiplier = IsZeroDecimalCurrency(currency) ? 1 : 100;
        return decimal.ToInt64(decimal.Round(amount * multiplier, 0, MidpointRounding.AwayFromZero));
    }

    private static bool IsZeroDecimalCurrency(string currency)
    {
        return string.Equals(currency, "jpy", StringComparison.OrdinalIgnoreCase)
            || string.Equals(currency, "krw", StringComparison.OrdinalIgnoreCase);
    }

    private static string? MapRefundReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return null;

        return reason.Trim().ToLowerInvariant() switch
        {
            "duplicate" => "duplicate",
            "fraudulent" => "fraudulent",
            "requested_by_customer" => "requested_by_customer",
            _ => "requested_by_customer"
        };
    }

    private static PaymentProviderException MapStripeException(StripeException ex, string fallbackMessage)
    {
        var safeMessage = ex.StripeError?.Type == "card_error" && !string.IsNullOrWhiteSpace(ex.StripeError.Message)
            ? ex.StripeError.Message
            : fallbackMessage;

        return new PaymentProviderException(safeMessage, innerException: ex);
    }

    private static string ComputeSha256(string payload)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? TryGetNestedClientSecret(object instance)
    {
        var latestInvoice = instance.GetType().GetProperty("LatestInvoice")?.GetValue(instance);
        var paymentIntent = latestInvoice?.GetType().GetProperty("PaymentIntent")?.GetValue(latestInvoice);
        return paymentIntent?.GetType().GetProperty("ClientSecret")?.GetValue(paymentIntent) as string;
    }

    private static DateTime? TryGetDateTime(object instance, string propertyName)
    {
        var value = instance.GetType().GetProperty(propertyName)?.GetValue(instance);
        return value is DateTime dateTime ? dateTime : null;
    }
}
