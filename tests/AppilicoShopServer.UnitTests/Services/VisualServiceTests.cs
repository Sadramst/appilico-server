using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.DataAccess.Repositories;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AppilicoShopServer.UnitTests.Services;

public class VisualServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<IFileStorageService> _storageMock;
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly VisualService _sut;

    public VisualServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        _storageMock = new Mock<IFileStorageService>();
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new VisualService(
            new UnitOfWork(_db),
            _userManagerMock.Object,
            _storageMock.Object,
            new Mock<ILogger<VisualService>>().Object);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task DownloadAsync_WhenStorageUnavailable_DoesNotIncrementOrLogDownload()
    {
        var visual = await SeedVisualAsync(downloadCount: 7);
        var user = new AppUser { Id = "user-1", SubscriptionTier = SubscriptionTier.Enterprise };
        _userManagerMock.Setup(manager => manager.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _storageMock
            .Setup(storage => storage.GetPresignedUrlAsync(It.IsAny<string>(), 30))
            .ThrowsAsync(new NotSupportedException("storage disabled"));

        var result = await _sut.DownloadAsync(visual.Id, user.Id, "127.0.0.1");

        result.Success.Should().BeFalse();
        (await _db.VisualDownloads.CountAsync()).Should().Be(0);
        (await _db.Visuals.SingleAsync(item => item.Id == visual.Id)).DownloadCount.Should().Be(7);
    }

    [Fact]
    public async Task DownloadAsync_WhenStorageSucceeds_IncrementsAndLogsDownload()
    {
        var visual = await SeedVisualAsync(downloadCount: 2);
        var user = new AppUser { Id = "user-1", SubscriptionTier = SubscriptionTier.Enterprise };
        _userManagerMock.Setup(manager => manager.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _storageMock
            .Setup(storage => storage.GetPresignedUrlAsync(It.IsAny<string>(), 30))
            .ReturnsAsync("https://storage.test/download.pbiviz");

        var result = await _sut.DownloadAsync(visual.Id, user.Id, "127.0.0.1");

        result.Success.Should().BeTrue();
        result.Data!.DownloadUrl.Should().Be("https://storage.test/download.pbiviz");
        (await _db.VisualDownloads.CountAsync()).Should().Be(1);
        (await _db.Visuals.SingleAsync(item => item.Id == visual.Id)).DownloadCount.Should().Be(3);
    }

    private async Task<Visual> SeedVisualAsync(int downloadCount)
    {
        var visual = new Visual
        {
            Name = "Mine KPI",
            Slug = "mine-kpi",
            Description = "Visual description",
            Category = VisualCategory.Production,
            Type = "KPI",
            RequiredPlan = SubscriptionTier.Free,
            DownloadCount = downloadCount,
            DemoUrl = "visuals/mine-kpi.pbiviz",
            IsActive = true
        };

        _db.Visuals.Add(visual);
        await _db.SaveChangesAsync();
        return visual;
    }
}