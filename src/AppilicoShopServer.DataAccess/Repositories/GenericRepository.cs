using System.Linq.Expressions;
using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Common;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Generic repository implementation for CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type inheriting BaseAuditableEntity.</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseAuditableEntity
{
    /// <summary>The database context.</summary>
    protected readonly AppDbContext _context;

    /// <summary>The DbSet for the entity.</summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>Initializes a new instance of the <see cref="GenericRepository{T}"/> class.</summary>
    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (predicate != null)
            query = query.Where(predicate);

        foreach (var include in includes)
            query = query.Include(include);

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = orderBy(query);
        else
            query = query.OrderByDescending(e => e.CreatedAt);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <inheritdoc/>
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }

    /// <inheritdoc/>
    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    /// <inheritdoc/>
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    /// <inheritdoc/>
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    /// <inheritdoc/>
    public void SoftDelete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    /// <inheritdoc/>
    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }
}
