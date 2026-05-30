using FluentAssertions;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using AppilicoShopServer.Business.DTOs.Subscription;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.DataAccess.Repositories;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.UnitTests.Services;

public class SubscriptionServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _stripeServiceMock = new Mock<IStripeService>();

        var logger = new Mock<ILogger<SubscriptionService>>().Object;
        _sut = new SubscriptionService(
            _userManagerMock.Object,
            new UnitOfWork(_db),
            _stripeServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StripeOptions { Enabled = false }),
            logger);
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
    public async Task UpgradeAsync_FreeTier_AllowsDowngradeAndCreatesSubscription()
    {
        var user = CreateUser("user-upgrade", SubscriptionTier.Starter);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpgradeAsync("user-upgrade", new UpgradeSubscriptionRequest { Plan = "Free" });

        result.Success.Should().BeTrue();
        result.Data!.Tier.Should().Be("Free");

        var subscription = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == "user-upgrade");
        subscription.Should().NotBeNull();
        subscription!.Tier.Should().Be(SubscriptionTier.Free);
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task UpgradeAsync_FreeTier_CreatesHistoryRecord()
    {
        var user = CreateUser("user-history", SubscriptionTier.Professional);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

        await _sut.UpgradeAsync("user-history", new UpgradeSubscriptionRequest { Plan = "Free" });

        var history = await _db.SubscriptionHistories.FirstOrDefaultAsync(h => h.UserId == "user-history");
        history.Should().NotBeNull();
        history!.ToTier.Should().Be(SubscriptionTier.Free);
        history.FromTier.Should().Be(SubscriptionTier.Professional);
    }

    [Fact]
    public async Task UpgradeAsync_PaidTier_ReturnsFailureWithoutChangingUser()
    {
        var user = CreateUser("user-paid", SubscriptionTier.Free);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var result = await _sut.UpgradeAsync("user-paid", new UpgradeSubscriptionRequest { Plan = "Starter" });

        result.Success.Should().BeFalse();
        user.SubscriptionTier.Should().Be(SubscriptionTier.Free);
        (await _db.Subscriptions.AnyAsync(s => s.UserId == "user-paid")).Should().BeFalse();
    }

    [Fact]
    public async Task UpgradeAsync_PaidTier_ReturnsClientSecretWithoutGrantingTierUntilProviderActive()
    {
        var user = CreateUser("user-stripe", SubscriptionTier.Free);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var sut = new SubscriptionService(
            _userManagerMock.Object,
            new UnitOfWork(_db),
            _stripeServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StripeOptions
            {
                Enabled = true,
                SecretKey = "sk_test",
                PublishableKey = "pk_test",
                WebhookSecret = "whsec_test",
                StarterPriceId = "price_starter",
                ProfessionalPriceId = "price_professional",
                EnterprisePriceId = "price_enterprise"
            }),
            new Mock<ILogger<SubscriptionService>>().Object);

        _stripeServiceMock
            .Setup(s => s.CreateSubscriptionAsync(It.Is<StripeSubscriptionRequest>(request => request.PriceId == "price_starter"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StripeSubscriptionResult("cus_123", "sub_123", "price_starter", "incomplete", "client_secret", null));

        var result = await sut.UpgradeAsync("user-stripe", new UpgradeSubscriptionRequest { Plan = "Starter" });

        result.Success.Should().BeTrue();
        result.Data!.RequiresPayment.Should().BeTrue();
        result.Data.PaymentClientSecret.Should().Be("client_secret");
        result.Data.PendingTier.Should().Be("Starter");
        user.SubscriptionTier.Should().Be(SubscriptionTier.Free);

        var subscription = await _db.Subscriptions.SingleAsync(s => s.UserId == "user-stripe");
        subscription.Status.Should().Be(SubscriptionStatus.Incomplete);
        subscription.StripeSubscriptionId.Should().Be("sub_123");
    }

    [Fact]
    public async Task HandleStripeWebhookAsync_DuplicateEvent_IsIdempotent()
    {
        _db.ExternalWebhookEvents.Add(new ExternalWebhookEvent
        {
            Provider = "stripe",
            EventId = "evt_duplicate",
            EventType = "payment_intent.succeeded",
            PayloadHash = "hash",
            ProcessedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        using var json = JsonDocument.Parse("{}");
        _stripeServiceMock
            .Setup(s => s.ConstructWebhookEvent("{}", "sig"))
            .Returns(new StripeWebhookEvent("evt_duplicate", "payment_intent.succeeded", "hash", json.RootElement.Clone()));

        var result = await _sut.HandleStripeWebhookAsync("{}", "sig");

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Webhook already processed");
        var count = await _db.ExternalWebhookEvents.CountAsync(e => e.EventId == "evt_duplicate");
        count.Should().Be(1);
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
