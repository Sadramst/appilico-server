using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a product brand.
/// </summary>
public class Brand : BaseAuditableEntity
{
    /// <summary>Gets or sets the brand name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the brand description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the brand logo URL.</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Gets or sets whether the brand is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property for products.</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
