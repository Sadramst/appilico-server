using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using AppilicoShopServer.Business.DTOs.Voucher;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;

namespace AppilicoShopServer.UnitTests.Services;

public class VoucherServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IVoucherRepository> _voucherRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<VoucherService>> _loggerMock;
    private readonly VoucherService _sut;

    public VoucherServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _voucherRepoMock = new Mock<IVoucherRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<VoucherService>>();

        _unitOfWorkMock.Setup(u => u.Vouchers).Returns(_voucherRepoMock.Object);

        _sut = new VoucherService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllVouchers()
    {
        var vouchers = new List<Voucher> { CreateTestVoucher("SAVE10"), CreateTestVoucher("SAVE20") };
        _voucherRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(vouchers);

        var result = await _sut.GetAllAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingVoucher_ReturnsSuccess()
    {
        var voucher = CreateTestVoucher("SAVE10");
        _voucherRepoMock.Setup(r => r.GetByIdAsync(voucher.Id)).ReturnsAsync(voucher);

        var result = await _sut.GetByIdAsync(voucher.Id);

        result.Success.Should().BeTrue();
        result.Data!.Code.Should().Be("SAVE10");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingVoucher_ReturnsFail()
    {
        _voucherRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Voucher?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsSuccess()
    {
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("NEWCODE")).ReturnsAsync((Voucher?)null);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateVoucherRequest
        {
            Code = "NEWCODE",
            Description = "New voucher",
            VoucherType = VoucherType.Gift,
            Value = 15,
            ValueType = VoucherValueType.Percentage,
            StartDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        var result = await _sut.CreateAsync(request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_ReturnsFail()
    {
        var existing = CreateTestVoucher("EXISTING");
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("EXISTING")).ReturnsAsync(existing);

        var request = new CreateVoucherRequest
        {
            Code = "EXISTING",
            Value = 10,
            VoucherType = VoucherType.Gift,
            ValueType = VoucherValueType.Fixed,
            StartDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(30)
        };

        var result = await _sut.CreateAsync(request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ExistingVoucher_ReturnsSuccess()
    {
        var voucher = CreateTestVoucher("SAVE10");
        _voucherRepoMock.Setup(r => r.GetByIdAsync(voucher.Id)).ReturnsAsync(voucher);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateVoucherRequest { Value = 25, IsActive = true, StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(60) };
        var result = await _sut.UpdateAsync(voucher.Id, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingVoucher_ReturnsFail()
    {
        _voucherRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Voucher?)null);

        var request = new UpdateVoucherRequest { Value = 25, IsActive = true, StartDate = DateTime.UtcNow, ExpiryDate = DateTime.UtcNow.AddDays(60) };
        var result = await _sut.UpdateAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ExistingVoucher_ReturnsSuccess()
    {
        var voucher = CreateTestVoucher("TODELETE");
        _voucherRepoMock.Setup(r => r.GetByIdAsync(voucher.Id)).ReturnsAsync(voucher);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(voucher.Id, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ValidActiveVoucher_ReturnsValid()
    {
        var voucher = CreateTestVoucher("VALID10");
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("VALID10")).ReturnsAsync(voucher);

        var request = new ValidateVoucherRequest { Code = "VALID10", OrderAmount = 100m };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ExpiredVoucher_ReturnsInvalid()
    {
        var voucher = CreateTestVoucher("EXPIRED",
            startDate: DateTime.UtcNow.AddDays(-60),
            expiryDate: DateTime.UtcNow.AddDays(-1));
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("EXPIRED")).ReturnsAsync(voucher);

        var request = new ValidateVoucherRequest { Code = "EXPIRED", OrderAmount = 100m };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_InactiveVoucher_ReturnsInvalid()
    {
        var voucher = CreateTestVoucher("INACTIVE", isActive: false);
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("INACTIVE")).ReturnsAsync(voucher);

        var request = new ValidateVoucherRequest { Code = "INACTIVE", OrderAmount = 100m };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_NonExistingCode_ReturnsInvalid()
    {
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("NOPE")).ReturnsAsync((Voucher?)null);

        var request = new ValidateVoucherRequest { Code = "NOPE", OrderAmount = 100m };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_BelowMinOrderAmount_ReturnsInvalid()
    {
        var voucher = CreateTestVoucher("MINORDER", minOrderAmount: 200m);
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("MINORDER")).ReturnsAsync(voucher);

        var request = new ValidateVoucherRequest { Code = "MINORDER", OrderAmount = 50m };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_MaxRedemptionsReached_ReturnsInvalid()
    {
        var voucher = CreateTestVoucher("MAXED", maxRedemptions: 5, currentRedemptions: 5);
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("MAXED")).ReturnsAsync(voucher);

        var request = new ValidateVoucherRequest { Code = "MAXED", OrderAmount = 100m };
        var result = await _sut.ValidateAsync(request);

        result.Success.Should().BeTrue();
        result.Data!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task RedeemAsync_ValidVoucher_ReturnsSuccess()
    {
        var voucher = CreateTestVoucher("REDEEM");
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("REDEEM")).ReturnsAsync(voucher);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new RedeemVoucherRequest { Code = "REDEEM", OrderId = Guid.NewGuid() };
        var result = await _sut.RedeemAsync(request, Guid.NewGuid());

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task RedeemAsync_InactiveVoucher_ReturnsFail()
    {
        var voucher = CreateTestVoucher("INACTIVE", isActive: false);
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("INACTIVE")).ReturnsAsync(voucher);

        var request = new RedeemVoucherRequest { Code = "INACTIVE", OrderId = Guid.NewGuid() };
        var result = await _sut.RedeemAsync(request, Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RedeemAsync_NonExistingVoucher_ReturnsFail()
    {
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("NOPE")).ReturnsAsync((Voucher?)null);

        var request = new RedeemVoucherRequest { Code = "NOPE", OrderId = Guid.NewGuid() };
        var result = await _sut.RedeemAsync(request, Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingVoucher_ReturnsFail()
    {
        _voucherRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Voucher?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task RedeemAsync_ExpiredButActiveVoucher_StillRedeems()
    {
        var voucher = CreateTestVoucher("EXPIRED-REDEEM",
            startDate: DateTime.UtcNow.AddDays(-60),
            expiryDate: DateTime.UtcNow.AddDays(-1));
        _voucherRepoMock.Setup(r => r.GetByCodeAsync("EXPIRED-REDEEM")).ReturnsAsync(voucher);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new RedeemVoucherRequest { Code = "EXPIRED-REDEEM", OrderId = Guid.NewGuid() };
        var result = await _sut.RedeemAsync(request, Guid.NewGuid());

        // Redeem only checks IsActive, not expiry dates
        result.Success.Should().BeTrue();
    }

    private static Voucher CreateTestVoucher(string code, bool isActive = true,
        DateTime? startDate = null, DateTime? expiryDate = null,
        decimal? minOrderAmount = null, int? maxRedemptions = null, int currentRedemptions = 0)
    {
        return new Voucher
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = $"Voucher {code}",
            VoucherType = VoucherType.Gift,
            Value = 10m,
            ValueType = VoucherValueType.Percentage,
            MinOrderAmount = minOrderAmount,
            MaxRedemptions = maxRedemptions,
            CurrentRedemptions = currentRedemptions,
            StartDate = startDate ?? DateTime.UtcNow.AddDays(-1),
            ExpiryDate = expiryDate ?? DateTime.UtcNow.AddDays(30),
            IsActive = isActive,
            CreatedBy = "test"
        };
    }
}
