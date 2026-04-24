using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a completed sale linked to an order.
/// </summary>
public class Sale : BaseAuditableEntity
{
    /// <summary>Gets or sets the order ID (FK).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the sale date.</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>Gets or sets the total sale amount.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the payment method used.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Gets or sets the transaction reference.</summary>
    public string? TransactionReference { get; set; }

    /// <summary>Gets or sets the sale status.</summary>
    public SaleStatus Status { get; set; }

    /// <summary>Navigation property for the order.</summary>
    public virtual Order Order { get; set; } = null!;
}
