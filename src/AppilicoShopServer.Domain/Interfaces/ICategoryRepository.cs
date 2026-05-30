using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Category-specific operations.
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>Gets the full category tree with children.</summary>
    Task<IReadOnlyList<Category>> GetCategoryTreeAsync();

    /// <summary>Gets a category with its subcategories.</summary>
    Task<Category?> GetWithSubCategoriesAsync(Guid id);
}
