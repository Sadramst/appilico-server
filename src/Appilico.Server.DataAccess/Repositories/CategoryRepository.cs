using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for Category-specific operations.
/// </summary>
public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    /// <summary>Initializes a new instance of the <see cref="CategoryRepository"/> class.</summary>
    public CategoryRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetCategoryTreeAsync()
    {
        return await _dbSet
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Category?> GetWithSubCategoriesAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
