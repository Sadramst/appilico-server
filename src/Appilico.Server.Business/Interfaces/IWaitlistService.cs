using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Waitlist;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Waitlist service interface.</summary>
public interface IWaitlistService
{
    Task<ApiResponse<WaitlistSubscribeResponse>> SubscribeAsync(WaitlistSubscribeRequest request, string? ipAddress = null);
    Task<ApiResponse<int>> GetCountAsync();
    Task<ApiResponse<PagedResult<WaitlistEntryDto>>> GetAdminListAsync(int page, int pageSize, bool? isNotified);
    Task<ApiResponse<WaitlistEntryDto>> NotifyAsync(Guid id);
}
