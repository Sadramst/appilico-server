using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a voucher redemption record.
/// </summary>
public class VoucherRedemption : BaseAuditableEntity
{
    /// <summary>Gets or sets the voucher ID (FK).</summary>
    public Guid VoucherId { get; set; }

    /// <summary>Gets or sets the customer ID (FK).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the order ID (FK).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets when the voucher was redeemed.</summary>
    public DateTime RedeemedAt { get; set; }

    /// <summary>Gets or sets the amount discounted.</summary>
    public decimal AmountDiscounted { get; set; }

    /// <summary>Navigation property for the voucher.</summary>
    public virtual Voucher Voucher { get; set; } = null!;

    /// <summary>Navigation property for the customer.</summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>Navigation property for the order.</summary>
    public virtual Order Order { get; set; } = null!;
}
