using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Waitlist;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Waitlist service interface.</summary>
public interface IWaitlistService
{
    Task<ApiResponse<WaitlistSubscribeResponse>> SubscribeAsync(WaitlistSubscribeRequest request);
    Task<ApiResponse<int>> GetCountAsync();
}
