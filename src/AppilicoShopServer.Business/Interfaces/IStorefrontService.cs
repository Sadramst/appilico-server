using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Storefront;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Public storefront engine contract service.</summary>
public interface IStorefrontService
{
    /// <summary>Gets the storefront bootstrap configuration for reusable clients.</summary>
    Task<ApiResponse<StorefrontConfigDto>> GetConfigAsync(CancellationToken cancellationToken = default);
}
