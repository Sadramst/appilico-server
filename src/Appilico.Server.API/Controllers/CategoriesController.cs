using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Category;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Categories controller.</summary>
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    /// <summary>Initializes CategoriesController.</summary>
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Get all categories.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get category tree.</summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        var result = await _categoryService.GetCategoryTreeAsync();
        return Ok(result);
    }

    /// <summary>Get category by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create category.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update category.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete category.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }
}
