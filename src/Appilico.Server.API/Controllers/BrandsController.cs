using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Brand;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Brands controller.</summary>
public class BrandsController : BaseApiController
{
    private readonly IBrandService _brandService;

    /// <summary>Initializes BrandsController.</summary>
    public BrandsController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    /// <summary>Get all brands.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _brandService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get brand by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _brandService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create brand.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateBrandRequest request)
    {
        var result = await _brandService.CreateAsync(request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update brand.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandRequest request)
    {
        var result = await _brandService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete brand.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _brandService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }
}
