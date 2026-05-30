using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>
/// Repository interface for Order-specific operations.
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    /// <summary>Gets an order with all details.</summary>
    Task<Order?> GetWithDetailsAsync(Guid id);

    /// <summary>Gets orders by customer ID.</summary>
    Task<IReadOnlyList<Order>> GetByCustomerAsync(Guid customerId);

    /// <summary>Gets the next order number.</summary>
    Task<string> GenerateOrderNumberAsync();

    /// <summary>Gets order status history.</summary>
    Task<IReadOnlyList<OrderStatusHistory>> GetStatusHistoryAsync(Guid orderId);
}
