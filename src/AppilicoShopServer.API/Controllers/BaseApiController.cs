using System.Security.Claims;
using AppilicoShopServer.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace AppilicoShopServer.API.Controllers;

/// <summary>
/// Base controller with common helper methods.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Gets the current user's ID from claims.</summary>
    protected string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    /// <summary>Gets whether the current user has elevated operational access.</summary>
    protected bool IsPrivilegedUser()
    {
        return User.IsInRole(AppConstants.Roles.SuperAdmin)
            || User.IsInRole(AppConstants.Roles.Admin)
            || User.IsInRole(AppConstants.Roles.Manager);
    }

    /// <summary>Gets whether the request has an authenticated principal.</summary>
    protected bool HasAuthenticatedUser()
    {
        return User.Identity?.IsAuthenticated == true;
    }
}
