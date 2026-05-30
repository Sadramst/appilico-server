using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Waitlist;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Waitlist service interface.</summary>
public interface IWaitlistService
{
    Task<ApiResponse<WaitlistSubscribeResponse>> SubscribeAsync(WaitlistSubscribeRequest request, string? ipAddress = null);
    Task<ApiResponse<int>> GetCountAsync();
    Task<ApiResponse<PagedResult<WaitlistEntryDto>>> GetAdminListAsync(int page, int pageSize, bool? isNotified);
    Task<ApiResponse<WaitlistEntryDto>> NotifyAsync(Guid id);
}
