using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for Discount-specific operations.
/// </summary>
public class DiscountRepository : GenericRepository<Discount>, IDiscountRepository
{
    /// <summary>Initializes a new instance of the <see cref="DiscountRepository"/> class.</summary>
    public DiscountRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Discount?> GetByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Code == code);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Discount>> GetActiveDiscountsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(d => d.IsActive && d.StartDate <= now && d.EndDate >= now)
            .ToListAsync();
    }
}
