using AppilicoShopServer.DataAccess.Data;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AppilicoShopServer.DataAccess.Repositories;

/// <summary>
/// Repository for Order-specific operations.
/// </summary>
public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    /// <summary>Initializes a new instance of the <see cref="OrderRepository"/> class.</summary>
    public OrderRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<Order?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Customer)
                .ThenInclude(c => c.User)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId)
    {
        return await _dbSet
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<string> GenerateOrderNumberAsync()
    {
        var count = await _dbSet.IgnoreQueryFilters().CountAsync();
        return $"ORD-{(count + 1):D6}";
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<OrderStatusHistory>> GetStatusHistoryAsync(Guid orderId)
    {
        return await _context.OrderStatusHistories
            .Where(h => h.OrderId == orderId)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
}
