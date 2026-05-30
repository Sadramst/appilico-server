using System.Text.Json;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Subscription;
using AppilicoShopServer.Business.Exceptions;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppilicoShopServer.Business.Services;

/// <summary>Subscription service implementation.</summary>
public class SubscriptionService : ISubscriptionService
{
    private const string StripeProvider = "stripe";
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStripeService _stripeService;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        IStripeService stripeService,
        IOptions<StripeOptions> stripeOptions,
        ILogger<SubscriptionService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _stripeService = stripeService;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
    }

    private static readonly List<SubscriptionPlanDto> Plans = new()
    {
        new SubscriptionPlanDto
        {
            Name = "Starter",
            MonthlyPrice = 299,
            AnnualPrice = 2990,
            Price = 299,
            Description = "Perfect for small mining operations getting started with data analytics.",
            BillingCycle = "monthly",
            IsPopular = false,
            Features = new() { "5 visuals", "1 data source", "Email support" }
        },
        new SubscriptionPlanDto
        {
            Name = "Professional",
            MonthlyPrice = 499,
            AnnualPrice = 4990,
            Price = 499,
            Description = "Ideal for growing operations that need deeper insights.",
            BillingCycle = "monthly",
            IsPopular = true,
            Features = new() { "15 visuals", "3 data sources", "AI query", "Priority support" }
        },
        new SubscriptionPlanDto
        {
            Name = "Enterprise",
            MonthlyPrice = 799,
            AnnualPrice = 7990,
            Price = 799,
            Description = "Full suite for large-scale mining enterprises.",
            BillingCycle = "monthly",
            IsPopular = false,
            Features = new() { "Unlimited visuals", "Unlimited sources", "Full AI suite", "SLA", "Custom visuals" }
        }
    };

    private static SubscriptionPlanDto? PlanByName(string name) =>
        Plans.FirstOrDefault(plan => plan.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static SubscriptionTier? TierFromName(string name) => name.ToLowerInvariant() switch
    {
        "starter" => SubscriptionTier.Starter,
        "professional" => SubscriptionTier.Professional,
        "enterprise" => SubscriptionTier.Enterprise,
        "free" => SubscriptionTier.Free,
        _ => null
    };

    /// <inheritdoc/>
    public ApiResponse<List<SubscriptionPlanDto>> GetPlans()
        => ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(Plans);

    /// <inheritdoc/>
    public async Task<ApiResponse<CurrentSubscriptionDto>> GetCurrentAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<CurrentSubscriptionDto>.FailResponse("User not found");

        var subscription = await _unitOfWork.Subscriptions.GetByUserIdAsync(userId);
        return ApiResponse<CurrentSubscriptionDto>.SuccessResponse(BuildCurrentSubscriptionDto(user, subscription));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CurrentSubscriptionDto>> UpgradeAsync(string userId, UpgradeSubscriptionRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<CurrentSubscriptionDto>.FailResponse("User not found");

        var newTier = TierFromName(request.Plan);
        if (newTier == null)
            return ApiResponse<CurrentSubscriptionDto>.FailResponse($"Unknown subscription plan: {request.Plan}");

        var subscription = await _unitOfWork.Subscriptions.GetByUserIdAsync(userId);
        if (newTier.Value == SubscriptionTier.Free)
            return await ApplyFreeTierAsync(user, subscription, request.Plan);

        if (!_stripeOptions.Enabled || !_stripeOptions.HasSubscriptionSettings)
            return ApiResponse<CurrentSubscriptionDto>.FailResponse("Paid subscription upgrades are unavailable because Stripe is not configured.");

        var priceId = _stripeOptions.GetPriceId(newTier.Value);
        if (string.IsNullOrWhiteSpace(priceId))
            return ApiResponse<CurrentSubscriptionDto>.FailResponse($"Stripe price is not configured for {newTier.Value}.");

        try
        {
            var providerResult = await _stripeService.CreateSubscriptionAsync(new StripeSubscriptionRequest(
                user.Id,
                user.Email ?? user.UserName ?? string.Empty,
                newTier.Value.ToString(),
                priceId,
                subscription?.StripeCustomerId,
                $"subscription:{user.Id}:{newTier.Value}"));

            subscription ??= new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                StartedAt = DateTime.UtcNow
            };

            var oldTier = user.SubscriptionTier;
            subscription.Tier = newTier.Value;
            subscription.Status = MapStripeSubscriptionStatus(providerResult.Status);
            subscription.StripeCustomerId = providerResult.CustomerId;
            subscription.StripeSubscriptionId = providerResult.SubscriptionId;
            subscription.StripePriceId = providerResult.PriceId;
            subscription.NextBillingAt = providerResult.CurrentPeriodEnd;

            if (subscription.CreatedAt == default)
                await _unitOfWork.Subscriptions.AddAsync(subscription);

            if (IsActiveProviderStatus(providerResult.Status))
                await ApplyUserTierAsync(user, newTier.Value);

            await _unitOfWork.Subscriptions.AddHistoryAsync(new SubscriptionHistory
            {
                UserId = user.Id,
                SubscriptionId = subscription.Id,
                FromTier = oldTier,
                ToTier = newTier.Value,
                ChangedAt = DateTime.UtcNow,
                Reason = IsActiveProviderStatus(providerResult.Status) ? "Stripe subscription active" : "Stripe subscription created; awaiting payment confirmation",
                ChangedBy = user.Id
            });

            await _unitOfWork.SaveChangesAsync();

            var current = BuildCurrentSubscriptionDto(
                user,
                subscription,
                providerResult.ClientSecret,
                IsActiveProviderStatus(providerResult.Status) ? null : newTier.Value.ToString(),
                providerResult.Status);

            return ApiResponse<CurrentSubscriptionDto>.SuccessResponse(current, "Subscription update started");
        }
        catch (PaymentProviderException ex)
        {
            _logger.LogWarning(ex, "Stripe subscription creation failed for user {UserId}. ProviderRequestId={ProviderRequestId}", userId, ex.ProviderRequestId);
            return ApiResponse<CurrentSubscriptionDto>.FailResponse(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> CancelAsync(string userId, CancelSubscriptionRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<bool>.FailResponse("User not found");

        var subscription = await _unitOfWork.Subscriptions.GetByUserIdAsync(userId);
        if (subscription == null) return ApiResponse<bool>.FailResponse("No active subscription found");

        if (!string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId))
        {
            if (!_stripeOptions.Enabled || !_stripeOptions.HasRequiredSettings)
                return ApiResponse<bool>.FailResponse("Stripe is not configured, so this subscription cannot be cancelled automatically.");

            try
            {
                var providerResult = await _stripeService.CancelSubscriptionAsync(
                    subscription.StripeSubscriptionId,
                    $"subscription-cancel:{subscription.StripeSubscriptionId}");

                if (!string.Equals(providerResult.Status, "canceled", StringComparison.OrdinalIgnoreCase))
                    return ApiResponse<bool>.FailResponse("Subscription cancellation has not been confirmed by Stripe.");

                subscription.NextBillingAt = providerResult.CurrentPeriodEnd;
            }
            catch (PaymentProviderException ex)
            {
                _logger.LogWarning(ex, "Stripe subscription cancellation failed for user {UserId}. ProviderRequestId={ProviderRequestId}", userId, ex.ProviderRequestId);
                return ApiResponse<bool>.FailResponse(ex.Message);
            }
        }

        var oldTier = user.SubscriptionTier;
        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        await ApplyUserTierAsync(user, SubscriptionTier.Free);

        await _unitOfWork.Subscriptions.AddHistoryAsync(new SubscriptionHistory
        {
            UserId = userId,
            SubscriptionId = subscription.Id,
            FromTier = oldTier,
            ToTier = SubscriptionTier.Free,
            ChangedAt = DateTime.UtcNow,
            Reason = request.Reason ?? "User requested cancellation",
            ChangedBy = userId
        });

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("User {UserId} cancelled subscription", userId);
        return ApiResponse<bool>.SuccessResponse(true, "Subscription cancelled");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> HandleStripeWebhookAsync(string payload, string signature)
    {
        StripeWebhookEvent webhookEvent;
        try
        {
            webhookEvent = _stripeService.ConstructWebhookEvent(payload, signature);
        }
        catch (PaymentProviderException ex)
        {
            _logger.LogWarning(ex, "Rejected Stripe webhook: signature or payload validation failed");
            return ApiResponse<bool>.FailResponse("Invalid Stripe webhook.");
        }

        if (await _unitOfWork.ExternalWebhookEvents.HasProcessedAsync(StripeProvider, webhookEvent.Id))
        {
            _logger.LogInformation("Ignoring duplicate Stripe webhook {EventId} ({EventType})", webhookEvent.Id, webhookEvent.Type);
            return ApiResponse<bool>.SuccessResponse(true, "Webhook already processed");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await ProcessWebhookEventAsync(webhookEvent);

            await _unitOfWork.ExternalWebhookEvents.AddAsync(new ExternalWebhookEvent
            {
                Provider = StripeProvider,
                EventId = webhookEvent.Id,
                EventType = webhookEvent.Type,
                PayloadHash = webhookEvent.PayloadHash,
                ProcessedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Webhook processed");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Failed to process Stripe webhook {EventId} ({EventType})", webhookEvent.Id, webhookEvent.Type);
            return ApiResponse<bool>.FailResponse("Stripe webhook processing failed.");
        }
    }

    private async Task<ApiResponse<CurrentSubscriptionDto>> ApplyFreeTierAsync(AppUser user, Subscription? subscription, string requestedPlan)
    {
        if (subscription != null && !string.IsNullOrWhiteSpace(subscription.StripeSubscriptionId) && subscription.Status != SubscriptionStatus.Cancelled)
        {
            var cancelResult = await CancelAsync(user.Id, new CancelSubscriptionRequest { Reason = "Downgrade to free plan" });
            if (!cancelResult.Success)
                return ApiResponse<CurrentSubscriptionDto>.FailResponse(cancelResult.Message);
        }

        var oldTier = user.SubscriptionTier;
        await ApplyUserTierAsync(user, SubscriptionTier.Free, requestedPlan);

        subscription ??= new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            StartedAt = DateTime.UtcNow
        };

        subscription.Tier = SubscriptionTier.Free;
        subscription.Status = SubscriptionStatus.Active;
        subscription.NextBillingAt = null;

        if (subscription.CreatedAt == default)
            await _unitOfWork.Subscriptions.AddAsync(subscription);

        await _unitOfWork.Subscriptions.AddHistoryAsync(new SubscriptionHistory
        {
            UserId = user.Id,
            SubscriptionId = subscription.Id,
            FromTier = oldTier,
            ToTier = SubscriptionTier.Free,
            ChangedAt = DateTime.UtcNow,
            Reason = "Free plan selected",
            ChangedBy = user.Id
        });

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<CurrentSubscriptionDto>.SuccessResponse(BuildCurrentSubscriptionDto(user, subscription), "Subscription updated");
    }

    private async Task ProcessWebhookEventAsync(StripeWebhookEvent webhookEvent)
    {
        switch (webhookEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentIntentAsync(webhookEvent.DataObject, PaymentStatus.Paid);
                break;
            case "payment_intent.payment_failed":
            case "payment_intent.canceled":
                await HandlePaymentIntentAsync(webhookEvent.DataObject, PaymentStatus.Failed);
                break;
            case "charge.refunded":
                await HandleChargeRefundedAsync(webhookEvent.DataObject);
                break;
            case "customer.subscription.created":
            case "customer.subscription.updated":
                await HandleSubscriptionUpsertAsync(webhookEvent.DataObject);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(webhookEvent.DataObject);
                break;
            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(webhookEvent.DataObject);
                break;
            default:
                _logger.LogInformation("Stripe webhook {EventType} verified but has no local handler", webhookEvent.Type);
                break;
        }
    }

    private async Task HandlePaymentIntentAsync(JsonElement dataObject, PaymentStatus status)
    {
        var paymentIntentId = GetString(dataObject, "id");
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            return;

        var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(paymentIntentId);
        if (payment == null)
        {
            _logger.LogWarning("Stripe PaymentIntent {PaymentIntentId} has no matching local payment", paymentIntentId);
            return;
        }

        payment.Status = status;
        payment.PaidAt = status == PaymentStatus.Paid ? DateTime.UtcNow : payment.PaidAt;

        var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
        if (order != null)
        {
            order.PaymentStatus = status;
            _unitOfWork.Orders.Update(order);
        }

        _unitOfWork.Payments.Update(payment);
    }

    private async Task HandleChargeRefundedAsync(JsonElement dataObject)
    {
        var paymentIntentId = GetString(dataObject, "payment_intent");
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            return;

        var payment = await _unitOfWork.Payments.GetByTransactionIdAsync(paymentIntentId);
        if (payment == null)
            return;

        var amountRefunded = GetMinorAmountAsDecimal(dataObject, "amount_refunded", _stripeOptions.Currency);
        payment.Status = amountRefunded >= payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;

        var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
        if (order != null)
        {
            order.PaymentStatus = payment.Status;
            if (payment.Status == PaymentStatus.Refunded)
                order.OrderStatus = OrderStatus.Refunded;
            _unitOfWork.Orders.Update(order);
        }

        _unitOfWork.Payments.Update(payment);
    }

    private async Task HandleSubscriptionUpsertAsync(JsonElement dataObject)
    {
        var stripeSubscriptionId = GetString(dataObject, "id");
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            return;

        var status = GetString(dataObject, "status") ?? string.Empty;
        var customerId = GetString(dataObject, "customer");
        var userId = GetMetadataValue(dataObject, "userId");
        var priceId = GetSubscriptionPriceId(dataObject);
        var tier = TierFromPriceId(priceId);

        var subscription = await _unitOfWork.Subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription == null && !string.IsNullOrWhiteSpace(userId))
            subscription = await _unitOfWork.Subscriptions.GetByUserIdAsync(userId);

        if (subscription == null || tier == null)
        {
            _logger.LogWarning("Stripe subscription {StripeSubscriptionId} could not be matched to a local user/tier", stripeSubscriptionId);
            return;
        }

        var oldTier = subscription.Tier;
        subscription.Tier = tier.Value;
        subscription.Status = MapStripeSubscriptionStatus(status);
        subscription.StripeCustomerId = customerId ?? subscription.StripeCustomerId;
        subscription.StripeSubscriptionId = stripeSubscriptionId;
        subscription.StripePriceId = priceId ?? subscription.StripePriceId;
        subscription.NextBillingAt = GetUnixDateTime(dataObject, "current_period_end");

        var user = await _userManager.FindByIdAsync(subscription.UserId);
        if (user != null)
        {
            if (IsActiveProviderStatus(status))
                await ApplyUserTierAsync(user, tier.Value);
            else if (subscription.Status == SubscriptionStatus.Cancelled)
                await ApplyUserTierAsync(user, SubscriptionTier.Free);
        }

        if (oldTier != tier.Value && IsActiveProviderStatus(status))
        {
            await _unitOfWork.Subscriptions.AddHistoryAsync(new SubscriptionHistory
            {
                UserId = subscription.UserId,
                SubscriptionId = subscription.Id,
                FromTier = oldTier,
                ToTier = tier.Value,
                ChangedAt = DateTime.UtcNow,
                Reason = "Stripe webhook subscription update",
                ChangedBy = "stripe"
            });
        }
    }

    private async Task HandleSubscriptionDeletedAsync(JsonElement dataObject)
    {
        var stripeSubscriptionId = GetString(dataObject, "id");
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            return;

        var subscription = await _unitOfWork.Subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription == null)
            return;

        var oldTier = subscription.Tier;
        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;

        var user = await _userManager.FindByIdAsync(subscription.UserId);
        if (user != null)
            await ApplyUserTierAsync(user, SubscriptionTier.Free);

        await _unitOfWork.Subscriptions.AddHistoryAsync(new SubscriptionHistory
        {
            UserId = subscription.UserId,
            SubscriptionId = subscription.Id,
            FromTier = oldTier,
            ToTier = SubscriptionTier.Free,
            ChangedAt = DateTime.UtcNow,
            Reason = "Stripe webhook subscription deleted",
            ChangedBy = "stripe"
        });
    }

    private async Task HandleInvoicePaymentFailedAsync(JsonElement dataObject)
    {
        var stripeSubscriptionId = GetString(dataObject, "subscription");
        if (string.IsNullOrWhiteSpace(stripeSubscriptionId))
            return;

        var subscription = await _unitOfWork.Subscriptions.GetByStripeSubscriptionIdAsync(stripeSubscriptionId);
        if (subscription != null)
            subscription.Status = SubscriptionStatus.PastDue;
    }

    private async Task ApplyUserTierAsync(AppUser user, SubscriptionTier tier, string? planName = null)
    {
        user.SubscriptionTier = tier;
        user.SubscriptionPlan = planName ?? tier.ToString();
        await _userManager.UpdateAsync(user);
    }

    private CurrentSubscriptionDto BuildCurrentSubscriptionDto(
        AppUser user,
        Subscription? subscription,
        string? paymentClientSecret = null,
        string? pendingTier = null,
        string? providerStatus = null)
    {
        var tierName = user.SubscriptionTier.ToString();
        var plan = PlanByName(tierName) ?? Plans[0];

        return new CurrentSubscriptionDto
        {
            Tier = tierName,
            Plan = tierName,
            Status = subscription?.Status.ToString() ?? "Active",
            StartedAt = subscription?.StartedAt ?? user.CreatedAt,
            StartDate = subscription?.StartedAt ?? user.CreatedAt,
            NextBillingAt = subscription?.NextBillingAt,
            NextBillingDate = subscription?.NextBillingAt,
            Price = plan.MonthlyPrice,
            Features = plan.Features,
            RequiresPayment = !string.IsNullOrWhiteSpace(paymentClientSecret),
            PaymentClientSecret = paymentClientSecret,
            PendingTier = pendingTier,
            ProviderStatus = providerStatus,
            ProviderSubscriptionId = subscription?.StripeSubscriptionId
        };
    }

    private static SubscriptionStatus MapStripeSubscriptionStatus(string? status) => status?.ToLowerInvariant() switch
    {
        "active" or "trialing" => SubscriptionStatus.Active,
        "canceled" or "incomplete_expired" => SubscriptionStatus.Cancelled,
        "past_due" or "unpaid" => SubscriptionStatus.PastDue,
        _ => SubscriptionStatus.Incomplete
    };

    private static bool IsActiveProviderStatus(string? status)
    {
        return string.Equals(status, "active", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "trialing", StringComparison.OrdinalIgnoreCase);
    }

    private SubscriptionTier? TierFromPriceId(string? priceId)
    {
        if (string.IsNullOrWhiteSpace(priceId))
            return null;

        if (priceId == _stripeOptions.StarterPriceId) return SubscriptionTier.Starter;
        if (priceId == _stripeOptions.ProfessionalPriceId) return SubscriptionTier.Professional;
        if (priceId == _stripeOptions.EnterprisePriceId) return SubscriptionTier.Enterprise;
        return null;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string? GetMetadataValue(JsonElement element, string key)
    {
        if (!element.TryGetProperty("metadata", out var metadata) || metadata.ValueKind != JsonValueKind.Object)
            return null;

        return metadata.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? GetSubscriptionPriceId(JsonElement element)
    {
        if (!element.TryGetProperty("items", out var items) || !items.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
            return null;

        var firstItem = data.EnumerateArray().FirstOrDefault();
        if (firstItem.ValueKind == JsonValueKind.Undefined || !firstItem.TryGetProperty("price", out var price))
            return null;

        return GetString(price, "id");
    }

    private static DateTime? GetUnixDateTime(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(property.GetInt64()).UtcDateTime;
    }

    private static decimal GetMinorAmountAsDecimal(JsonElement element, string propertyName, string currency)
    {
        if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number)
            return 0;

        var amount = property.GetDecimal();
        var divisor = string.Equals(currency, "jpy", StringComparison.OrdinalIgnoreCase) || string.Equals(currency, "krw", StringComparison.OrdinalIgnoreCase)
            ? 1m
            : 100m;
        return amount / divisor;
    }
}
