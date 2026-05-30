using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Discount;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;

namespace AppilicoShopServer.UnitTests.Services;

public class DiscountServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDiscountRepository> _discountRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<DiscountService>> _loggerMock;
    private readonly DiscountService _sut;

    public DiscountServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _discountRepoMock = new Mock<IDiscountRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<DiscountService>>();

        _unitOfWorkMock.Setup(u => u.Discounts).Returns(_discountRepoMock.Object);

        _sut = new DiscountService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDiscount_ReturnsSuccess()
    {
        var id = Guid.NewGuid();
        var discount = new Discount
        {
            Id = id, Name = "Summer Sale", Code = "SUMMER10",
            DiscountType = DiscountType.Percentage, Value = 10,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true, CreatedBy = "test"
        };
        _discountRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(discount);

        var result = await _sut.GetByIdAsync(id);

        result.Success.Should().BeTrue();
        result.Data!.Name.Should().Be("Summer Sale");
    }

    [Fact]
    public async Task ValidateAsync_ValidCode_ReturnsValid()
    {
        var discount = new Discount
        {
            Id = Guid.NewGuid(), Name = "Test", Code = "TEST10",
            DiscountType = DiscountType.Percentage, Value = 10,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true, UsageLimit = 100, UsedCount = 0, CreatedBy = "test"
        };
        _discountRepoMock.Setup(r => r.GetByCodeAsync("TEST10")).ReturnsAsync(discount);

        var request = new ValidateDiscountRequest { Code = "TEST10", OrderAmount = 100 };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidCode_ReturnsClearMessage()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("BADCODE")).ReturnsAsync((Discount?)null);

        var result = await _sut.ValidateAsync(new ValidateDiscountRequest { Code = "BADCODE", OrderAmount = 100m });

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeFalse();
        result.Data.Message.Should().Be("Invalid discount code");
    }

    [Fact]
    public async Task ValidateAsync_InactiveDiscount_ReturnsClearMessage()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("OFF10")).ReturnsAsync(new Discount
        {
            Id = Guid.NewGuid(),
            Code = "OFF10",
            Name = "Off",
            DiscountType = DiscountType.Fixed,
            Value = 10m,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = false,
            CreatedBy = "test"
        });

        var result = await _sut.ValidateAsync(new ValidateDiscountRequest { Code = "OFF10", OrderAmount = 100m });

        result.Data!.Message.Should().Be("Discount code is inactive");
    }

    [Fact]
    public async Task ValidateAsync_ExpiredDiscount_ReturnsClearMessage()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("OLD10")).ReturnsAsync(new Discount
        {
            Id = Guid.NewGuid(),
            Code = "OLD10",
            Name = "Old",
            DiscountType = DiscountType.Fixed,
            Value = 10m,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            CreatedBy = "test"
        });

        var result = await _sut.ValidateAsync(new ValidateDiscountRequest { Code = "OLD10", OrderAmount = 100m });

        result.Data!.Message.Should().Be("Discount code has expired");
    }

    [Fact]
    public async Task ValidateAsync_MinimumOrderNotMet_ReturnsClearMessage()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("MIN50")).ReturnsAsync(new Discount
        {
            Id = Guid.NewGuid(),
            Code = "MIN50",
            Name = "Min order",
            DiscountType = DiscountType.Fixed,
            Value = 10m,
            MinOrderAmount = 50m,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true,
            CreatedBy = "test"
        });

        var result = await _sut.ValidateAsync(new ValidateDiscountRequest { Code = "MIN50", OrderAmount = 20m });

        result.Data!.Message.Should().Contain("Minimum order amount not met");
    }

    [Fact]
    public async Task ValidateAsync_MaxRedemptionsReached_ReturnsClearMessage()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("MAXED")).ReturnsAsync(new Discount
        {
            Id = Guid.NewGuid(),
            Code = "MAXED",
            Name = "Maxed",
            DiscountType = DiscountType.Fixed,
            Value = 10m,
            UsageLimit = 5,
            UsedCount = 5,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true,
            CreatedBy = "test"
        });

        var result = await _sut.ValidateAsync(new ValidateDiscountRequest { Code = "MAXED", OrderAmount = 100m });

        result.Data!.Message.Should().Be("Discount code has reached its maximum number of redemptions");
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateDiscountRequest
        {
            Name = "New Discount", Code = "NEW10",
            DiscountType = DiscountType.Percentage, Value = 10,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
        };
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CreateAsync(request, "user1");

        result.Success.Should().BeTrue();
    }

    // ──────── GetAllAsync ────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllDiscounts()
    {
        var discounts = new List<Discount>
        {
            new() { Id = Guid.NewGuid(), Code = "D1", Name = "Disc1", DiscountType = DiscountType.Fixed, Value = 10m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30), IsActive = true, CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), Code = "D2", Name = "Disc2", DiscountType = DiscountType.Percentage, Value = 5m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30), IsActive = true, CreatedBy = "test" }
        };
        _discountRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(discounts);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    // ──────── GetActiveAsync ────────

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveDiscounts()
    {
        var discounts = new List<Discount>
        {
            new() { Id = Guid.NewGuid(), Code = "ACTIVE", Name = "Active", DiscountType = DiscountType.Fixed, Value = 10m, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(30), IsActive = true, CreatedBy = "test" }
        };
        _discountRepoMock.Setup(r => r.GetActiveDiscountsAsync()).ReturnsAsync(discounts);

        var result = await _sut.GetActiveAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    // ──────── UpdateAsync ────────

    [Fact]
    public async Task UpdateAsync_ExistingDiscount_ReturnsSuccess()
    {
        var discountId = Guid.NewGuid();
        var discount = new Discount
        {
            Id = discountId, Code = "UPD", Name = "Old", DiscountType = DiscountType.Fixed,
            Value = 5m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true, CreatedBy = "test"
        };
        _discountRepoMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateDiscountRequest { Name = "Updated", Value = 15m, IsActive = true, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(60) };
        var result = await _sut.UpdateAsync(discountId, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingDiscount_ReturnsFail()
    {
        _discountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Discount?)null);

        var request = new UpdateDiscountRequest { Name = "Updated", Value = 10m };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    // ──────── DeleteAsync ────────

    [Fact]
    public async Task DeleteAsync_ExistingDiscount_ReturnsSuccess()
    {
        var discountId = Guid.NewGuid();
        var discount = new Discount { Id = discountId, Code = "DEL", Name = "Delete Me", DiscountType = DiscountType.Fixed, Value = 5m, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30), CreatedBy = "test" };
        _discountRepoMock.Setup(r => r.GetByIdAsync(discountId)).ReturnsAsync(discount);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(discountId, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingDiscount_ReturnsFail()
    {
        _discountRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Discount?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "admin1");

        result.Success.Should().BeFalse();
    }

    // ──────── CreateAsync Duplicate ────────

    [Fact]
    public async Task CreateAsync_DuplicateCode_ReturnsFail()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("EXISTING")).ReturnsAsync(new Discount
        {
            Id = Guid.NewGuid(), Code = "EXISTING", Name = "Existing",
            DiscountType = DiscountType.Fixed, Value = 5m,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30),
            IsActive = true, CreatedBy = "test"
        });

        var request = new CreateDiscountRequest
        {
            Name = "Dup", Code = "EXISTING",
            DiscountType = DiscountType.Percentage, Value = 10,
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30)
        };
        var result = await _sut.CreateAsync(request, "user1");

        result.Success.Should().BeFalse();
    }

    // ──────── Validate percentage with max cap ────────

    [Fact]
    public async Task ValidateAsync_PercentageDiscount_CalculatesCorrectAmount()
    {
        _discountRepoMock.Setup(r => r.GetByCodeAsync("PCT20")).ReturnsAsync(new Discount
        {
            Id = Guid.NewGuid(), Code = "PCT20", Name = "20% Off",
            DiscountType = DiscountType.Percentage, Value = 20m,
            MaxDiscountAmount = 50m,
            StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true, CreatedBy = "test"
        });

        var result = await _sut.ValidateAsync(new ValidateDiscountRequest { Code = "PCT20", OrderAmount = 500m });

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeTrue();
        result.Data.DiscountAmount.Should().Be(50m); // 20% of 500 = 100, but capped at 50
    }
}
