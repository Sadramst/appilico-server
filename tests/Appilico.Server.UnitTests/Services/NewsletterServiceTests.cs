using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Newsletter;
using Appilico.Server.Business.Services;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.UnitTests.Services;

public class NewsletterServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly NewsletterService _sut;

    public NewsletterServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        var logger = new Mock<ILogger<NewsletterService>>().Object;
        _sut = new NewsletterService(_db, logger);
    }

    public void Dispose() => _db.Dispose();

    // ─── SubscribeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SubscribeAsync_NewEmail_CreatesSubscriber()
    {
        var result = await _sut.SubscribeAsync(new NewsletterSubscribeRequest { Email = "new@test.com" });

        result.Success.Should().BeTrue();
        var subscriber = await _db.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Email == "new@test.com");
        subscriber.Should().NotBeNull();
        subscriber!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task SubscribeAsync_AlreadyActiveEmail_ReturnsSuccessWithoutDuplicate()
    {
        _db.NewsletterSubscribers.Add(new NewsletterSubscriber { Email = "existing@test.com", IsActive = true, SubscribedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.SubscribeAsync(new NewsletterSubscribeRequest { Email = "existing@test.com" });

        result.Success.Should().BeTrue();
        var count = await _db.NewsletterSubscribers.CountAsync(s => s.Email == "existing@test.com");
        count.Should().Be(1); // No duplicate
    }

    [Fact]
    public async Task SubscribeAsync_InactiveEmail_ReactivatesSubscriber()
    {
        _db.NewsletterSubscribers.Add(new NewsletterSubscriber
        {
            Email = "inactive@test.com",
            IsActive = false,
            SubscribedAt = DateTime.UtcNow.AddDays(-30),
            UnsubscribedAt = DateTime.UtcNow.AddDays(-10)
        });
        await _db.SaveChangesAsync();

        var result = await _sut.SubscribeAsync(new NewsletterSubscribeRequest { Email = "inactive@test.com" });

        result.Success.Should().BeTrue();
        var subscriber = await _db.NewsletterSubscribers.FirstAsync(s => s.Email == "inactive@test.com");
        subscriber.IsActive.Should().BeTrue();
        subscriber.UnsubscribedAt.Should().BeNull();
    }

    [Fact]
    public async Task SubscribeAsync_NormalisesEmailToLowercase()
    {
        await _sut.SubscribeAsync(new NewsletterSubscribeRequest { Email = "UPPER@TEST.COM" });

        var exists = await _db.NewsletterSubscribers.AnyAsync(s => s.Email == "upper@test.com");
        exists.Should().BeTrue();
    }

    // ─── UnsubscribeAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UnsubscribeAsync_ActiveSubscriber_DeactivatesRecord()
    {
        _db.NewsletterSubscribers.Add(new NewsletterSubscriber { Email = "active@test.com", IsActive = true, SubscribedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _sut.UnsubscribeAsync(new NewsletterUnsubscribeRequest { Email = "active@test.com" });

        result.Success.Should().BeTrue();
        var subscriber = await _db.NewsletterSubscribers.FirstAsync(s => s.Email == "active@test.com");
        subscriber.IsActive.Should().BeFalse();
        subscriber.UnsubscribedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UnsubscribeAsync_NonExistentEmail_ReturnsSuccessIdempotently()
    {
        var result = await _sut.UnsubscribeAsync(new NewsletterUnsubscribeRequest { Email = "nobody@test.com" });

        result.Success.Should().BeTrue(); // Idempotent
    }
}
