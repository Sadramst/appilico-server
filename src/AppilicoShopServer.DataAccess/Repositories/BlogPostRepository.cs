using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>Blog post repository.</summary>
public class BlogPostRepository : GenericRepository<BlogPost>, IBlogPostRepository
{
    /// <summary>Initializes the repository.</summary>
    public BlogPostRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedAsync(string? category, int page, int pageSize)
    {
        var query = _dbSet.Where(post => !post.IsDeleted && post.IsPublished);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(post => post.Category == category);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(post => post.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<BlogPost?> GetPublishedBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(post => post.Slug == slug && !post.IsDeleted && post.IsPublished);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BlogPost>> GetRelatedPublishedAsync(string category, string excludedSlug, int take)
    {
        return await _dbSet
            .Where(post => post.Category == category && post.Slug != excludedSlug && !post.IsDeleted && post.IsPublished)
            .OrderByDescending(post => post.PublishedAt)
            .Take(take)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<BlogPost?> GetEditableByIdAsync(Guid id)
    {
        return await _dbSet.FirstOrDefaultAsync(post => post.Id == id && !post.IsDeleted);
    }
}