using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a product on a customer's wishlist.
/// </summary>
public class Wishlist : BaseAuditableEntity
{
    /// <summary>Gets or sets the customer ID (FK).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets when the item was added to the wishlist.</summary>
    public DateTime AddedAt { get; set; }

    /// <summary>Navigation property for the customer.</summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;
}
