using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Subscription;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.Services;

/// <summary>Subscription service implementation.</summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(UserManager<AppUser> userManager, AppDbContext db, ILogger<SubscriptionService> logger)
    {
        _userManager = userManager;
        _db = db;
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
        Plans.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static SubscriptionTier? TierFromName(string name) => name.ToLower() switch
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

        var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        var tierName = user.SubscriptionTier.ToString();
        var plan = PlanByName(tierName) ?? Plans[0];

        return ApiResponse<CurrentSubscriptionDto>.SuccessResponse(new CurrentSubscriptionDto
        {
            Tier = tierName,
            Plan = tierName,
            Status = sub?.Status.ToString() ?? "Active",
            StartedAt = sub?.StartedAt ?? user.CreatedAt,
            StartDate = sub?.StartedAt ?? user.CreatedAt,
            NextBillingAt = sub?.NextBillingAt,
            NextBillingDate = sub?.NextBillingAt,
            Price = plan.MonthlyPrice,
            Features = plan.Features
        });
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CurrentSubscriptionDto>> UpgradeAsync(string userId, UpgradeSubscriptionRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ApiResponse<CurrentSubscriptionDto>.FailResponse("User not found");

        var newTier = TierFromName(request.Plan);
        if (newTier == null)
            return ApiResponse<CurrentSubscriptionDto>.FailResponse($"Unknown subscription plan: {request.Plan}");

        var oldTier = user.SubscriptionTier;

        // TODO: Wire Stripe PaymentIntent before allowing paid upgrades
        user.SubscriptionTier = newTier.Value;
        user.SubscriptionPlan = request.Plan;
        await _userManager.UpdateAsync(user);

        // Upsert Subscription record
        var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sub == null)
        {
            sub = new Subscription
            {
                UserId = userId,
                Tier = newTier.Value,
                Status = SubscriptionStatus.Active,
                StartedAt = DateTime.UtcNow,
                NextBillingAt = DateTime.UtcNow.AddMonths(1)
            };
            _db.Subscriptions.Add(sub);
        }
        else
        {
            sub.Tier = newTier.Value;
            sub.Status = SubscriptionStatus.Active;
            sub.NextBillingAt = DateTime.UtcNow.AddMonths(1);
        }

        _db.SubscriptionHistories.Add(new SubscriptionHistory
        {
            UserId = userId,
            SubscriptionId = sub.Id == Guid.Empty ? Guid.NewGuid() : sub.Id,
            FromTier = oldTier,
            ToTier = newTier.Value,
            ChangedAt = DateTime.UtcNow,
            Reason = "User upgrade",
            ChangedBy = userId
        });

        await _db.SaveChangesAsync();

        _logger.LogInformation("User {UserId} upgraded from {OldTier} to {NewTier}", userId, oldTier, newTier);

        return await GetCurrentAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> CancelAsync(string userId, CancelSubscriptionRequest request)
    {
        var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sub == null) return ApiResponse<bool>.FailResponse("No active subscription found");

        sub.Status = SubscriptionStatus.Cancelled;
        sub.CancelledAt = DateTime.UtcNow;

        _db.SubscriptionHistories.Add(new SubscriptionHistory
        {
            UserId = userId,
            SubscriptionId = sub.Id,
            FromTier = sub.Tier,
            ToTier = sub.Tier,
            ChangedAt = DateTime.UtcNow,
            Reason = request.Reason ?? "User requested cancellation",
            ChangedBy = userId
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("User {UserId} cancelled subscription", userId);
        return ApiResponse<bool>.SuccessResponse(true, "Subscription cancelled");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> HandleStripeWebhookAsync(string payload, string signature)
    {
        // TODO: Implement proper Stripe webhook signature verification
        // TODO: Handle checkout.session.completed → activate subscription
        //       payment_intent.payment_failed → mark PastDue
        //       customer.subscription.deleted → mark Cancelled
        _logger.LogInformation("[Stripe Webhook] Received event — payload length={Len}", payload.Length);
        return await Task.FromResult(ApiResponse<bool>.SuccessResponse(true));
    }
}
