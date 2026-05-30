using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a product category with self-referencing hierarchy.
/// </summary>
public class Category : BaseAuditableEntity
{
    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the category image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets the Cloudinary public ID for the image.</summary>
    public string? CloudinaryPublicId { get; set; }

    /// <summary>Gets or sets the parent category ID for hierarchy (FK-self).</summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>Gets or sets the display sort order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets whether the category is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property for the parent category.</summary>
    public virtual Category? ParentCategory { get; set; }

    /// <summary>Navigation property for child categories.</summary>
    public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();

    /// <summary>Navigation property for products in this category.</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
