using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Order;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

namespace Appilico.Server.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<ICartRepository> _cartRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _cartRepoMock = new Mock<ICartRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<OrderService>>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Carts).Returns(_cartRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new OrderService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId);
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);

        var result = await _sut.GetByIdAsync(orderId);

        result.Success.Should().BeTrue();
        result.Data!.OrderNumber.Should().Be("ORD-001");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingOrder_ReturnsFail()
    {
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedOrders()
    {
        var orders = new List<Order> { CreateTestOrder(), CreateTestOrder(orderNumber: "ORD-002") };
        _orderRepoMock.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, object>>[]>()))
            .ReturnsAsync((orders.AsReadOnly() as IReadOnlyList<Order>, 2));

        var result = await _sut.GetAllAsync(1, 10);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCustomerAsync_ReturnsCustomerOrders()
    {
        var customerId = Guid.NewGuid();
        var orders = new List<Order> { CreateTestOrder(customerId: customerId) };
        _orderRepoMock.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Order, object>>[]>()))
            .ReturnsAsync((orders.AsReadOnly() as IReadOnlyList<Order>, 1));

        var result = await _sut.GetByCustomerAsync(customerId);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, status: OrderStatus.Pending);
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new UpdateOrderStatusRequest { NewStatus = OrderStatus.Confirmed, Notes = "Confirmed" };
        var result = await _sut.UpdateStatusAsync(orderId, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateStatusAsync_NonExistingOrder_ReturnsFail()
    {
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var request = new UpdateOrderStatusRequest { NewStatus = OrderStatus.Confirmed };
        var result = await _sut.UpdateStatusAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_PendingOrder_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, status: OrderStatus.Pending);
        order.Items = new List<OrderItem>
        {
            new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "P1", UnitPrice = 50, Quantity = 2, TotalPrice = 100, CreatedBy = "test" }
        };
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Product { Id = Guid.NewGuid(), Name = "P1", StockQuantity = 10, CreatedBy = "test" });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CancelAsync(orderId, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CancelAsync_ConfirmedOrder_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, status: OrderStatus.Confirmed);
        order.Items = new List<OrderItem>();
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CancelAsync(orderId, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CancelAsync_DeliveredOrder_ReturnsFail()
    {
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, status: OrderStatus.Delivered);
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);

        var result = await _sut.CancelAsync(orderId, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_ShippedOrder_ReturnsFail()
    {
        var orderId = Guid.NewGuid();
        var order = CreateTestOrder(orderId, status: OrderStatus.Shipped);
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);

        var result = await _sut.CancelAsync(orderId, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetStatusHistoryAsync_ReturnsHistory()
    {
        var orderId = Guid.NewGuid();
        var history = new List<OrderStatusHistory>
        {
            new() { Id = Guid.NewGuid(), OrderId = orderId, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Confirmed, ChangedAt = DateTime.UtcNow, CreatedBy = "admin" }
        };
        _orderRepoMock.Setup(r => r.GetStatusHistoryAsync(orderId)).ReturnsAsync(history);

        var result = await _sut.GetStatusHistoryAsync(orderId);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateFromCartAsync_EmptyCart_ReturnsFail()
    {
        var customerId = Guid.NewGuid();
        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync((Cart?)null);

        var request = new CreateOrderRequest { ShippingAddressId = Guid.NewGuid(), BillingAddressId = Guid.NewGuid(), PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.CreateFromCartAsync(customerId, request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateFromCartAsync_CartWithItems_ReturnsSuccess()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product", BasePrice = 50m, StockQuantity = 100, CreatedBy = "test" };
        var cart = new Cart
        {
            Id = cartId, CustomerId = customerId, IsActive = true, CreatedBy = "test",
            Items = new List<CartItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2, UnitPrice = 50m, CreatedBy = "test" }
            }
        };

        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);

        var request = new CreateOrderRequest { ShippingAddressId = Guid.NewGuid(), BillingAddressId = Guid.NewGuid(), PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.CreateFromCartAsync(customerId, request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateFromCartAsync_InsufficientStock_ReturnsFail()
    {
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cartId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test Product", BasePrice = 50m, StockQuantity = 1, CreatedBy = "test" };
        var cart = new Cart
        {
            Id = cartId, CustomerId = customerId, IsActive = true, CreatedBy = "test",
            Items = new List<CartItem>
            {
                new() { Id = Guid.NewGuid(), ProductId = productId, Quantity = 10, UnitPrice = 50m, CreatedBy = "test" }
            }
        };

        _cartRepoMock.Setup(r => r.GetActiveCartAsync(customerId)).ReturnsAsync(cart);
        _cartRepoMock.Setup(r => r.GetWithItemsAsync(cartId)).ReturnsAsync(cart);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

        var request = new CreateOrderRequest { ShippingAddressId = Guid.NewGuid(), BillingAddressId = Guid.NewGuid(), PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.CreateFromCartAsync(customerId, request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_PendingOrder_RestoresStock()
    {
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "Test", StockQuantity = 5, CreatedBy = "test" };
        var order = CreateTestOrder(orderId, status: OrderStatus.Pending);
        order.Items = new List<OrderItem>
        {
            new() { Id = Guid.NewGuid(), ProductId = productId, ProductName = "Test", Quantity = 3, UnitPrice = 10m, TotalPrice = 30m, CreatedBy = "test" }
        };

        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(orderId)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _sut.CancelAsync(orderId, "user1");

        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(8); // 5 + 3 restored
    }

    [Fact]
    public async Task CancelAsync_NonExistingOrder_ReturnsFail()
    {
        _orderRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await _sut.CancelAsync(Guid.NewGuid(), "user1");

        result.Success.Should().BeFalse();
    }

    private static Order CreateTestOrder(Guid? id = null, string orderNumber = "ORD-001",
        OrderStatus status = OrderStatus.Pending, Guid? customerId = null)
    {
        return new Order
        {
            Id = id ?? Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = customerId ?? Guid.NewGuid(),
            OrderStatus = status,
            SubTotal = 100m,
            TaxAmount = 10m,
            ShippingAmount = 5m,
            TotalAmount = 115m,
            PaymentStatus = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.CreditCard,
            OrderDate = DateTime.UtcNow,
            CreatedBy = "test",
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    UnitPrice = 50m,
                    Quantity = 2,
                    TotalPrice = 100m,
                    CreatedBy = "test"
                }
            }
        };
    }
}
