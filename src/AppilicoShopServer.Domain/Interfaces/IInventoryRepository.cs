using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Inventory-specific operations.
/// </summary>
public interface IInventoryRepository : IGenericRepository<InventoryTransaction>
{
    /// <summary>Gets transactions by product ID.</summary>
    Task<IReadOnlyList<InventoryTransaction>> GetByProductIdAsync(Guid productId);

    /// <summary>Gets products with stock below minimum level.</summary>
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync();
}
