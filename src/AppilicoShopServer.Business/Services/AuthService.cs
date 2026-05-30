using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using AppilicoShopServer.Business.DTOs.Auth;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Constants;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Auth service implementation.</summary>
public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService _emailService;

    /// <summary>Initializes a new instance of AuthService.</summary>
    public AuthService(
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IEmailService emailService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
        _logger = logger;
        _emailService = emailService;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return ApiResponse<AuthResponse>.FailResponse("A user with this email already exists");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new AppUser
            {
                UserName = request.Email.Trim(),
                Email = request.Email.Trim(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(request.Phone ?? request.PhoneNumber) ? null : (request.Phone ?? request.PhoneNumber)!.Trim()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<AuthResponse>.FailResponse("Registration failed", result.Errors.Select(e => e.Description).ToList());
            }

            var roleResult = await _userManager.AddToRoleAsync(user, AppConstants.Roles.Customer);
            if (!roleResult.Succeeded)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<AuthResponse>.FailResponse("Registration failed", roleResult.Errors.Select(e => e.Description).ToList());
            }

            var customer = new Customer
            {
                UserId = user.Id,
                CustomerCode = $"CUST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
                JoinDate = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            var authResponse = await GenerateAuthResponseAsync(user);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("User {UserId} registered successfully", user.Id);
            return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Registration successful");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Registration failed for email {Email}", request.Email);
            return ApiResponse<AuthResponse>.FailResponse("Registration failed. Please try again.");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return ApiResponse<AuthResponse>.FailResponse("Invalid email or password");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.FailResponse("Account is deactivated");

        try
        {
            var authResponse = await GenerateAuthResponseAsync(user);
            _logger.LogInformation("User {UserId} logged in", user.Id);
            return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed while generating tokens for user {UserId}", user.Id);
            return ApiResponse<AuthResponse>.FailResponse("Login failed. Please try again.");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await FindRefreshTokenAsync(request.RefreshToken);
        if (storedToken == null || storedToken.RevokedAt != null || storedToken.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponse>.FailResponse("Invalid or expired refresh token");

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null)
            return ApiResponse<AuthResponse>.FailResponse("User not found");

        try
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            var authResponse = await GenerateAuthResponseAsync(user);
            return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token flow failed for user {UserId}", storedToken.UserId);
            return ApiResponse<AuthResponse>.FailResponse("Token refresh failed. Please log in again.");
        }
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> RevokeTokenAsync(string token, string userId)
    {
        var storedToken = await FindRefreshTokenAsync(token);
        if (storedToken == null)
            return ApiResponse<bool>.FailResponse("Token not found");

        storedToken.RevokedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token revoked for user {UserId}", userId);
        return ApiResponse<bool>.SuccessResponse(true, "Token revoked successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<UserDto>> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<UserDto>.FailResponse("User not found");

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            Company = user.Company,
            Phone = user.PhoneNumber,
            Avatar = user.Avatar,
            Role = roles.FirstOrDefault() ?? string.Empty,
            Plan = user.SubscriptionPlan,
            Roles = roles
        };

        return ApiResponse<UserDto>.SuccessResponse(userDto);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResponse<UserDto>.FailResponse("User not found");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        if (request.Company != null) user.Company = request.Company;
        var phone = request.Phone ?? request.PhoneNumber;
        if (phone != null) user.PhoneNumber = phone;
        if (request.DateOfBirth.HasValue) user.DateOfBirth = request.DateOfBirth;

        await _userManager.UpdateAsync(user);

        var roles2 = (await _userManager.GetRolesAsync(user)).ToList();
        var userDto2 = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            Company = user.Company,
            Phone = user.PhoneNumber,
            Avatar = user.Avatar,
            Role = roles2.FirstOrDefault() ?? string.Empty,
            Plan = user.SubscriptionPlan,
            Roles = roles2
        };

        _logger.LogInformation("Profile updated for user {UserId}", userId);
        return ApiResponse<UserDto>.SuccessResponse(userDto2, "Profile updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a reset link has been sent");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(request.Email, token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
        }

        return ApiResponse<bool>.SuccessResponse(true, "If the email exists, a reset link has been sent");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return ApiResponse<bool>.FailResponse("Invalid request");

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return ApiResponse<bool>.FailResponse("Password reset failed", result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("Password reset for user {UserId}", user.Id);
        return ApiResponse<bool>.SuccessResponse(true, "Password reset successfully");
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtSecret = _configuration["JWT:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret == "will-be-overridden-by-user-secrets")
            throw new InvalidOperationException("JWT signing secret is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = int.Parse(_configuration["JWT:ExpirationInMinutes"] ?? "60");
        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshTokenDays = int.Parse(_configuration["JWT:RefreshTokenExpirationInDays"] ?? "7");

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expires,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                Company = user.Company,
                Phone = user.PhoneNumber,
                Avatar = user.Avatar,
                Role = roles.FirstOrDefault() ?? string.Empty,
                Plan = user.SubscriptionPlan,
                Roles = roles.ToList()
            }
        };
    }

    private async Task<RefreshToken?> FindRefreshTokenAsync(string token)
    {
        return await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
    }
}
