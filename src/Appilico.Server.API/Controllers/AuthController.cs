using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Auth;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Authentication controller.</summary>
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    /// <summary>Initializes AuthController.</summary>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Register a new user.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Login.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Refresh token.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Revoke token.</summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        var result = await _authService.RevokeTokenAsync(request.Token, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get profile.</summary>
    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _authService.GetProfileAsync(GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update profile.</summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await _authService.UpdateProfileAsync(GetUserId(), request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Forgot password.</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        return Ok(result);
    }

    /// <summary>Reset password.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
