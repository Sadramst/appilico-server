using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Domain.Interfaces;

/// <summary>
/// Repository interface for Wishlist-specific operations.
/// </summary>
public interface IWishlistRepository : IGenericRepository<Wishlist>
{
    /// <summary>Gets wishlist items by customer ID.</summary>
    Task<IReadOnlyList<Wishlist>> GetByCustomerIdAsync(Guid customerId);

    /// <summary>Gets a specific wishlist item.</summary>
    Task<Wishlist?> GetByCustomerAndProductAsync(Guid customerId, Guid productId);

    /// <summary>Gets a soft-deleted wishlist item for restore flows.</summary>
    Task<Wishlist?> GetSoftDeletedByCustomerAndProductAsync(Guid customerId, Guid productId);
}
