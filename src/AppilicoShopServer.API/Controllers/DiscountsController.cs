using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Discount;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Constants;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Discounts controller.</summary>
public class DiscountsController : BaseApiController
{
    private readonly IDiscountService _discountService;

    /// <summary>Initializes DiscountsController.</summary>
    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    /// <summary>Get all discounts.</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _discountService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get active discounts.</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _discountService.GetActiveAsync();
        return Ok(result);
    }

    /// <summary>Get discount by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _discountService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create discount.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRequest request)
    {
        var result = await _discountService.CreateAsync(request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update discount.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountRequest request)
    {
        var result = await _discountService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete discount.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _discountService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Validate discount code.</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateDiscountRequest request)
    {
        var result = await _discountService.ValidateAsync(request);
        return Ok(result);
    }
}
