using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Cart-specific operations.
/// </summary>
public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    /// <summary>Initializes a new instance of the <see cref="CartRepository"/> class.</summary>
    public CartRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Cart?> GetActiveCartAsync(Guid customerId)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Images)
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Variant)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsActive);
    }

    /// <inheritdoc/>
    public async Task<Cart?> GetBySessionIdAsync(string sessionId)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive);
    }

    /// <inheritdoc/>
    public async Task<Cart?> GetWithItemsAsync(Guid cartId)
    {
        return await _dbSet
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Images)
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Variant)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }
}
