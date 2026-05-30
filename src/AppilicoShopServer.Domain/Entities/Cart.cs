using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a shopping cart.
/// </summary>
public class Cart : BaseAuditableEntity
{
    /// <summary>Gets or sets the customer ID (FK).</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the session ID for anonymous carts.</summary>
    public string? SessionId { get; set; }

    /// <summary>Gets or sets whether the cart is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation property for the customer.</summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>Navigation property for cart items.</summary>
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
