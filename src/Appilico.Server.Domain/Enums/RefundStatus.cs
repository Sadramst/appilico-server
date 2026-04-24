namespace Appilico.Server.Domain.Enums;

/// <summary>
/// Represents the status of a refund.
/// </summary>
public enum RefundStatus
{
    /// <summary>Refund is pending.</summary>
    Pending = 0,
    /// <summary>Refund has been approved.</summary>
    Approved = 1,
    /// <summary>Refund has been processed.</summary>
    Processed = 2,
    /// <summary>Refund has been rejected.</summary>
    Rejected = 3
}
