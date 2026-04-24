using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Cart;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Cart controller.</summary>
[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;

    /// <summary>Initializes CartController.</summary>
    public CartController(ICartService cartService, ICustomerService customerService)
    {
        _cartService = cartService;
        _customerService = customerService;
    }

    /// <summary>Get my cart.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _cartService.GetCartAsync(customer.Data.Id);
        return Ok(result);
    }

    /// <summary>Add item to cart.</summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _cartService.AddItemAsync(customer.Data.Id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update cart item.</summary>
    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _cartService.UpdateItemAsync(customer.Data.Id, cartItemId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Remove cart item.</summary>
    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _cartService.RemoveItemAsync(customer.Data.Id, cartItemId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Clear cart.</summary>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _cartService.ClearCartAsync(customer.Data.Id);
        return Ok(result);
    }
}
