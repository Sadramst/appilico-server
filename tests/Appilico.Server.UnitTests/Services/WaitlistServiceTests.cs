using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Waitlist;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Services;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.UnitTests.Services;

public class WaitlistServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<IEmailService> _emailMock;
    private readonly WaitlistService _sut;

    public WaitlistServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _emailMock = new Mock<IEmailService>();
                _emailMock.Setup(e => e.SendWaitlistConfirmationAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        var logger = new Mock<ILogger<WaitlistService>>().Object;
        _sut = new WaitlistService(_db, logger, _emailMock.Object);
    }

    public void Dispose() => _db.Dispose();

    // ─── SubscribeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SubscribeAsync_NewEmail_CreatesEntry()
    {
        var result = await _sut.SubscribeAsync(
            new WaitlistSubscribeRequest { Email = "alice@test.com", Company = "Mines Co" },
            "1.2.3.4");

        result.Success.Should().BeTrue();
        var entry = await _db.WaitlistEntries.FirstOrDefaultAsync(e => e.Email == "alice@test.com");
        entry.Should().NotBeNull();
        entry!.IPAddress.Should().Be("1.2.3.4");
    }

    [Fact]
    public async Task SubscribeAsync_DuplicateEmail_ReturnsSuccessIdempotently()
    {
        _db.WaitlistEntries.Add(new WaitlistEntry { Email = "alice@test.com" });
        await _db.SaveChangesAsync();

        var result = await _sut.SubscribeAsync(
            new WaitlistSubscribeRequest { Email = "alice@test.com" },
            null);

        result.Success.Should().BeTrue();
        var count = await _db.WaitlistEntries.CountAsync(e => e.Email == "alice@test.com");
        count.Should().Be(1); // No duplicate
    }

    [Fact]
    public async Task SubscribeAsync_SendsConfirmationEmail()
    {
        await _sut.SubscribeAsync(
            new WaitlistSubscribeRequest { Email = "bob@test.com" },
            null);

        _emailMock.Verify(e => e.SendWaitlistConfirmationAsync("bob@test.com", It.IsAny<int>()), Times.Once);
    }

    // ─── GetCountAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCountAsync_ReturnsCorrectCount()
    {
        _db.WaitlistEntries.AddRange(
            new WaitlistEntry { Email = "a@t.com" },
            new WaitlistEntry { Email = "b@t.com" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetCountAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().Be(2);
    }

    // ─── GetAdminListAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetAdminListAsync_ReturnsPagedEntries()
    {
        for (var i = 1; i <= 5; i++)
            _db.WaitlistEntries.Add(new WaitlistEntry { Email = $"u{i}@t.com" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAdminListAsync(1, 3, null);

        result.Success.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAdminListAsync_FiltersByIsNotified()
    {
        _db.WaitlistEntries.AddRange(
            new WaitlistEntry { Email = "n@t.com", IsNotified = true },
            new WaitlistEntry { Email = "nn@t.com", IsNotified = false });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAdminListAsync(1, 10, isNotified: true);

        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Email.Should().Be("n@t.com");
    }

    // ─── NotifyAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task NotifyAsync_ExistingEntry_SetsIsNotified()
    {
        var entry = new WaitlistEntry { Email = "carol@test.com" };
        _db.WaitlistEntries.Add(entry);
        await _db.SaveChangesAsync();

        var result = await _sut.NotifyAsync(entry.Id);

        result.Success.Should().BeTrue();
        var updated = await _db.WaitlistEntries.FindAsync(entry.Id);
        updated!.IsNotified.Should().BeTrue();
        updated.NotifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task NotifyAsync_NonExistentId_ReturnsFailure()
    {
        var result = await _sut.NotifyAsync(Guid.NewGuid());
        result.Success.Should().BeFalse();
    }
}
