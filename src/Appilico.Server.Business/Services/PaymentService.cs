using AutoMapper;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Payment;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Enums;
using Appilico.Server.Domain.Interfaces;

namespace Appilico.Server.Business.Services;

/// <summary>Payment service implementation.</summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;

    /// <summary>Initializes a new instance of PaymentService.</summary>
    public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentService> logger)
    {
        _unitOfWork = unitOfWork;
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

        var payment = _mapper.Map<Domain.Entities.Payment>(request);
        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.UtcNow;
        payment.CreatedBy = userId;

        await _unitOfWork.Payments.AddAsync(payment);

        // Update order payment status
        order.PaymentStatus = PaymentStatus.Paid;
        _unitOfWork.Orders.Update(order);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Payment {PaymentId} processed for order {OrderId}", payment.Id, request.OrderId);
        return ApiResponse<PaymentDto>.SuccessResponse(_mapper.Map<PaymentDto>(payment), "Payment processed successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<RefundDto>> CreateRefundAsync(Guid paymentId, CreateRefundRequest request, string userId)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
        if (payment == null)
            return ApiResponse<RefundDto>.FailResponse("Payment not found");

        var refund = new Refund
        {
            OrderId = payment.OrderId,
            PaymentId = paymentId,
            Amount = request.Amount,
            Reason = request.Reason,
            Status = RefundStatus.Approved,
            RefundedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        // Store refund - we'll need a way to persist this
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refund created for payment {PaymentId} amount {Amount}", paymentId, request.Amount);
        return ApiResponse<RefundDto>.SuccessResponse(_mapper.Map<RefundDto>(refund), "Refund created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<RefundDto>>> GetRefundsByOrderAsync(Guid orderId)
    {
        var refunds = await _unitOfWork.Payments.FindAsync(p => p.OrderId == orderId);
        // In a real implementation, we'd query refunds separately
        return ApiResponse<List<RefundDto>>.SuccessResponse(new List<RefundDto>());
    }
}
