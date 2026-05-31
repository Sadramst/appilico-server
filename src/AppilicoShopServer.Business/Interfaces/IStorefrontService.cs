using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Storefront;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Public storefront engine contract service.</summary>
public interface IStorefrontService
{
    /// <summary>Gets the storefront bootstrap configuration for reusable clients.</summary>
    Task<ApiResponse<StorefrontConfigDto>> GetConfigAsync(string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Gets storefront theme tokens for reusable clients.</summary>
    Task<ApiResponse<StorefrontThemeDto>> GetThemeAsync(string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the writable storefront configuration document (effective defaults merged with persisted overrides).</summary>
    Task<ApiResponse<StorefrontEditableConfigDto>> GetEditableConfigAsync(string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Persists the writable storefront configuration document.</summary>
    Task<ApiResponse<StorefrontEditableConfigDto>> UpdateConfigAsync(StorefrontEditableConfigDto config, string updatedBy, string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Computes a shipping quote using the storefront shipping policy.</summary>
    Task<ApiResponse<ShippingQuoteResultDto>> GetShippingQuoteAsync(ShippingQuoteRequestDto request, string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Computes a tax quote using the storefront tax policy.</summary>
    Task<ApiResponse<TaxQuoteResultDto>> GetTaxQuoteAsync(TaxQuoteRequestDto request, string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the page-builder homepage layout for a storefront.</summary>
    Task<ApiResponse<StorefrontHomePageDto>> GetHomePageAsync(string? storefrontKey = null, CancellationToken cancellationToken = default);

    /// <summary>Lists all registered storefront tenants.</summary>
    Task<ApiResponse<List<StorefrontSummaryDto>>> ListStorefrontsAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates or updates a storefront tenant.</summary>
    Task<ApiResponse<StorefrontSummaryDto>> UpsertStorefrontAsync(StorefrontUpsertRequestDto request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>Deletes a non-default storefront tenant and its persisted configuration.</summary>
    Task<ApiResponse<bool>> DeleteStorefrontAsync(string storefrontKey, string updatedBy, CancellationToken cancellationToken = default);
}
