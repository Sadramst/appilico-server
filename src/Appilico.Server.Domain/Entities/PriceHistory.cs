using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Tracks price changes for a product over time.
/// </summary>
public class PriceHistory : BaseAuditableEntity
{
    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the previous price.</summary>
    public decimal OldPrice { get; set; }

    /// <summary>Gets or sets the new price.</summary>
    public decimal NewPrice { get; set; }

    /// <summary>Gets or sets when the price was changed.</summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;
}
