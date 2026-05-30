using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>Visual repository.</summary>
public class VisualRepository : GenericRepository<Visual>, IVisualRepository
{
    /// <summary>Initializes the repository.</summary>
    public VisualRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Visual>> GetActiveOrderedAsync()
    {
        return await _dbSet
            .Where(visual => visual.IsActive && !visual.IsDeleted)
            .OrderBy(visual => visual.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Visual> Items, int TotalCount)> GetPagedActiveAsync(
        VisualCategory? category,
        SubscriptionTier? requiredPlan,
        string? search,
        int page,
        int pageSize)
    {
        var query = _dbSet.Where(visual => visual.IsActive && !visual.IsDeleted);

        if (category.HasValue)
            query = query.Where(visual => visual.Category == category.Value);

        if (requiredPlan.HasValue)
            query = query.Where(visual => visual.RequiredPlan == requiredPlan.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(visual => visual.Name.Contains(search) || visual.Description.Contains(search));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(visual => visual.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<Visual?> GetVisibleByIdAsync(Guid id, bool requireActive = false)
    {
        var query = _dbSet.Where(visual => visual.Id == id && !visual.IsDeleted);
        if (requireActive)
            query = query.Where(visual => visual.IsActive);

        return await query.FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public async Task<Visual?> GetVisibleBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(visual => visual.Slug == slug && !visual.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<bool> SlugExistsAsync(string slug, Guid? excludingId = null)
    {
        return await _dbSet.AnyAsync(visual => visual.Slug == slug && !visual.IsDeleted && (!excludingId.HasValue || visual.Id != excludingId.Value));
    }

    /// <inheritdoc/>
    public async Task AddDownloadAsync(VisualDownload download)
    {
        await _context.VisualDownloads.AddAsync(download);
    }
}