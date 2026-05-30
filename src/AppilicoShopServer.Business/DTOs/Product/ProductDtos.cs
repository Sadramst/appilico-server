using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Business.DTOs.Product;

/// <summary>
/// DTO for product list/summary view.
/// </summary>
public class ProductDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the SKU.</summary>
    public string SKU { get; set; } = string.Empty;
    /// <summary>Gets or sets the barcode.</summary>
    public string? Barcode { get; set; }
    /// <summary>Gets or sets the category ID.</summary>
    public Guid CategoryId { get; set; }
    /// <summary>Gets or sets the category name.</summary>
    public string? CategoryName { get; set; }
    /// <summary>Gets or sets the brand ID.</summary>
    public Guid BrandId { get; set; }
    /// <summary>Gets or sets the brand name.</summary>
    public string? BrandName { get; set; }
    /// <summary>Gets or sets the base price.</summary>
    public decimal BasePrice { get; set; }
    /// <summary>Gets or sets the stock quantity.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets whether the product is active.</summary>
    public bool IsActive { get; set; }
    /// <summary>Gets or sets whether the product is featured.</summary>
    public bool IsFeatured { get; set; }
    /// <summary>Gets or sets the average rating.</summary>
    public decimal AverageRating { get; set; }
    /// <summary>Gets or sets the total reviews.</summary>
    public int TotalReviews { get; set; }
    /// <summary>Gets or sets the primary image URL.</summary>
    public string? PrimaryImageUrl { get; set; }
    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Gets or sets product images.</summary>
    public List<ProductImageDto> Images { get; set; } = new();
    /// <summary>Gets or sets product variants.</summary>
    public List<ProductVariantDto> Variants { get; set; } = new();
}

/// <summary>
/// DTO for creating a product.
/// </summary>
public class CreateProductRequest
{
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the SKU.</summary>
    public string SKU { get; set; } = string.Empty;
    /// <summary>Gets or sets the barcode.</summary>
    public string? Barcode { get; set; }
    /// <summary>Gets or sets the category ID.</summary>
    public Guid CategoryId { get; set; }
    /// <summary>Gets or sets the brand ID.</summary>
    public Guid BrandId { get; set; }
    /// <summary>Gets or sets the base price.</summary>
    public decimal BasePrice { get; set; }
    /// <summary>Gets or sets the cost price.</summary>
    public decimal CostPrice { get; set; }
    /// <summary>Gets or sets the stock quantity.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets the minimum stock level.</summary>
    public int MinStockLevel { get; set; }
    /// <summary>Gets or sets the weight.</summary>
    public decimal? Weight { get; set; }
    /// <summary>Gets or sets the dimensions.</summary>
    public string? Dimensions { get; set; }
    /// <summary>Gets or sets whether the product is featured.</summary>
    public bool IsFeatured { get; set; }
}

/// <summary>
/// DTO for updating a product.
/// </summary>
public class UpdateProductRequest
{
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the barcode.</summary>
    public string? Barcode { get; set; }
    /// <summary>Gets or sets the category ID.</summary>
    public Guid CategoryId { get; set; }
    /// <summary>Gets or sets the brand ID.</summary>
    public Guid BrandId { get; set; }
    /// <summary>Gets or sets the base price.</summary>
    public decimal BasePrice { get; set; }
    /// <summary>Gets or sets the cost price.</summary>
    public decimal CostPrice { get; set; }
    /// <summary>Gets or sets the stock quantity.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets the minimum stock level.</summary>
    public int MinStockLevel { get; set; }
    /// <summary>Gets or sets the weight.</summary>
    public decimal? Weight { get; set; }
    /// <summary>Gets or sets the dimensions.</summary>
    public string? Dimensions { get; set; }
    /// <summary>Gets or sets whether the product is active.</summary>
    public bool IsActive { get; set; }
    /// <summary>Gets or sets whether the product is featured.</summary>
    public bool IsFeatured { get; set; }
}

/// <summary>
/// DTO for product search parameters.
/// </summary>
public class ProductSearchRequest
{
    /// <summary>Gets or sets the search term.</summary>
    public string? SearchTerm { get; set; }
    /// <summary>Gets or sets the category filter.</summary>
    public Guid? CategoryId { get; set; }
    /// <summary>Gets or sets the brand filter.</summary>
    public Guid? BrandId { get; set; }
    /// <summary>Gets or sets the min price filter.</summary>
    public decimal? MinPrice { get; set; }
    /// <summary>Gets or sets the max price filter.</summary>
    public decimal? MaxPrice { get; set; }
    /// <summary>Gets or sets the page number.</summary>
    public int Page { get; set; } = 1;
    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; set; } = 10;
    /// <summary>Gets or sets the sort field.</summary>
    public string? SortBy { get; set; }
    /// <summary>Gets or sets the sort direction.</summary>
    public bool SortDescending { get; set; }
}

/// <summary>
/// DTO for product image.
/// </summary>
public class ProductImageDto
{
    /// <summary>Gets or sets the image ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;
    /// <summary>Gets or sets the alt text.</summary>
    public string? AltText { get; set; }
    /// <summary>Gets or sets the sort order.</summary>
    public int SortOrder { get; set; }
    /// <summary>Gets or sets whether this is the primary image.</summary>
    public bool IsPrimary { get; set; }
}

/// <summary>
/// DTO for product variant.
/// </summary>
public class ProductVariantDto
{
    /// <summary>Gets or sets the variant ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the variant name.</summary>
    public string VariantName { get; set; } = string.Empty;
    /// <summary>Gets or sets the variant SKU.</summary>
    public string SKU { get; set; } = string.Empty;
    /// <summary>Gets or sets the variant price.</summary>
    public decimal Price { get; set; }
    /// <summary>Gets or sets the variant stock.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets the variant attributes.</summary>
    public string? Attributes { get; set; }
}

/// <summary>
/// DTO for creating a product variant.
/// </summary>
public class CreateProductVariantRequest
{
    /// <summary>Gets or sets the variant name.</summary>
    public string VariantName { get; set; } = string.Empty;
    /// <summary>Gets or sets the variant SKU.</summary>
    public string SKU { get; set; } = string.Empty;
    /// <summary>Gets or sets the variant price.</summary>
    public decimal Price { get; set; }
    /// <summary>Gets or sets the variant stock.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets the variant attributes as JSON.</summary>
    public string? Attributes { get; set; }
}
