using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Product-specific operations.
/// </summary>
public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    /// <summary>Initializes a new instance of the <see cref="ProductRepository"/> class.</summary>
    public ProductRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(Guid categoryId)
    {
        var subCategoryIds = await _context.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();

        var categoryIds = subCategoryIds.Count > 0
            ? subCategoryIds.Append(categoryId).ToList()
            : new List<Guid> { categoryId };

        return await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Brand)
            .Where(p => categoryIds.Contains(p.CategoryId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count)
    {
        return await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Brand)
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Product?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(
        string? searchTerm, Guid? categoryId, Guid? brandId,
        decimal? minPrice, decimal? maxPrice,
        int page, int pageSize, string? sortBy, bool sortDescending)
    {
        var query = _dbSet
            .Include(p => p.Images)
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)) ||
                p.SKU.ToLower().Contains(term));
        }

        if (categoryId.HasValue)
        {
            var subCategoryIds = await _context.Categories
                .Where(c => c.ParentCategoryId == categoryId.Value)
                .Select(c => c.Id)
                .ToListAsync();

            if (subCategoryIds.Count > 0)
            {
                subCategoryIds.Add(categoryId.Value);
                query = query.Where(p => subCategoryIds.Contains(p.CategoryId));
            }
            else
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
        }

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.BasePrice >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= maxPrice.Value);

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => sortDescending ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice),
            "rating" => sortDescending ? query.OrderByDescending(p => p.AverageRating) : query.OrderBy(p => p.AverageRating),
            "date" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
