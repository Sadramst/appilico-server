using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Subscription;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Subscription service interface.</summary>
public interface ISubscriptionService
{
    Task<ApiResponse<CurrentSubscriptionDto>> GetCurrentAsync(string userId);
    ApiResponse<List<SubscriptionPlanDto>> GetPlans();
}
