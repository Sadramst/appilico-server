using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Payment-specific operations.
/// </summary>
public interface IPaymentRepository : IGenericRepository<Payment>
{
    /// <summary>Gets payments by order ID.</summary>
    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId);

    /// <summary>Gets a payment with refund details.</summary>
    Task<Payment?> GetWithRefundsAsync(Guid id);

    /// <summary>Gets a payment by external provider transaction ID.</summary>
    Task<Payment?> GetByTransactionIdAsync(string transactionId);
}
