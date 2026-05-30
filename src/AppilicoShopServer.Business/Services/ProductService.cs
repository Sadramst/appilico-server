using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Product;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Product service implementation.</summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    /// <summary>Initializes a new instance of ProductService.</summary>
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<ProductDto>>> SearchProductsAsync(ProductSearchRequest request)
    {
        var (items, totalCount) = await _unitOfWork.Products.SearchAsync(
            request.SearchTerm, request.CategoryId, request.BrandId,
            request.MinPrice, request.MaxPrice,
            request.Page, request.PageSize, request.SortBy, request.SortDescending);

        var dtos = _mapper.Map<List<ProductDto>>(items);
        var pagination = PaginationMeta.Create(request.Page, request.PageSize, totalCount);

        return ApiResponse<List<ProductDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.FailResponse("Product not found");

        return ApiResponse<ProductDto>.SuccessResponse(_mapper.Map<ProductDto>(product));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ProductDto>> GetBySkuAsync(string sku)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.SKU == sku);
        if (product == null)
            return ApiResponse<ProductDto>.FailResponse("Product not found");

        return ApiResponse<ProductDto>.SuccessResponse(_mapper.Map<ProductDto>(product));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request, string userId)
    {
        if (await _unitOfWork.Products.AnyAsync(p => p.SKU == request.SKU))
            return ApiResponse<ProductDto>.FailResponse("A product with this SKU already exists");

        var product = _mapper.Map<Product>(request);
        product.CreatedBy = userId;

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} created by {UserId}", product.Id, userId);

        var created = await _unitOfWork.Products.GetWithDetailsAsync(product.Id);
        return ApiResponse<ProductDto>.SuccessResponse(_mapper.Map<ProductDto>(created), "Product created successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.FailResponse("Product not found");

        _mapper.Map(request, product);
        product.UpdatedBy = userId;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} updated by {UserId}", id, userId);

        var updated = await _unitOfWork.Products.GetWithDetailsAsync(id);
        return ApiResponse<ProductDto>.SuccessResponse(_mapper.Map<ProductDto>(updated), "Product updated successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<bool>.FailResponse("Product not found");

        product.UpdatedBy = userId;
        _unitOfWork.Products.SoftDelete(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted by {UserId}", id, userId);
        return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<ProductDto>>> GetFeaturedAsync(int count = 10)
    {
        var products = await _unitOfWork.Products.GetFeaturedAsync(count);
        return ApiResponse<List<ProductDto>>.SuccessResponse(_mapper.Map<List<ProductDto>>(products));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<ProductVariantDto>> AddVariantAsync(Guid productId, CreateProductVariantRequest request, string userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null)
            return ApiResponse<ProductVariantDto>.FailResponse("Product not found");

        var variant = _mapper.Map<ProductVariant>(request);
        variant.ProductId = productId;
        variant.CreatedBy = userId;

        product.Variants ??= new List<ProductVariant>();
        product.Variants.Add(variant);

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Variant added to product {ProductId} by {UserId}", productId, userId);
        return ApiResponse<ProductVariantDto>.SuccessResponse(_mapper.Map<ProductVariantDto>(variant), "Variant added successfully");
    }
}
