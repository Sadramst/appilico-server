namespace Appilico.Server.Domain.Enums;

/// <summary>
/// Represents the status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order is pending.</summary>
    Pending = 0,
    /// <summary>Order has been confirmed.</summary>
    Confirmed = 1,
    /// <summary>Order is being processed.</summary>
    Processing = 2,
    /// <summary>Order has been shipped.</summary>
    Shipped = 3,
    /// <summary>Order has been delivered.</summary>
    Delivered = 4,
    /// <summary>Order has been cancelled.</summary>
    Cancelled = 5,
    /// <summary>Order has been returned.</summary>
    Returned = 6,
    /// <summary>Order has been refunded.</summary>
    Refunded = 7
}
