using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Offer;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Special offers controller.</summary>
public class OffersController : BaseApiController
{
    private readonly ISpecialOfferService _offerService;

    /// <summary>Initializes OffersController.</summary>
    public OffersController(ISpecialOfferService offerService)
    {
        _offerService = offerService;
    }

    /// <summary>Get all offers.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _offerService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get active offers.</summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var result = await _offerService.GetActiveAsync();
        return Ok(result);
    }

    /// <summary>Get offer by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _offerService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create offer.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateSpecialOfferRequest request)
    {
        var result = await _offerService.CreateAsync(request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update offer.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialOfferRequest request)
    {
        var result = await _offerService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete offer.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _offerService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Add products to offer.</summary>
    [HttpPost("{offerId:guid}/products")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> AddProducts(Guid offerId, [FromBody] AddOfferProductsRequest request)
    {
        var result = await _offerService.AddProductsAsync(offerId, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
