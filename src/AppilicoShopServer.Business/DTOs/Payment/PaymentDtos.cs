using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Business.DTOs.Payment;

/// <summary>DTO for payment.</summary>
public class PaymentDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the order ID.</summary>
    public Guid OrderId { get; set; }
    /// <summary>Gets or sets the amount.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }
    /// <summary>Gets or sets the transaction ID.</summary>
    public string? TransactionId { get; set; }
    /// <summary>Gets or sets the status.</summary>
    public PaymentStatus Status { get; set; }
    /// <summary>Gets or sets when paid.</summary>
    public DateTime? PaidAt { get; set; }
    /// <summary>Provider client secret returned only for client-side confirmation flows.</summary>
    public string? ProviderClientSecret { get; set; }
    /// <summary>Current payment provider status when available.</summary>
    public string? ProviderStatus { get; set; }
}

/// <summary>DTO for creating a payment.</summary>
public class CreatePaymentRequest
{
    /// <summary>Gets or sets the order ID.</summary>
    public Guid OrderId { get; set; }
    /// <summary>Gets or sets the amount.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }
    /// <summary>Gets or sets the transaction ID.</summary>
    public string? TransactionId { get; set; }
}

/// <summary>DTO for creating a refund.</summary>
public class CreateRefundRequest
{
    /// <summary>Gets or sets the amount.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the reason.</summary>
    public string? Reason { get; set; }
}

/// <summary>DTO for refund.</summary>
public class RefundDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the order ID.</summary>
    public Guid OrderId { get; set; }
    /// <summary>Gets or sets the payment ID.</summary>
    public Guid PaymentId { get; set; }
    /// <summary>Gets or sets the amount.</summary>
    public decimal Amount { get; set; }
    /// <summary>Gets or sets the reason.</summary>
    public string? Reason { get; set; }
    /// <summary>Gets or sets the status.</summary>
    public RefundStatus Status { get; set; }
    /// <summary>Gets or sets when refunded.</summary>
    public DateTime? RefundedAt { get; set; }
    /// <summary>External provider refund ID when available.</summary>
    public string? ProviderRefundId { get; set; }
}
