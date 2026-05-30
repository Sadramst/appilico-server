using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a product in the catalog.
/// </summary>
public class Product : BaseAuditableEntity
{
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the product description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the stock keeping unit.</summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>Gets or sets the barcode.</summary>
    public string? Barcode { get; set; }

    /// <summary>Gets or sets the category ID (FK).</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Gets or sets the brand ID (FK).</summary>
    public Guid BrandId { get; set; }

    /// <summary>Gets or sets the base retail price.</summary>
    public decimal BasePrice { get; set; }

    /// <summary>Gets or sets the cost price.</summary>
    public decimal CostPrice { get; set; }

    /// <summary>Gets or sets the current stock quantity.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Gets or sets the minimum stock level before reorder alert.</summary>
    public int MinStockLevel { get; set; }

    /// <summary>Gets or sets the product weight in kg.</summary>
    public decimal? Weight { get; set; }

    /// <summary>Gets or sets the product dimensions (e.g. "10x20x5 cm").</summary>
    public string? Dimensions { get; set; }

    /// <summary>Gets or sets whether the product is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether the product is featured.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Gets or sets the average customer rating.</summary>
    public decimal AverageRating { get; set; }

    /// <summary>Gets or sets the total number of reviews.</summary>
    public int TotalReviews { get; set; }

    /// <summary>Navigation property for the category.</summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>Navigation property for the brand.</summary>
    public virtual Brand Brand { get; set; } = null!;

    /// <summary>Navigation property for product images.</summary>
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    /// <summary>Navigation property for product variants.</summary>
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    /// <summary>Navigation property for price history.</summary>
    public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();

    /// <summary>Navigation property for reviews.</summary>
    public virtual ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();

    /// <summary>Navigation property for wishlists.</summary>
    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

    /// <summary>Navigation property for inventory transactions.</summary>
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    /// <summary>Navigation property for special offer products.</summary>
    public virtual ICollection<SpecialOfferProduct> SpecialOfferProducts { get; set; } = new List<SpecialOfferProduct>();
}
