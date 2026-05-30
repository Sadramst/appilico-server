using AppilicoShopServer.Domain.Common;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a product review by a customer.
/// </summary>
public class ProductReview : BaseAuditableEntity
{
    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the customer ID (FK).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the rating (1-5).</summary>
    public int Rating { get; set; }

    /// <summary>Gets or sets the review title.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the review comment.</summary>
    public string? Comment { get; set; }

    /// <summary>Gets or sets whether this is a verified purchase review.</summary>
    public bool IsVerifiedPurchase { get; set; }

    /// <summary>Gets or sets whether the review has been approved.</summary>
    public bool IsApproved { get; set; }

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>Navigation property for the customer.</summary>
    public virtual Customer Customer { get; set; } = null!;
}
