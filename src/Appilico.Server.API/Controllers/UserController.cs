using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Auth;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>User profile controller.</summary>
[Route("api/user")]
[Authorize]
public class UserController : BaseApiController
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService) => _authService = authService;

    /// <summary>Get the current user's profile.</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _authService.GetProfileAsync(GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update the current user's profile.</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _authService.UpdateProfileAsync(GetUserId(), request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
