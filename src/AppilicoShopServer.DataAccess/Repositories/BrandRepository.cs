using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Brand-specific operations.
/// </summary>
public class BrandRepository : GenericRepository<Brand>, IBrandRepository
{
    /// <summary>Initializes a new instance of the <see cref="BrandRepository"/> class.</summary>
    public BrandRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Brand?> GetWithProductsAsync(Guid id)
    {
        return await _dbSet
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
