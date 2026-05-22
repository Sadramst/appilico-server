using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Appilico.Server.Business.Exceptions;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Payment;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Business.Options;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Payment service implementation.</summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStripeService _stripeService;
    private readonly StripeOptions _stripeOptions;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    /// <summary>Initializes a new instance of PaymentService.</summary>
    public PaymentService(IUnitOfWork unitOfWork, IStripeService stripeService, IOptions<StripeOptions> stripeOptions, IMapper mapper, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
        _stripeService = stripeService;
        _stripeOptions = stripeOptions.Value;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<PaymentDto>>> GetByOrderAsync(Guid orderId)
    {
        var payments = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        return ApiResponse<List<PaymentDto>>.SuccessResponse(_mapper.Map<List<PaymentDto>>(payments));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PaymentDto>> GetByIdAsync(Guid id)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id);
        if (payment == null)
            return ApiResponse<PaymentDto>.FailResponse("Payment not found");

        return ApiResponse<PaymentDto>.SuccessResponse(_mapper.Map<PaymentDto>(payment));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PaymentDto>> ProcessPaymentAsync(CreatePaymentRequest request, string userId)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
        if (order == null)
            return ApiResponse<PaymentDto>.FailResponse("Order not found");

        if (request.Amount <= 0)
            return ApiResponse<PaymentDto>.FailResponse("Payment amount must be greater than zero");

        if (request.Amount != order.TotalAmount)
            return ApiResponse<PaymentDto>.FailResponse("Payment amount must match the order total");

        if (order.OrderStatus is OrderStatus.Cancelled or OrderStatus.Returned or OrderStatus.Refunded)
            return ApiResponse<PaymentDto>.FailResponse("Payment cannot be processed for this order status");

        if (order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded)
            return ApiResponse<PaymentDto>.FailResponse("Order payment has already been completed");

        if (request.PaymentMethod == PaymentMethod.PayPal)
            return ApiResponse<PaymentDto>.FailResponse("PayPal payments are not supported by this backend.");

        if (RequiresExternalProcessor(request.PaymentMethod))
        {
            return await CreateProviderBackedPaymentAsync(request, order, userId);
        }

        var payment = _mapper.Map<Domain.Entities.Payment>(request);
        payment.Status = PaymentStatus.Pending;
        payment.CreatedBy = userId;

        await _unitOfWork.Payments.AddAsync(payment);

        order.PaymentStatus = PaymentStatus.Pending;
        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Pending offline payment {PaymentId} recorded for order {OrderId}", payment.Id, request.OrderId);
        return ApiResponse<PaymentDto>.SuccessResponse(_mapper.Map<PaymentDto>(payment), "Payment recorded as pending");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<RefundDto>> CreateRefundAsync(Guid paymentId, CreateRefundRequest request, string userId)
    {
        var payment = await _unitOfWork.Payments.GetWithRefundsAsync(paymentId);
        if (payment == null)
            return ApiResponse<RefundDto>.FailResponse("Payment not found");

        if (request.Amount <= 0)
            return ApiResponse<RefundDto>.FailResponse("Refund amount must be greater than zero");

        if (payment.Status is not PaymentStatus.Paid and not PaymentStatus.PartiallyRefunded)
            return ApiResponse<RefundDto>.FailResponse("Only paid payments can be refunded");

        var alreadyRefunded = payment.Refunds
            .Where(refund => refund.Status != RefundStatus.Rejected)
            .Sum(refund => refund.Amount);
        var remainingRefundable = payment.Amount - alreadyRefunded;

        if (request.Amount > remainingRefundable)
            return ApiResponse<RefundDto>.FailResponse("Refund amount exceeds the remaining refundable balance");

        StripeRefundResult? providerRefund = null;
        if (RequiresExternalProcessor(payment.PaymentMethod))
        {
            if (string.IsNullOrWhiteSpace(payment.TransactionId))
                return ApiResponse<RefundDto>.FailResponse("Payment is missing provider transaction details and cannot be refunded automatically");

            try
            {
                providerRefund = await _stripeService.CreateRefundAsync(new StripeRefundRequest(
                    payment.TransactionId,
                    request.Amount,
                    _stripeOptions.Currency,
                    request.Reason,
                    $"refund:{paymentId}:{request.Amount}"));
            }
            catch (PaymentProviderException ex)
            {
                _logger.LogWarning(ex, "Stripe refund failed for payment {PaymentId}. ProviderRequestId={ProviderRequestId}", paymentId, ex.ProviderRequestId);
                return ApiResponse<RefundDto>.FailResponse(ex.Message);
            }
        }

        var refundStatus = providerRefund == null || string.Equals(providerRefund.Status, "succeeded", StringComparison.OrdinalIgnoreCase)
            ? RefundStatus.Processed
            : RefundStatus.Pending;

        var refund = new Refund
        {
            OrderId = payment.OrderId,
            PaymentId = paymentId,
            Amount = request.Amount,
            Reason = request.Reason,
            Status = refundStatus,
            RefundedAt = refundStatus == RefundStatus.Processed ? DateTime.UtcNow : null,
            ProviderRefundId = providerRefund?.RefundId,
            CreatedBy = userId
        };

        payment.Refunds.Add(refund);
        if (refundStatus == RefundStatus.Processed)
        {
            payment.Status = request.Amount == remainingRefundable
                ? PaymentStatus.Refunded
                : PaymentStatus.PartiallyRefunded;
        }

        var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
        if (order != null && refundStatus == RefundStatus.Processed)
        {
            order.PaymentStatus = payment.Status;
            if (payment.Status == PaymentStatus.Refunded)
                order.OrderStatus = OrderStatus.Refunded;
            _unitOfWork.Orders.Update(order);
        }

        _unitOfWork.Payments.Update(payment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refund created for payment {PaymentId} amount {Amount}", paymentId, request.Amount);
        return ApiResponse<RefundDto>.SuccessResponse(_mapper.Map<RefundDto>(refund), "Refund created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<RefundDto>>> GetRefundsByOrderAsync(Guid orderId)
    {
        var payments = await _unitOfWork.Payments.GetByOrderIdAsync(orderId);
        var refunds = payments.SelectMany(payment => payment.Refunds).OrderByDescending(refund => refund.RefundedAt).ToList();
        return ApiResponse<List<RefundDto>>.SuccessResponse(_mapper.Map<List<RefundDto>>(refunds));
    }

    private static bool RequiresExternalProcessor(PaymentMethod paymentMethod)
    {
        return paymentMethod is PaymentMethod.CreditCard or PaymentMethod.DebitCard;
    }

    private async Task<ApiResponse<PaymentDto>> CreateProviderBackedPaymentAsync(CreatePaymentRequest request, Order order, string userId)
    {
        if (!_stripeOptions.Enabled || !_stripeOptions.HasRequiredSettings)
        {
            _logger.LogWarning("Rejected Stripe payment for order {OrderId}: Stripe is disabled or misconfigured", request.OrderId);
            return ApiResponse<PaymentDto>.FailResponse("Card payments are temporarily unavailable.");
        }

        var payment = _mapper.Map<Domain.Entities.Payment>(request);
        payment.Status = PaymentStatus.Pending;
        payment.CreatedBy = userId;

        try
        {
            var intent = await _stripeService.CreatePaymentIntentAsync(new StripePaymentIntentRequest(
                request.Amount,
                _stripeOptions.Currency,
                $"Appilico order {order.OrderNumber}",
                $"payment-intent:{request.OrderId}:{request.Amount}",
                new Dictionary<string, string>
                {
                    ["orderId"] = request.OrderId.ToString(),
                    ["userId"] = userId,
                    ["orderNumber"] = order.OrderNumber
                }));

            payment.TransactionId = intent.PaymentIntentId;
            await _unitOfWork.Payments.AddAsync(payment);

            order.PaymentStatus = PaymentStatus.Pending;
            order.PaymentMethod = request.PaymentMethod;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<PaymentDto>(payment);
            dto.ProviderClientSecret = intent.ClientSecret;
            dto.ProviderStatus = intent.Status;

            _logger.LogInformation("Stripe PaymentIntent {PaymentIntentId} created for order {OrderId}", intent.PaymentIntentId, request.OrderId);
            return ApiResponse<PaymentDto>.SuccessResponse(dto, "Payment intent created; client confirmation is required");
        }
        catch (PaymentProviderException ex)
        {
            _logger.LogWarning(ex, "Stripe payment intent creation failed for order {OrderId}. ProviderRequestId={ProviderRequestId}", request.OrderId, ex.ProviderRequestId);
            return ApiResponse<PaymentDto>.FailResponse(ex.Message);
        }
    }
}
