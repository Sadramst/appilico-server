using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;
using Appilico.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Appilico.Server.DataAccess.Repositories;

/// <summary>
/// Repository for Customer-specific operations.
/// </summary>
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    /// <summary>Initializes a new instance of the <see cref="CustomerRepository"/> class.</summary>
    public CustomerRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Customer?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    /// <inheritdoc/>
    public async Task<Customer?> GetWithAddressesAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Addresses)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Customer?> GetWithOrdersAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Orders)
                .ThenInclude(o => o.Items)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> SearchAsync(
        string? searchTerm, int page, int pageSize)
    {
        var query = _dbSet.Include(c => c.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(c =>
                c.CustomerCode.ToLower().Contains(term) ||
                c.User.FirstName.ToLower().Contains(term) ||
                c.User.LastName.ToLower().Contains(term) ||
                (c.User.Email != null && c.User.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
