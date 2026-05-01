namespace Appilico.Server.Business.DTOs.Auth;

/// <summary>DTO for login request.</summary>
public class LoginRequest
{
    /// <summary>Gets or sets the email.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the password.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>DTO for registration request.</summary>
public class RegisterRequest
{
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the email.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the company or organisation.</summary>
    public string? Company { get; set; }
    /// <summary>Gets or sets the phone number.</summary>
    public string? Phone { get; set; }
    /// <summary>Gets or sets the phone number (alias).</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Gets or sets the password.</summary>
    public string Password { get; set; } = string.Empty;
    /// <summary>Gets or sets the confirm password.</summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>DTO for auth response.</summary>
public class AuthResponse
{
    /// <summary>Gets or sets the access token.</summary>
    public string AccessToken { get; set; } = string.Empty;
    /// <summary>Gets or sets the refresh token.</summary>
    public string RefreshToken { get; set; } = string.Empty;
    /// <summary>Gets or sets the token expiry.</summary>
    public DateTime ExpiresAt { get; set; }
    /// <summary>Gets or sets the user info.</summary>
    public UserDto User { get; set; } = null!;
}

/// <summary>DTO for user info.</summary>
public class UserDto
{
    /// <summary>Gets or sets the user ID.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the email.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the company.</summary>
    public string? Company { get; set; }
    /// <summary>Gets or sets the phone number.</summary>
    public string? Phone { get; set; }
    /// <summary>Gets or sets the avatar.</summary>
    public string? Avatar { get; set; }
    /// <summary>Gets or sets the primary role.</summary>
    public string Role { get; set; } = string.Empty;
    /// <summary>Gets or sets the subscription plan.</summary>
    public string Plan { get; set; } = "Starter";
    /// <summary>Gets or sets the roles.</summary>
    public List<string> Roles { get; set; } = new();
}

/// <summary>DTO for refresh token request.</summary>
public class RefreshTokenRequest
{
    /// <summary>Gets or sets the refresh token.</summary>
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>DTO for revoke token request.</summary>
public class RevokeTokenRequest
{
    /// <summary>Gets or sets the token to revoke.</summary>
    public string Token { get; set; } = string.Empty;
}

/// <summary>DTO for forgot password request.</summary>
public class ForgotPasswordRequest
{
    /// <summary>Gets or sets the email.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>DTO for reset password request.</summary>
public class ResetPasswordRequest
{
    /// <summary>Gets or sets the email.</summary>
    public string Email { get; set; } = string.Empty;
    /// <summary>Gets or sets the reset token.</summary>
    public string Token { get; set; } = string.Empty;
    /// <summary>Gets or sets the new password.</summary>
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>DTO for updating user profile.</summary>
public class UpdateProfileRequest
{
    /// <summary>Gets or sets the first name.</summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>Gets or sets the last name.</summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>Gets or sets the company.</summary>
    public string? Company { get; set; }
    /// <summary>Gets or sets the phone.</summary>
    public string? Phone { get; set; }
    /// <summary>Gets or sets the phone number (alias).</summary>
    public string? PhoneNumber { get; set; }
    /// <summary>Gets or sets the date of birth.</summary>
    public DateTime? DateOfBirth { get; set; }
}
