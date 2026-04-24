namespace Appilico.Server.Domain.Enums;

/// <summary>
/// Represents the status of a sale transaction.
/// </summary>
public enum SaleStatus
{
    /// <summary>Sale is completed.</summary>
    Completed = 0,
    /// <summary>Sale is pending.</summary>
    Pending = 1,
    /// <summary>Sale has been cancelled.</summary>
    Cancelled = 2,
    /// <summary>Sale has been refunded.</summary>
    Refunded = 3
}
