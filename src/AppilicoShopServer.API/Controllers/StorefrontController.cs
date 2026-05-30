using AppilicoShopServer.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Public storefront engine contract controller.</summary>
[AllowAnonymous]
public class StorefrontController : BaseApiController
{
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
        var result = await _storefrontService.GetConfigAsync(cancellationToken);
        return Ok(result);
    }
}
