using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for Cart-specific operations.
/// </summary>
public interface ICartRepository : IGenericRepository<Cart>
{
    /// <summary>Gets the active cart for a customer.</summary>
    Task<Cart?> GetActiveCartAsync(Guid customerId);

    /// <summary>Gets the active cart by session ID.</summary>
    Task<Cart?> GetBySessionIdAsync(string sessionId);

    /// <summary>Gets a cart with items and product details.</summary>
    Task<Cart?> GetWithItemsAsync(Guid cartId);
}
