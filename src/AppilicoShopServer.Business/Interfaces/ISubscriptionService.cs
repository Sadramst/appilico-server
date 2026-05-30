using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Subscription;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Subscription service interface.</summary>
public interface ISubscriptionService
{
    Task<ApiResponse<CurrentSubscriptionDto>> GetCurrentAsync(string userId);
    ApiResponse<List<SubscriptionPlanDto>> GetPlans();
    Task<ApiResponse<CurrentSubscriptionDto>> UpgradeAsync(string userId, UpgradeSubscriptionRequest request);
    Task<ApiResponse<bool>> CancelAsync(string userId, CancelSubscriptionRequest request);
    Task<ApiResponse<bool>> HandleStripeWebhookAsync(string payload, string signature);
}
