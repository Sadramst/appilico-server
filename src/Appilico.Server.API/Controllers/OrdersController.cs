using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Order;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Orders controller.</summary>
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;

    /// <summary>Initializes OrdersController.</summary>
    public OrdersController(IOrderService orderService, ICustomerService customerService)
    {
        _orderService = orderService;
        _customerService = customerService;
    }

    /// <summary>Get all orders (admin).</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _orderService.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get order by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get my orders.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null)
            return NotFound(customer);

        var result = await _orderService.GetByCustomerAsync(customer.Data.Id, page, pageSize);
        return Ok(result);
    }

    /// <summary>Create order from cart.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null)
            return BadRequest(customer);

        var result = await _orderService.CreateFromCartAsync(customer.Data.Id, request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update order status.</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateStatusAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get order status history.</summary>
    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetStatusHistory(Guid id)
    {
        var result = await _orderService.GetStatusHistoryAsync(id);
        return Ok(result);
    }

    /// <summary>Cancel order.</summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _orderService.CancelAsync(id, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
