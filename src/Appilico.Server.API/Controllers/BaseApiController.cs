using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Appilico.Server.API.Controllers;

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
}
