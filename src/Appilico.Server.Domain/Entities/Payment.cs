using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a payment for an order.
/// </summary>
public class Payment : BaseAuditableEntity
{
    /// <summary>Gets or sets the order ID (FK).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the payment amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Gets or sets the transaction ID from payment gateway.</summary>
    public string? TransactionId { get; set; }

    /// <summary>Gets or sets the payment status.</summary>
    public PaymentStatus Status { get; set; }

    /// <summary>Gets or sets when the payment was made.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Navigation property for the order.</summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>Navigation property for refunds.</summary>
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
