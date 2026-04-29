using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Appilico.Server.Business.DTOs.Payment;
using Appilico.Server.Business.Services;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;
using Appilico.Server.UnitTests.Helpers;

namespace Appilico.Server.UnitTests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<PaymentService>>();

        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);

        _sut = new PaymentService(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByOrderAsync_ReturnsPayments()
    {
        var orderId = Guid.NewGuid();
        var payments = new List<Payment>
        {
            new() { Id = Guid.NewGuid(), OrderId = orderId, Amount = 100m, PaymentMethod = PaymentMethod.CreditCard, Status = PaymentStatus.Paid, CreatedBy = "test" }
        };
        _paymentRepoMock.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(payments);

        var result = await _sut.GetByOrderAsync(orderId);

        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByOrderAsync_NoPayments_ReturnsEmptyList()
    {
        _paymentRepoMock.Setup(r => r.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<Payment>());

        var result = await _sut.GetByOrderAsync(Guid.NewGuid());

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingPayment_ReturnsSuccess()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment { Id = paymentId, OrderId = Guid.NewGuid(), Amount = 50m, PaymentMethod = PaymentMethod.PayPal, Status = PaymentStatus.Paid, CreatedBy = "test" };
        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

        var result = await _sut.GetByIdAsync(paymentId);

        result.Success.Should().BeTrue();
        result.Data!.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPayment_ReturnsFail()
    {
        _paymentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessPaymentAsync_ValidOrder_ReturnsSuccess()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, TotalAmount = 100m, PaymentStatus = PaymentStatus.Pending, OrderStatus = OrderStatus.Pending, CreatedBy = "test" };
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreatePaymentRequest { OrderId = orderId, Amount = 100m, PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.ProcessPaymentAsync(request, "user1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessPaymentAsync_NonExistingOrder_ReturnsFail()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var request = new CreatePaymentRequest { OrderId = Guid.NewGuid(), Amount = 100m, PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.ProcessPaymentAsync(request, "user1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRefundAsync_ValidPayment_ReturnsSuccess()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment { Id = paymentId, OrderId = Guid.NewGuid(), Amount = 100m, Status = PaymentStatus.Paid, CreatedBy = "test" };
        _paymentRepoMock.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateRefundRequest { Amount = 50m, Reason = "Defective item" };
        var result = await _sut.CreateRefundAsync(paymentId, request, "admin1");

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRefundAsync_NonExistingPayment_ReturnsFail()
    {
        _paymentRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Payment?)null);

        var request = new CreateRefundRequest { Amount = 50m, Reason = "Reason" };
        var result = await _sut.CreateRefundAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }
}
