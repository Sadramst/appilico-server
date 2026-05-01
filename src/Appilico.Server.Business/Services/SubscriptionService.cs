using Microsoft.AspNetCore.Identity;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Subscription;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Services;

/// <summary>Subscription service implementation.</summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly UserManager<AppUser> _userManager;

    public SubscriptionService(UserManager<AppUser> userManager) => _userManager = userManager;

    private static readonly List<SubscriptionPlanDto> Plans = new()
    {
        new SubscriptionPlanDto
        {
            Name = "Starter",
            Price = 299,
            Description = "Perfect for small mining operations getting started with data analytics.",
            BillingCycle = "monthly",
            IsPopular = false,
            Features = new()
            {
                "Up to 5 Power BI custom visuals",
                "Basic mining dashboard templates",
                "Email support",
                "Monthly data refresh",
                "1 user seat"
            }
        },
        new SubscriptionPlanDto
        {
            Name = "Professional",
            Price = 499,
            Description = "Ideal for growing operations that need deeper insights and more visuals.",
            BillingCycle = "monthly",
            IsPopular = true,
            Features = new()
            {
                "Up to 20 Power BI custom visuals",
                "Advanced mining analytics templates",
                "Priority email & chat support",
                "Daily data refresh",
                "5 user seats",
                "AI-powered natural language queries",
                "Custom KPI configuration"
            }
        },
        new SubscriptionPlanDto
        {
            Name = "Enterprise",
            Price = 799,
            Description = "Full suite for large-scale mining enterprises requiring enterprise-grade analytics.",
            BillingCycle = "monthly",
            IsPopular = false,
            Features = new()
            {
                "Unlimited Power BI custom visuals",
                "Enterprise mining analytics suite",
                "Dedicated account manager",
                "Real-time data refresh",
                "Unlimited user seats",
                "AI NL Query with custom training",
                "Custom integrations & API access",
                "SLA guarantee",
                "On-premise deployment option"
            }
        }
    };

    /// <inheritdoc/>
    public ApiResponse<List<SubscriptionPlanDto>> GetPlans()
        => ApiResponse<List<SubscriptionPlanDto>>.SuccessResponse(Plans);

    /// <inheritdoc/>
    public async Task<ApiResponse<CurrentSubscriptionDto>> GetCurrentAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<CurrentSubscriptionDto>.FailResponse("User not found");

        var plan = Plans.FirstOrDefault(p => p.Name == user.SubscriptionPlan) ?? Plans[0];

        var sub = new CurrentSubscriptionDto
        {
            Plan = plan.Name,
            Status = "Active",
            StartDate = user.CreatedAt,
            NextBillingDate = user.CreatedAt.AddMonths(
                (int)Math.Ceiling((DateTime.UtcNow - user.CreatedAt).TotalDays / 30) + 1),
            Price = plan.Price,
            Features = plan.Features
        };

        return ApiResponse<CurrentSubscriptionDto>.SuccessResponse(sub);
    }
}
