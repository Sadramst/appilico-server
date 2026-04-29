using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Dashboard;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;
using System.Linq.Expressions;

namespace Appilico.Server.UnitTests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<DashboardService>> _loggerMock;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<DashboardService>>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Customers).Returns(_customerRepoMock.Object);

        _sut = new DashboardService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetSalesSummaryAsync_ReturnsValidSummary()
    {
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), TotalAmount = 100m, OrderStatus = OrderStatus.Delivered, OrderDate = DateTime.UtcNow, CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), TotalAmount = 200m, OrderStatus = OrderStatus.Confirmed, OrderDate = DateTime.UtcNow, CreatedBy = "test" }
        };
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(orders);
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(10);

        var result = await _sut.GetSalesSummaryAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalOrders.Should().Be(2);
        result.Data.TotalRevenue.Should().Be(300m);
    }

    [Fact]
    public async Task GetSalesSummaryAsync_NoOrders_ReturnsZeroSummary()
    {
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(new List<Order>());
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(0);

        var result = await _sut.GetSalesSummaryAsync();

        result.Success.Should().BeTrue();
        result.Data!.TotalOrders.Should().Be(0);
        result.Data.AverageOrderValue.Should().Be(0);
    }

    [Fact]
    public async Task GetTopProductsAsync_ReturnsTopProducts()
    {
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(new List<Order>());

        var result = await _sut.GetTopProductsAsync(5);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetRevenueChartAsync_ReturnsChartData()
    {
        var orders = new List<Order>
        {
            new() { Id = Guid.NewGuid(), TotalAmount = 100m, OrderStatus = OrderStatus.Delivered, OrderDate = DateTime.UtcNow, CreatedBy = "test" },
            new() { Id = Guid.NewGuid(), TotalAmount = 150m, OrderStatus = OrderStatus.Confirmed, OrderDate = DateTime.UtcNow.AddDays(-1), CreatedBy = "test" }
        };
        _orderRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>())).ReturnsAsync(orders);

        var result = await _sut.GetRevenueChartAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomerStatsAsync_ReturnsStats()
    {
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(50);

        var result = await _sut.GetCustomerStatsAsync();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCustomerStatsAsync_NoCustomers_ReturnsZeroStats()
    {
        _customerRepoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Customer, bool>>>())).ReturnsAsync(0);

        var result = await _sut.GetCustomerStatsAsync();

        result.Success.Should().BeTrue();
        result.Data!.TotalCustomers.Should().Be(0);
    }
}
