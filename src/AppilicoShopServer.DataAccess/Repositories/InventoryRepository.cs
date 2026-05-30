using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Inventory-specific operations.
/// </summary>
public class InventoryRepository : GenericRepository<InventoryTransaction>, IInventoryRepository
{
    /// <summary>Initializes a new instance of the <see cref="InventoryRepository"/> class.</summary>
    public InventoryRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<InventoryTransaction>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(it => it.Product)
            .Include(it => it.Variant)
            .Where(it => it.ProductId == productId)
            .OrderByDescending(it => it.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.StockQuantity <= p.MinStockLevel && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }
}
