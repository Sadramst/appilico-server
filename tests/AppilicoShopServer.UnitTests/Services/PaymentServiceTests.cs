using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using AppilicoShopServer.Business.DTOs.Payment;
using AppilicoShopServer.Business.Exceptions;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Business.Options;
using AppilicoShopServer.Business.Services;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using AppilicoShopServer.UnitTests.Helpers;

namespace AppilicoShopServer.UnitTests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPaymentRepository> _paymentRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IStripeService> _stripeServiceMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<PaymentService>> _loggerMock;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _paymentRepoMock = new Mock<IPaymentRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _stripeServiceMock = new Mock<IStripeService>();
        _mapper = TestMapperConfig.CreateMapper();
        _loggerMock = new Mock<ILogger<PaymentService>>();

        _unitOfWorkMock.Setup(u => u.Payments).Returns(_paymentRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);

        _sut = new PaymentService(
            _unitOfWorkMock.Object,
            _stripeServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StripeOptions { Enabled = false }),
            _mapper,
            _loggerMock.Object);
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
    public async Task ProcessPaymentAsync_OfflinePaymentMethod_RecordsPendingPayment()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, TotalAmount = 100m, PaymentStatus = PaymentStatus.Pending, OrderStatus = OrderStatus.Pending, CreatedBy = "test" };
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment payment) => payment);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreatePaymentRequest { OrderId = orderId, Amount = 100m, PaymentMethod = PaymentMethod.BankTransfer };
        var result = await _sut.ProcessPaymentAsync(request, "user1");

        result.Success.Should().BeTrue();
        result.Data!.Status.Should().Be(PaymentStatus.Pending);
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        _paymentRepoMock.Verify(r => r.AddAsync(It.Is<Payment>(payment => payment.Status == PaymentStatus.Pending && payment.PaidAt == null)), Times.Once);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ExternalProcessorMethod_ReturnsFailClosed()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, TotalAmount = 100m, PaymentStatus = PaymentStatus.Pending, OrderStatus = OrderStatus.Pending, CreatedBy = "test" };
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        var request = new CreatePaymentRequest { OrderId = orderId, Amount = 100m, PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.ProcessPaymentAsync(request, "user1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Card payments are temporarily unavailable");
        _paymentRepoMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentAsync_CardPayment_CreatesPendingProviderBackedPayment()
    {
        var sut = new PaymentService(
            _unitOfWorkMock.Object,
            _stripeServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StripeOptions
            {
                Enabled = true,
                SecretKey = "sk_test",
                PublishableKey = "pk_test",
                WebhookSecret = "whsec_test",
                Currency = "aud"
            }),
            _mapper,
            _loggerMock.Object);

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            OrderNumber = "ORD-1001",
            TotalAmount = 100m,
            PaymentStatus = PaymentStatus.Pending,
            OrderStatus = OrderStatus.Pending,
            CreatedBy = "test"
        };

        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
        _paymentRepoMock.Setup(r => r.AddAsync(It.IsAny<Payment>())).ReturnsAsync((Payment payment) => payment);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _stripeServiceMock
            .Setup(s => s.CreatePaymentIntentAsync(It.Is<StripePaymentIntentRequest>(request =>
                request.Amount == 100m && request.Currency == "aud" && request.Metadata["orderId"] == orderId.ToString()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentIntentResult("client_secret", "pi_123", "requires_payment_method"));

        var result = await sut.ProcessPaymentAsync(new CreatePaymentRequest
        {
            OrderId = orderId,
            Amount = 100m,
            PaymentMethod = PaymentMethod.CreditCard
        }, "user1");

        result.Success.Should().BeTrue();
        result.Data!.ProviderClientSecret.Should().Be("client_secret");
        result.Data.ProviderStatus.Should().Be("requires_payment_method");
        order.PaymentStatus.Should().Be(PaymentStatus.Pending);
        _paymentRepoMock.Verify(r => r.AddAsync(It.Is<Payment>(payment =>
            payment.TransactionId == "pi_123" && payment.Status == PaymentStatus.Pending)), Times.Once);
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
        var payment = new Payment { Id = paymentId, OrderId = Guid.NewGuid(), Amount = 100m, PaymentMethod = PaymentMethod.BankTransfer, Status = PaymentStatus.Paid, CreatedBy = "test" };
        _paymentRepoMock.Setup(r => r.GetWithRefundsAsync(paymentId)).ReturnsAsync(payment);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var request = new CreateRefundRequest { Amount = 50m, Reason = "Defective item" };
        var result = await _sut.CreateRefundAsync(paymentId, request, "admin1");

        result.Success.Should().BeTrue();
        payment.Refunds.Should().ContainSingle();
        payment.Status.Should().Be(PaymentStatus.PartiallyRefunded);
        _paymentRepoMock.Verify(r => r.Update(payment), Times.Once);
    }

    [Fact]
    public async Task CreateRefundAsync_ProviderFailure_DoesNotMutatePaymentOrPersistRefund()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = Guid.NewGuid(),
            Amount = 100m,
            PaymentMethod = PaymentMethod.CreditCard,
            TransactionId = "pi_123",
            Status = PaymentStatus.Paid,
            Refunds = new List<Refund>(),
            CreatedBy = "test"
        };

        var sut = new PaymentService(
            _unitOfWorkMock.Object,
            _stripeServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StripeOptions
            {
                Enabled = true,
                SecretKey = "sk_test",
                PublishableKey = "pk_test",
                WebhookSecret = "whsec_test",
                Currency = "aud"
            }),
            _mapper,
            _loggerMock.Object);

        _paymentRepoMock.Setup(r => r.GetWithRefundsAsync(paymentId)).ReturnsAsync(payment);
        _stripeServiceMock
            .Setup(s => s.CreateRefundAsync(It.IsAny<StripeRefundRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PaymentProviderException("Stripe refund failed"));

        var result = await sut.CreateRefundAsync(paymentId, new CreateRefundRequest { Amount = 50m, Reason = "Requested" }, "admin1");

        result.Success.Should().BeFalse();
        payment.Refunds.Should().BeEmpty();
        payment.Status.Should().Be(PaymentStatus.Paid);
        _paymentRepoMock.Verify(r => r.Update(It.IsAny<Payment>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateRefundAsync_NonExistingPayment_ReturnsFail()
    {
        _paymentRepoMock.Setup(r => r.GetWithRefundsAsync(It.IsAny<Guid>())).ReturnsAsync((Payment?)null);

        var request = new CreateRefundRequest { Amount = 50m, Reason = "Reason" };
        var result = await _sut.CreateRefundAsync(Guid.NewGuid(), request, "admin1");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task CreateRefundAsync_RefundExceedsRemainingBalance_ReturnsFail()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = Guid.NewGuid(),
            Amount = 100m,
            Status = PaymentStatus.PartiallyRefunded,
            Refunds = new List<Refund> { new() { Amount = 80m, Status = RefundStatus.Processed } }
        };
        _paymentRepoMock.Setup(r => r.GetWithRefundsAsync(paymentId)).ReturnsAsync(payment);

        var request = new CreateRefundRequest { Amount = 25m, Reason = "Too much" };
        var result = await _sut.CreateRefundAsync(paymentId, request, "admin1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("remaining refundable balance");
    }

    [Fact]
    public async Task CreateRefundAsync_UnpaidPayment_ReturnsFail()
    {
        var paymentId = Guid.NewGuid();
        var payment = new Payment { Id = paymentId, OrderId = Guid.NewGuid(), Amount = 100m, Status = PaymentStatus.Pending };
        _paymentRepoMock.Setup(r => r.GetWithRefundsAsync(paymentId)).ReturnsAsync(payment);

        var request = new CreateRefundRequest { Amount = 50m, Reason = "Invalid" };
        var result = await _sut.CreateRefundAsync(paymentId, request, "admin1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Only paid payments");
    }

    [Fact]
    public async Task GetRefundsByOrderAsync_ReturnsPersistedRefunds()
    {
        var orderId = Guid.NewGuid();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 100m,
            Refunds = new List<Refund>
            {
                new() { Id = Guid.NewGuid(), OrderId = orderId, Amount = 40m, Status = RefundStatus.Processed, Reason = "Returned" }
            }
        };
        _paymentRepoMock.Setup(r => r.GetByOrderIdAsync(orderId)).ReturnsAsync(new List<Payment> { payment });

        var result = await _sut.GetRefundsByOrderAsync(orderId);

        result.Success.Should().BeTrue();
        result.Data.Should().ContainSingle();
        result.Data![0].Amount.Should().Be(40m);
    }

    [Fact]
    public async Task ProcessPaymentAsync_AmountMismatch_ReturnsFail()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, TotalAmount = 100m, PaymentStatus = PaymentStatus.Pending, OrderStatus = OrderStatus.Pending };
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        var request = new CreatePaymentRequest { OrderId = orderId, Amount = 99m, PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.ProcessPaymentAsync(request, "user1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("match the order total");
    }

    [Fact]
    public async Task ProcessPaymentAsync_AlreadyPaidOrder_ReturnsFail()
    {
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, TotalAmount = 100m, PaymentStatus = PaymentStatus.Paid, OrderStatus = OrderStatus.Confirmed };
        _orderRepoMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

        var request = new CreatePaymentRequest { OrderId = orderId, Amount = 100m, PaymentMethod = PaymentMethod.CreditCard };
        var result = await _sut.ProcessPaymentAsync(request, "user1");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already been completed");
    }
}
