using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Appilico.Server.Business.DTOs.Auth;
using Appilico.Server.Business.Services;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AppDbContext _dbContext;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<AuthService>>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(() => _dbContext.SaveChangesAsync());

        var configValues = new Dictionary<string, string?>
        {
            { "JWT:Secret", "SuperSecretKeyForTestingPurposes_AtLeast32Chars!!" },
            { "JWT:Issuer", "TestIssuer" },
            { "JWT:Audience", "TestAudience" },
            { "JWT:ExpirationInMinutes", "60" },
            { "JWT:RefreshTokenExpirationInDays", "7" }
        };
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        _emailServiceMock = new Mock<IEmailService>();
        _sut = new AuthService(_userManagerMock.Object, _unitOfWorkMock.Object, _mapper, _configuration, _loggerMock.Object, _dbContext, _emailServiceMock.Object);
    }

    // ──────── RegisterAsync ────────

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("newuser@test.com")).ReturnsAsync((AppUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), "Password123!"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(new List<string> { "Customer" });
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "newuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            PhoneNumber = "09123456789"
        };

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.User.Email.Should().Be("newuser@test.com");
        result.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFail()
    {
        var existingUser = new AppUser { Email = "existing@test.com", FirstName = "Existing", LastName = "User" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task RegisterAsync_IdentityFails_ReturnsFail()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("new@test.com")).ReturnsAsync((AppUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "new@test.com",
            Password = "weak",
            ConfirmPassword = "weak"
        };

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_RoleAssignmentFails_ReturnsFail()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("newuser@test.com")).ReturnsAsync((AppUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Customer role does not exist" }));

        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "newuser@test.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await _sut.RegisterAsync(request);

        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Customer role"));
    }

    // ──────── LoginAsync ────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });
        var request = new LoginRequest { Email = "john@test.com", Password = "Password123!" };

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsFail()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("wrong@test.com")).ReturnsAsync((AppUser?)null);

        var request = new LoginRequest { Email = "wrong@test.com", Password = "Password123!" };

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFail()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPassword")).ReturnsAsync(false);

        var request = new LoginRequest { Email = "john@test.com", Password = "WrongPassword" };

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsFail()
    {
        var user = CreateTestUser();
        user.IsActive = false;
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "Password123!")).ReturnsAsync(true);

        var request = new LoginRequest { Email = "john@test.com", Password = "Password123!" };

        var result = await _sut.LoginAsync(request);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("deactivated");
    }

    // ──────── RefreshTokenAsync ────────

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ReturnsFail()
    {
        var request = new RefreshTokenRequest { RefreshToken = "some-token" };

        var result = await _sut.RefreshTokenAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsSuccess()
    {
        var user = CreateTestUser();
        var storedToken = new RefreshToken
        {
            Token = "refresh-token",
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        _dbContext.RefreshTokens.Add(storedToken);
        await _dbContext.SaveChangesAsync();

        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

        var request = new RefreshTokenRequest { RefreshToken = "refresh-token" };

        var result = await _sut.RefreshTokenAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    // ──────── RevokeTokenAsync ────────

    [Fact]
    public async Task RevokeTokenAsync_UnknownToken_ReturnsFail()
    {
        var result = await _sut.RevokeTokenAsync("some-token", "user-id");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_ExistingToken_ReturnsSuccess()
    {
        var token = new RefreshToken
        {
            Token = "revoke-me",
            UserId = "user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "user-id"
        };
        _dbContext.RefreshTokens.Add(token);
        await _dbContext.SaveChangesAsync();

        var result = await _sut.RevokeTokenAsync("revoke-me", "user-id");

        result.Success.Should().BeTrue();
        _dbContext.RefreshTokens.Single(rt => rt.Token == "revoke-me").RevokedAt.Should().NotBeNull();
    }

    // ──────── GetProfileAsync ────────

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

        var result = await _sut.GetProfileAsync(user.Id);

        result.Success.Should().BeTrue();
        result.Data!.FirstName.Should().Be("John");
        result.Data.Email.Should().Be("john@test.com");
    }

    [Fact]
    public async Task GetProfileAsync_NonExistingUser_ReturnsFail()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("nonexistent")).ReturnsAsync((AppUser?)null);

        var result = await _sut.GetProfileAsync("nonexistent");

        result.Success.Should().BeFalse();
    }

    // ──────── UpdateProfileAsync ────────

    [Fact]
    public async Task UpdateProfileAsync_ExistingUser_ReturnsSuccess()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Customer" });

        var request = new UpdateProfileRequest { FirstName = "Updated", LastName = "User", PhoneNumber = "555-9999" };

        var result = await _sut.UpdateProfileAsync(user.Id, request);

        result.Success.Should().BeTrue();
        result.Data!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateProfileAsync_NonExistingUser_ReturnsFail()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("nonexistent")).ReturnsAsync((AppUser?)null);

        var request = new UpdateProfileRequest { FirstName = "Updated", LastName = "User" };

        var result = await _sut.UpdateProfileAsync("nonexistent", request);

        result.Success.Should().BeFalse();
    }

    // ──────── ForgotPasswordAsync ────────

    [Fact]
    public async Task ForgotPasswordAsync_ExistingUser_ReturnsSuccess()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");

        var request = new ForgotPasswordRequest { Email = "john@test.com" };

        var result = await _sut.ForgotPasswordAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ForgotPasswordAsync_NonExistingUser_StillReturnsSuccess()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("nope@test.com")).ReturnsAsync((AppUser?)null);

        var request = new ForgotPasswordRequest { Email = "nope@test.com" };

        var result = await _sut.ForgotPasswordAsync(request);

        result.Success.Should().BeTrue(); // By design, doesn't reveal user existence
    }

    // ──────── ResetPasswordAsync ────────

    [Fact]
    public async Task ResetPasswordAsync_ValidRequest_ReturnsSuccess()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "valid-token", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Success);

        var request = new ResetPasswordRequest { Email = "john@test.com", Token = "valid-token", NewPassword = "NewPassword123!" };

        var result = await _sut.ResetPasswordAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_ReturnsFail()
    {
        var user = CreateTestUser();
        _userManagerMock.Setup(m => m.FindByEmailAsync("john@test.com")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "invalid-token", "NewPassword123!"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var request = new ResetPasswordRequest { Email = "john@test.com", Token = "invalid-token", NewPassword = "NewPassword123!" };

        var result = await _sut.ResetPasswordAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_NonExistingUser_ReturnsFail()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("nope@test.com")).ReturnsAsync((AppUser?)null);

        var request = new ResetPasswordRequest { Email = "nope@test.com", Token = "token", NewPassword = "NewPassword123!" };

        var result = await _sut.ResetPasswordAsync(request);

        result.Success.Should().BeFalse();
    }

    // ──────── Helpers ────────

    private static AppUser CreateTestUser()
    {
        return new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "john@test.com",
            Email = "john@test.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };
    }
}
