using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Join entity linking a special offer to a product.
/// </summary>
public class SpecialOfferProduct : BaseAuditableEntity
{
    /// <summary>Gets or sets the special offer ID (FK).</summary>
    public Guid SpecialOfferId { get; set; }

    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the offer price for this product.</summary>
    public decimal OfferPrice { get; set; }

    /// <summary>Gets or sets the max quantity per customer.</summary>
    public int? MaxQuantityPerCustomer { get; set; }

    /// <summary>Navigation property for the special offer.</summary>
    public virtual SpecialOffer SpecialOffer { get; set; } = null!;

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;
}
