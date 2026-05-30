using AppilicoShopServer.Business.DTOs.Auth;
using AppilicoShopServer.Business.DTOs.Common;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Auth service interface.</summary>
public interface IAuthService
{
    /// <summary>Registers a new user.</summary>
    Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);

    /// <summary>Logs in a user.</summary>
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);

    /// <summary>Refreshes access token.</summary>
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);

    /// <summary>Revokes a refresh token.</summary>
    Task<ApiResponse<bool>> RevokeTokenAsync(string token, string userId);

    /// <summary>Gets user profile.</summary>
    Task<ApiResponse<UserDto>> GetProfileAsync(string userId);

    /// <summary>Updates user profile.</summary>
    Task<ApiResponse<UserDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    /// <summary>Initiates forgot password.</summary>
    Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>Resets password.</summary>
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
}
