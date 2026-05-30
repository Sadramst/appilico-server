using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>Waitlist repository.</summary>
public class WaitlistRepository : GenericRepository<WaitlistEntry>, IWaitlistRepository
{
    /// <summary>Initializes the repository.</summary>
    public WaitlistRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<WaitlistEntry?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(entry => entry.Email == email && !entry.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<int> CountActiveAsync()
    {
        return await _dbSet.CountAsync(entry => !entry.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<WaitlistEntry> Items, int TotalCount)> GetAdminPageAsync(int page, int pageSize, bool? isNotified)
    {
        var query = _dbSet.Where(entry => !entry.IsDeleted);
        if (isNotified.HasValue)
            query = query.Where(entry => entry.IsNotified == isNotified.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(entry => entry.Position)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<WaitlistEntry?> GetActiveByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(entry => entry.Id == id && !entry.IsDeleted);
    }
}