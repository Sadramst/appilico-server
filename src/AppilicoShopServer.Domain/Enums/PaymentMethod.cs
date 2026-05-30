namespace AppilicoShopServer.Domain.Enums;

/// <summary>
/// Represents the payment method used.
/// </summary>
public enum PaymentMethod
{
    /// <summary>Credit card payment.</summary>
    CreditCard = 0,
    /// <summary>Debit card payment.</summary>
    DebitCard = 1,
    /// <summary>PayPal payment.</summary>
    PayPal = 2,
    /// <summary>Bank transfer payment.</summary>
    BankTransfer = 3,
    /// <summary>Cash on delivery.</summary>
    CashOnDelivery = 4
}
