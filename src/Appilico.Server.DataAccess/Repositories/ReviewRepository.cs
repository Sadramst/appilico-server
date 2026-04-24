using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for ProductReview-specific operations.
/// </summary>
public class ReviewRepository : GenericRepository<ProductReview>, IReviewRepository
{
    /// <summary>Initializes a new instance of the <see cref="ReviewRepository"/> class.</summary>
    public ReviewRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductReview>> GetByProductIdAsync(Guid productId)
    {
        return await _dbSet
            .Include(r => r.Customer)
                .ThenInclude(c => c.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductReview>> GetByCustomerIdAsync(Guid customerId)
    {
        return await _dbSet
            .Include(r => r.Product)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
