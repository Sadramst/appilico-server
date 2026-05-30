using AppilicoShopServer.Domain.Common;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Tracks order status changes over time.
/// </summary>
public class OrderStatusHistory : BaseAuditableEntity
{
    /// <summary>Gets or sets the order ID (FK).</summary>
    public Guid OrderId { get; set; }

    /// <summary>Gets or sets the previous status.</summary>
    public OrderStatus OldStatus { get; set; }

    /// <summary>Gets or sets the new status.</summary>
    public OrderStatus NewStatus { get; set; }

    /// <summary>Gets or sets notes about the status change.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets when the status was changed.</summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>Navigation property for the order.</summary>
    public virtual Order Order { get; set; } = null!;
}
