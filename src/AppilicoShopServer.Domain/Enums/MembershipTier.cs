namespace AppilicoShopServer.Domain.Enums;

/// <summary>
/// Represents the membership tier of a customer.
/// </summary>
public enum MembershipTier
{
    /// <summary>Bronze tier — default for new customers.</summary>
    Bronze = 0,
    /// <summary>Silver tier.</summary>
    Silver = 1,
    /// <summary>Gold tier.</summary>
    Gold = 2,
    /// <summary>Platinum tier.</summary>
    Platinum = 3
}
