using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Wishlist controller.</summary>
[Authorize]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;
    private readonly ICustomerService _customerService;

    /// <summary>Initializes WishlistController.</summary>
    public WishlistController(IWishlistService wishlistService, ICustomerService customerService)
    {
        _wishlistService = wishlistService;
        _customerService = customerService;
    }

    /// <summary>Get my wishlist.</summary>
    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _wishlistService.GetByCustomerAsync(customer.Data.Id);
        return Ok(result);
    }

    /// <summary>Add to wishlist.</summary>
    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> Add(Guid productId)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _wishlistService.AddAsync(customer.Data.Id, productId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Remove from wishlist.</summary>
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> Remove(Guid productId)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _wishlistService.RemoveAsync(customer.Data.Id, productId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Check if product is in wishlist.</summary>
    [HttpGet("check/{productId:guid}")]
    public async Task<IActionResult> Check(Guid productId)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return NotFound(customer);

        var result = await _wishlistService.IsInWishlistAsync(customer.Data.Id, productId);
        return Ok(result);
    }
}
