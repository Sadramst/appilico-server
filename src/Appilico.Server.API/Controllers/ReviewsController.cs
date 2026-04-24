using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Review;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Reviews controller.</summary>
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;
    private readonly ICustomerService _customerService;

    /// <summary>Initializes ReviewsController.</summary>
    public ReviewsController(IReviewService reviewService, ICustomerService customerService)
    {
        _reviewService = reviewService;
        _customerService = customerService;
    }

    /// <summary>Get reviews for a product.</summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetByProductAsync(productId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get review by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _reviewService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create review.</summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewRequest request)
    {
        var customer = await _customerService.GetByUserIdAsync(GetUserId());
        if (!customer.Success || customer.Data == null) return BadRequest(customer);

        var result = await _reviewService.CreateAsync(customer.Data.Id, request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update review.</summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewRequest request)
    {
        var result = await _reviewService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete review.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _reviewService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Approve review.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _reviewService.ApproveAsync(id, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
