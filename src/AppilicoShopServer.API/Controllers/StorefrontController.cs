using AppilicoShopServer.Business.DTOs.Storefront;
using AppilicoShopServer.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Public storefront engine contract controller.</summary>
[AllowAnonymous]
public class StorefrontController : BaseApiController
{
    /// <summary>Header clients use to select a storefront tenant.</summary>
    public const string StorefrontKeyHeader = "X-Storefront-Key";

    private readonly IStorefrontService _storefrontService;

    /// <summary>Initializes the storefront controller.</summary>
    public StorefrontController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    /// <summary>Gets reusable client bootstrap configuration.</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetConfigAsync(ResolveStorefrontKey(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets reusable client theme tokens.</summary>
    [HttpGet("theme")]
    public async Task<IActionResult> GetTheme(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetThemeAsync(ResolveStorefrontKey(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets the page-builder homepage layout.</summary>
    [HttpGet("pages/home")]
    public async Task<IActionResult> GetHomePage(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetHomePageAsync(ResolveStorefrontKey(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists registered storefront tenants.</summary>
    [HttpGet("stores")]
    public async Task<IActionResult> GetStores(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.ListStorefrontsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Computes a shipping quote for the current storefront policy.</summary>
    [HttpPost("shipping/quote")]
    public async Task<IActionResult> GetShippingQuote([FromBody] ShippingQuoteRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetShippingQuoteAsync(request, ResolveStorefrontKey(), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Computes a tax quote for the current storefront policy.</summary>
    [HttpPost("tax/quote")]
    public async Task<IActionResult> GetTaxQuote([FromBody] TaxQuoteRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetTaxQuoteAsync(request, ResolveStorefrontKey(), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private string? ResolveStorefrontKey()
    {
        if (Request.Headers.TryGetValue(StorefrontKeyHeader, out var value))
        {
            var key = value.ToString();
            if (!string.IsNullOrWhiteSpace(key))
                return key;
        }

        return null;
    }
}
