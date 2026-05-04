using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Subscription;
using Appilico.Server.Business.Services;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.UnitTests.Services;

public class SubscriptionServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var logger = new Mock<ILogger<SubscriptionService>>().Object;
        _sut = new SubscriptionService(_userManagerMock.Object, _db, logger);
    }

    public void Dispose() => _db.Dispose();

    private AppUser CreateUser(string userId = "user-1", SubscriptionTier tier = SubscriptionTier.Free)
    {
        var user = new AppUser
        {
            Id = userId,
            UserName = $"{userId}@test.com",
            Email = $"{userId}@test.com",
            SubscriptionTier = tier
        };
        _userManagerMock.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(user);
        return user;
    }

    // ─── GetPlans ────────────────────────────────────────────────────────────

    [Fact]
    public void GetPlans_ReturnsNonEmptyList()
    {
        var result = _sut.GetPlans();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    // ─── GetCurrentAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentAsync_UserWithNoSubscription_ReturnsFreePlan()
    {
        var user = CreateUser("user-free");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var result = await _sut.GetCurrentAsync("user-free");

        result.Success.Should().BeTrue();
        result.Data!.Tier.Should().Be("Free");
    }

    // ─── UpgradeAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpgradeAsync_ValidTier_UpdatesUserAndCreatesSubscription()
    {
        var user = CreateUser("user-upgrade");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpgradeAsync("user-upgrade", new UpgradeSubscriptionRequest { Plan = "Starter" });

        result.Success.Should().BeTrue();
        result.Data!.Tier.Should().Be("Starter");

        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == "user-upgrade");
        subscription.Should().NotBeNull();
        subscription!.Tier.Should().Be(SubscriptionTier.Starter);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task UpgradeAsync_CreatesHistoryRecord()
    {
        var user = CreateUser("user-history");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.UpgradeAsync("user-history", new UpgradeSubscriptionRequest { Plan = "Professional" });

        var history = await _db.SubscriptionHistories.FirstOrDefaultAsync(h => h.UserId == "user-history");
        history.Should().NotBeNull();
        history!.ToTier.Should().Be(SubscriptionTier.Professional);
        history.FromTier.Should().Be(SubscriptionTier.Free);
    }

    [Fact]
    public async Task UpgradeAsync_InvalidTier_ReturnsFailure()
    {
        var user = CreateUser("user-invalid");
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var result = await _sut.UpgradeAsync("user-invalid", new UpgradeSubscriptionRequest { Plan = "NonExistentTier" });

        result.Success.Should().BeFalse();
    }

    // ─── CancelAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_ActiveSubscription_SetsStatusCancelled()
    {
        var userId = "user-cancel";
        var user = CreateUser(userId, SubscriptionTier.Starter);
        _db.Users.Add(user);
        _db.Subscriptions.Add(new Subscription
        {
            UserId = userId,
            Tier = SubscriptionTier.Starter,
            Status = SubscriptionStatus.Active,
            StartedAt = DateTime.UtcNow.AddDays(-30)
        });
        await _db.SaveChangesAsync();

        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.CancelAsync(userId, new CancelSubscriptionRequest());

        result.Success.Should().BeTrue();
        var sub = await _db.Subscriptions.FirstAsync(s => s.UserId == userId);
        sub.Status.Should().Be(SubscriptionStatus.Cancelled);
        sub.CancelledAt.Should().NotBeNull();
    }
}
