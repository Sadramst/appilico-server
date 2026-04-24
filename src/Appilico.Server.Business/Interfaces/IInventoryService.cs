using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Inventory;

namespace Appilico.Server.Business.Interfaces;

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
