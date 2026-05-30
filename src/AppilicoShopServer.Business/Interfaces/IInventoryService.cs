using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Inventory;

namespace AppilicoShopServer.Business.Interfaces;

/// <summary>Inventory service interface.</summary>
public interface IInventoryService
{
    /// <summary>Gets inventory transactions for a product.</summary>
    Task<ApiResponse<List<InventoryTransactionDto>>> GetTransactionsAsync(Guid productId, int page = 1, int pageSize = 20);

    /// <summary>Adjusts inventory.</summary>
    Task<ApiResponse<InventoryTransactionDto>> AdjustAsync(AdjustInventoryRequest request, string userId);

    /// <summary>Gets low stock products.</summary>
    Task<ApiResponse<List<LowStockProductDto>>> GetLowStockAsync(int threshold = 10);
}
