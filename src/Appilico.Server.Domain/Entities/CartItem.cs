using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents an item in a shopping cart.
/// </summary>
public class CartItem : BaseAuditableEntity
{
    /// <summary>Gets or sets the cart ID (FK).</summary>
    public Guid CartId { get; set; }

    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the optional variant ID (FK).</summary>
    public Guid? VariantId { get; set; }

    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the unit price at time of adding.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Navigation property for the cart.</summary>
    public virtual Cart Cart { get; set; } = null!;

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>Navigation property for the variant.</summary>
    public virtual ProductVariant? Variant { get; set; }
}
