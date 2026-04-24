namespace Appilico.Server.Domain.Enums;

/// <summary>
/// Represents the type of a customer address.
/// </summary>
public enum AddressType
{
    /// <summary>Shipping address.</summary>
    Shipping = 0,
    /// <summary>Billing address.</summary>
    Billing = 1,
    /// <summary>Both shipping and billing.</summary>
    Both = 2
}
