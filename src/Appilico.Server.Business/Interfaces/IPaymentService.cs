using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Payment;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Payment service interface.</summary>
public interface IPaymentService
{
    /// <summary>Gets payments for an order.</summary>
    Task<ApiResponse<List<PaymentDto>>> GetByOrderAsync(Guid orderId);

    /// <summary>Gets a payment by ID.</summary>
    Task<ApiResponse<PaymentDto>> GetByIdAsync(Guid id);

    /// <summary>Processes a payment.</summary>
    Task<ApiResponse<PaymentDto>> ProcessPaymentAsync(CreatePaymentRequest request, string userId);

    /// <summary>Creates a refund.</summary>
    Task<ApiResponse<RefundDto>> CreateRefundAsync(Guid paymentId, CreateRefundRequest request, string userId);

    /// <summary>Gets refunds for an order.</summary>
    Task<ApiResponse<List<RefundDto>>> GetRefundsByOrderAsync(Guid orderId);
}
