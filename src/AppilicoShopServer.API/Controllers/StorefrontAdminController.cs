using AppilicoShopServer.Business.DTOs.Storefront;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Privileged storefront engine administration controller.</summary>
[ApiController]
[Route("api/storefront/admin")]
[Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
public class StorefrontAdminController : BaseApiController
{
    private readonly IStorefrontService _storefrontService;

    /// <summary>Initializes the storefront admin controller.</summary>
    public StorefrontAdminController(IStorefrontService storefrontService)
    {
        _storefrontService = storefrontService;
    }

    /// <summary>Gets the editable storefront configuration document.</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig([FromQuery] string? storefrontKey, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.GetEditableConfigAsync(storefrontKey, cancellationToken);
        return Ok(result);
    }

    /// <summary>Persists the editable storefront configuration document.</summary>
    [HttpPut("config")]
    public async Task<IActionResult> UpdateConfig([FromBody] StorefrontEditableConfigDto config, [FromQuery] string? storefrontKey, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.UpdateConfigAsync(config, GetUserId(), storefrontKey, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Lists registered storefront tenants.</summary>
    [HttpGet("stores")]
    public async Task<IActionResult> ListStores(CancellationToken cancellationToken)
    {
        var result = await _storefrontService.ListStorefrontsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates or updates a storefront tenant.</summary>
    [HttpPost("stores")]
    public async Task<IActionResult> UpsertStore([FromBody] StorefrontUpsertRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.UpsertStorefrontAsync(request, GetUserId(), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Deletes a non-default storefront tenant.</summary>
    [HttpDelete("stores/{storefrontKey}")]
    [Authorize(Roles = $"{AppConstants.Roles.SuperAdmin},{AppConstants.Roles.Admin}")]
    public async Task<IActionResult> DeleteStore(string storefrontKey, CancellationToken cancellationToken)
    {
        var result = await _storefrontService.DeleteStorefrontAsync(storefrontKey, GetUserId(), cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
