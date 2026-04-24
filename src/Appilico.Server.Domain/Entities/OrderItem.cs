using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents an item in an order.
/// </summary>
public class OrderItem : BaseAuditableEntity
{
    /// <summary>Gets or sets the order ID (FK).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the optional variant ID (FK).</summary>
    public Guid? VariantId { get; set; }

    /// <summary>Gets or sets the product name at time of order (denormalized).</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit price at time of order.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets the quantity ordered.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the total price for this line item.</summary>
    public decimal TotalPrice { get; set; }

    /// <summary>Gets or sets the line-level discount.</summary>
    public decimal Discount { get; set; }

    /// <summary>Navigation property for the order.</summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>Navigation property for the variant.</summary>
    public virtual ProductVariant? Variant { get; set; }
}
