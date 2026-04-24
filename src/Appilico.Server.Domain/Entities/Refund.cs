using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a refund against a payment.
/// </summary>
public class Refund : BaseAuditableEntity
{
    /// <summary>Gets or sets the order ID (FK).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the payment ID (FK).</summary>
    public Guid PaymentId { get; set; }

    /// <summary>Gets or sets the refund amount.</summary>
    public decimal Amount { get; set; }

    /// <summary>Gets or sets the reason for the refund.</summary>
    public string? Reason { get; set; }

    /// <summary>Gets or sets the refund status.</summary>
    public RefundStatus Status { get; set; }

    /// <summary>Gets or sets when the refund was processed.</summary>
    public DateTime? RefundedAt { get; set; }

    /// <summary>Navigation property for the order.</summary>
    public virtual Order Order { get; set; } = null!;

    /// <summary>Navigation property for the payment.</summary>
    public virtual Payment Payment { get; set; } = null!;
}
