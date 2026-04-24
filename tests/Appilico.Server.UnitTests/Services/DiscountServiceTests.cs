using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Discount;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

namespace Appilico.Server.UnitTests.Services;

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
}
