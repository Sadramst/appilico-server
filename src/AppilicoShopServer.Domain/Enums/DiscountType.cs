namespace AppilicoShopServer.Domain.Enums;

/// <summary>
/// Represents the type of a discount.
/// </summary>
public enum DiscountType
{
    /// <summary>Percentage-based discount.</summary>
    Percentage = 0,
    /// <summary>Fixed amount discount.</summary>
    Fixed = 1,
    /// <summary>Buy X Get Y free discount.</summary>
    BuyXGetY = 2
}
