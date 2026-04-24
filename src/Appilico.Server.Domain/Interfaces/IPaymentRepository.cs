using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for Payment-specific operations.
/// </summary>
public interface IPaymentRepository : IGenericRepository<Payment>
{
    /// <summary>Gets payments by order ID.</summary>
    Task<IReadOnlyList<Payment>> GetByOrderIdAsync(Guid orderId);
}
