using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Product;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Constants;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Products controller.</summary>
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    /// <summary>Initializes ProductsController.</summary>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Search products.</summary>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] ProductSearchRequest request)
    {
        var result = await _productService.SearchProductsAsync(request);
        return Ok(result);
    }

    /// <summary>Get product by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _productService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get product by SKU.</summary>
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku)
    {
        var result = await _productService.GetBySkuAsync(sku);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get featured products.</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 10)
    {
        var result = await _productService.GetFeaturedAsync(count);
        return Ok(result);
    }

    /// <summary>Create product.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _productService.CreateAsync(request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update product.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _productService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete product.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Add variant to product.</summary>
    [HttpPost("{productId:guid}/variants")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> AddVariant(Guid productId, [FromBody] CreateProductVariantRequest request)
    {
        var result = await _productService.AddVariantAsync(productId, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
