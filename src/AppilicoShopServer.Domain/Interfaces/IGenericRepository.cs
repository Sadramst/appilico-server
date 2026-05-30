using System.Linq.Expressions;
using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Generic repository interface for CRUD operations.
/// </summary>
/// <typeparam name="T">Entity type inheriting BaseAuditableEntity.</typeparam>
public interface IGenericRepository<T> where T : BaseAuditableEntity
{
    /// <summary>Gets an entity by its ID.</summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>Gets all entities.</summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>Finds entities matching a predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Gets a single entity matching a predicate.</summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Gets a paged list of entities.</summary>
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        params Expression<Func<T, object>>[] includes);

    /// <summary>Checks if any entity matches the predicate.</summary>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Counts entities matching the predicate.</summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>Adds a new entity.</summary>
    Task<T> AddAsync(T entity);

    /// <summary>Adds multiple entities.</summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>Updates an existing entity.</summary>
    void Update(T entity);

    /// <summary>Soft-deletes an entity.</summary>
    void SoftDelete(T entity);

    /// <summary>Gets a queryable for complex queries.</summary>
    IQueryable<T> GetQueryable();
}
