using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a product variant (e.g. size, color).
/// </summary>
public class ProductVariant : BaseAuditableEntity
{
    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the variant name (e.g. "Large / Red").</summary>
    public string VariantName { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant-specific SKU.</summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant price.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the variant stock quantity.</summary>
    public int StockQuantity { get; set; }

    /// <summary>Gets or sets the variant attributes as JSON.</summary>
    public string? Attributes { get; set; }

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;
}
