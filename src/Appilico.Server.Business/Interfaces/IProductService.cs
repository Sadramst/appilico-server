using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Product;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Product service interface.</summary>
public interface IProductService
{
    /// <summary>Gets all products with pagination and filtering.</summary>
    Task<ApiResponse<List<ProductDto>>> SearchProductsAsync(ProductSearchRequest request);

    /// <summary>Gets a product by ID.</summary>
    Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id);

    /// <summary>Gets a product by SKU.</summary>
    Task<ApiResponse<ProductDto>> GetBySkuAsync(string sku);

    /// <summary>Creates a product.</summary>
    Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request, string userId);

    /// <summary>Updates a product.</summary>
    Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, string userId);

    /// <summary>Deletes a product (soft).</summary>
    Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId);

    /// <summary>Gets featured products.</summary>
    Task<ApiResponse<List<ProductDto>>> GetFeaturedAsync(int count = 10);

    /// <summary>Adds a variant to a product.</summary>
    Task<ApiResponse<ProductVariantDto>> AddVariantAsync(Guid productId, CreateProductVariantRequest request, string userId);
}
