namespace Appilico.Server.Domain.Enums;

/// <summary>
/// Represents the type of inventory transaction.
/// </summary>
public enum InventoryTransactionType
{
    /// <summary>Stock received / added.</summary>
    StockIn = 0,
    /// <summary>Stock sold / removed.</summary>
    StockOut = 1,
    /// <summary>Manual stock adjustment.</summary>
    Adjustment = 2,
    /// <summary>Stock returned by customer.</summary>
    Return = 3
}
