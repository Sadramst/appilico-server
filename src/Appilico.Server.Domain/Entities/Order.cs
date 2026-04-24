using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a customer order.
/// </summary>
public class Order : BaseAuditableEntity
{
    /// <summary>Gets or sets the customer ID (FK).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the auto-generated order number.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the order status.</summary>
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

    /// <summary>Gets or sets the shipping address ID (FK).</summary>
    public Guid ShippingAddressId { get; set; }

    /// <summary>Gets or sets the billing address ID (FK).</summary>
    public Guid BillingAddressId { get; set; }

    /// <summary>Gets or sets the subtotal before discounts and tax.</summary>
    public decimal SubTotal { get; set; }

    /// <summary>Gets or sets the discount amount.</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Gets or sets the tax amount.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Gets or sets the shipping amount.</summary>
    public decimal ShippingAmount { get; set; }

    /// <summary>Gets or sets the total order amount.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets the voucher code used.</summary>
    public string? VoucherCode { get; set; }

    /// <summary>Gets or sets the payment status.</summary>
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Gets or sets order notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets the order date.</summary>
    public DateTime OrderDate { get; set; }

    /// <summary>Navigation property for the customer.</summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>Navigation property for the shipping address.</summary>
    public virtual CustomerAddress ShippingAddress { get; set; } = null!;

    /// <summary>Navigation property for the billing address.</summary>
    public virtual CustomerAddress BillingAddress { get; set; } = null!;

    /// <summary>Navigation property for order items.</summary>
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>Navigation property for order status history.</summary>
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();

    /// <summary>Navigation property for payments.</summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>Navigation property for the sale.</summary>
    public virtual Sale? Sale { get; set; }

    /// <summary>Navigation property for voucher redemptions.</summary>
    public virtual ICollection<VoucherRedemption> VoucherRedemptions { get; set; } = new List<VoucherRedemption>();
}
