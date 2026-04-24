using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for SpecialOffer-specific operations.
/// </summary>
public class SpecialOfferRepository : GenericRepository<SpecialOffer>, ISpecialOfferRepository
{
    /// <summary>Initializes a new instance of the <see cref="SpecialOfferRepository"/> class.</summary>
    public SpecialOfferRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SpecialOffer>> GetActiveOffersAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(o => o.SpecialOfferProducts)
                .ThenInclude(sop => sop.Product)
                    .ThenInclude(p => p.Images)
            .Where(o => o.IsActive && o.StartDate <= now && o.EndDate >= now)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<SpecialOffer?> GetWithProductsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.SpecialOfferProducts)
                .ThenInclude(sop => sop.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
