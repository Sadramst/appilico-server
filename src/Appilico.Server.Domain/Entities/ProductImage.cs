using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents an image associated with a product.
/// </summary>
public class ProductImage : BaseAuditableEntity
{
    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the image URL.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the alt text for the image.</summary>
    public string? AltText { get; set; }

    /// <summary>Gets or sets the display sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets whether this is the primary image.</summary>
    public bool IsPrimary { get; set; }

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;
}
