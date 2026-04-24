using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Customer;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Customers controller.</summary>
[Authorize]
public class CustomersController : BaseApiController
{
    private readonly ICustomerService _customerService;

    /// <summary>Initializes CustomersController.</summary>
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>Get all customers.</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _customerService.GetAllAsync(page, pageSize, search);
        return Ok(result);
    }

    /// <summary>Get customer by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get my profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _customerService.GetByUserIdAsync(GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update customer.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await _customerService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get customer loyalty info.</summary>
    [HttpGet("{id:guid}/loyalty")]
    public async Task<IActionResult> GetLoyalty(Guid id)
    {
        var result = await _customerService.GetLoyaltyAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Add loyalty points.</summary>
    [HttpPost("{id:guid}/loyalty/points")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> AddLoyaltyPoints(Guid id, [FromQuery] int points)
    {
        var result = await _customerService.AddLoyaltyPointsAsync(id, points);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
