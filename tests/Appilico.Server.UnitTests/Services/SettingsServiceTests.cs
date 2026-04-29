using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Settings;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

namespace Appilico.Server.UnitTests.Services;

public class SettingsServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAppSettingRepository> _settingsRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<SettingsService>> _loggerMock;
    private readonly SettingsService _sut;

    public SettingsServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _settingsRepoMock = new Mock<IAppSettingRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<SettingsService>>();

        _unitOfWorkMock.Setup(u => u.Settings).Returns(_settingsRepoMock.Object);

        _sut = new SettingsService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSettings()
    {
        var settings = new List<AppSetting>
        {
            new() { Id = Guid.NewGuid(), Key = "SiteName", Value = "Appilico", Group = "General", CreatedBy = "admin" },
            new() { Id = Guid.NewGuid(), Key = "Currency", Value = "USD", Group = "General", CreatedBy = "admin" }
        };
        _settingsRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(settings);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByKeyAsync_ExistingKey_ReturnsSuccess()
    {
        var setting = new AppSetting { Id = Guid.NewGuid(), Key = "SiteName", Value = "Appilico", Group = "General", CreatedBy = "admin" };
        _settingsRepoMock.Setup(r => r.GetByKeyAsync("SiteName")).ReturnsAsync(setting);

        var result = await _sut.GetByKeyAsync("SiteName");

        result.Success.Should().BeTrue();
        result.Data!.Value.Should().Be("Appilico");
    }

    [Fact]
    public async Task GetByKeyAsync_NonExistingKey_ReturnsFail()
    {
        _settingsRepoMock.Setup(r => r.GetByKeyAsync("NonExistent")).ReturnsAsync((AppSetting?)null);

        var result = await _sut.GetByKeyAsync("NonExistent");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetByGroupAsync_ExistingGroup_ReturnsSettings()
    {
        var settings = new List<AppSetting>
        {
            new() { Id = Guid.NewGuid(), Key = "SiteName", Value = "Appilico", Group = "General", CreatedBy = "admin" },
            new() { Id = Guid.NewGuid(), Key = "Currency", Value = "USD", Group = "General", CreatedBy = "admin" }
        };
        _settingsRepoMock.Setup(r => r.GetByGroupAsync("General")).ReturnsAsync(settings);

        var result = await _sut.GetByGroupAsync("General");

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByGroupAsync_EmptyGroup_ReturnsEmptyList()
    {
        _settingsRepoMock.Setup(r => r.GetByGroupAsync("Empty")).ReturnsAsync(new List<AppSetting>());

        var result = await _sut.GetByGroupAsync("Empty");

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_ExistingSettings_ReturnsSuccess()
    {
        var setting = new AppSetting { Id = Guid.NewGuid(), Key = "SiteName", Value = "Appilico", Group = "General", CreatedBy = "admin" };
        _settingsRepoMock.Setup(r => r.GetByKeyAsync("SiteName")).ReturnsAsync(setting);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateSettingsRequest
        {
            Settings = new List<SettingItem> { new() { Key = "SiteName", Value = "New Appilico" } }
        };
        var result = await _sut.UpdateAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingKey_SkipsIt()
    {
        _settingsRepoMock.Setup(r => r.GetByKeyAsync("NonExistent")).ReturnsAsync((AppSetting?)null);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateSettingsRequest
        {
            Settings = new List<SettingItem> { new() { Key = "NonExistent", Value = "SomeValue" } }
        };
        var result = await _sut.UpdateAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }
}
