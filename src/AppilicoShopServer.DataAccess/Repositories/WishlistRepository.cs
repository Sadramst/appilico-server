using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Wishlist-specific operations.
/// </summary>
public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
{
    /// <summary>Initializes a new instance of the <see cref="WishlistRepository"/> class.</summary>
    public WishlistRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Wishlist>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet
            .Include(w => w.Product)
                .ThenInclude(p => p.Images)
            .Where(w => w.CustomerId == customerId)
            .OrderByDescending(w => w.AddedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Wishlist?> GetByCustomerAndProductAsync(Guid customerId, Guid productId)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId);
    }

    /// <inheritdoc/>
    public async Task<Wishlist?> GetSoftDeletedByCustomerAndProductAsync(Guid customerId, Guid productId)
    {
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId && w.IsDeleted);
    }
}
