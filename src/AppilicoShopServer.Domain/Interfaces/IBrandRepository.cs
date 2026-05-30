using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Brand-specific operations.
/// </summary>
public interface IBrandRepository : IGenericRepository<Brand>
{
    /// <summary>Gets a brand with its products.</summary>
    Task<Brand?> GetWithProductsAsync(Guid id);
}
