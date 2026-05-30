using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Product-specific operations.
/// </summary>
public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>Gets products by category ID with includes.</summary>
    Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId);

    /// <summary>Gets featured products.</summary>
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count);

    /// <summary>Gets a product with all related data.</summary>
    Task<Product?> GetWithDetailsAsync(Guid id);

    /// <summary>Searches products by name, description, or SKU.</summary>
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm, Guid? categoryId, Guid? brandId,
        decimal? minPrice, decimal? maxPrice,
        int page, int pageSize, string? sortBy, bool sortDescending);
}
