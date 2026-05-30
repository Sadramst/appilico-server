using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Order;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Order service interface.</summary>
public interface IOrderService
{
    /// <summary>Gets all orders with pagination.</summary>
    Task<ApiResponse<List<OrderDto>>> GetAllAsync(int page = 1, int pageSize = 10);

    /// <summary>Gets an order by ID.</summary>
    Task<ApiResponse<OrderDto>> GetByIdAsync(Guid id);

    /// <summary>Gets orders for a customer.</summary>
    Task<ApiResponse<List<OrderDto>>> GetByCustomerAsync(Guid customerId, int page = 1, int pageSize = 10);

    /// <summary>Creates an order from cart.</summary>
    Task<ApiResponse<OrderDto>> CreateFromCartAsync(Guid customerId, CreateOrderRequest request, string userId);

    /// <summary>Updates order status.</summary>
    Task<ApiResponse<OrderDto>> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, string userId);

    /// <summary>Gets order status history.</summary>
    Task<ApiResponse<List<OrderStatusHistoryDto>>> GetStatusHistoryAsync(Guid orderId);

    /// <summary>Cancels an order.</summary>
    Task<ApiResponse<bool>> CancelAsync(Guid id, string userId);
}
