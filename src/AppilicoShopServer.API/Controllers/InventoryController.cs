using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Inventory;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Constants;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Inventory controller.</summary>
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    /// <summary>Initializes InventoryController.</summary>
    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>Get inventory transactions for a product.</summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetTransactions(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _inventoryService.GetTransactionsAsync(productId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Adjust inventory.</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustInventoryRequest request)
    {
        var result = await _inventoryService.AdjustAsync(request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get low stock products.</summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
    {
        var result = await _inventoryService.GetLowStockAsync(threshold);
        return Ok(result);
    }
}
