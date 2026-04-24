using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.DTOs.Order;

/// <summary>DTO for order.</summary>
public class OrderDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the order number.</summary>
    public string OrderNumber { get; set; } = string.Empty;
    /// <summary>Gets or sets the customer ID.</summary>
    public Guid CustomerId { get; set; }
    /// <summary>Gets or sets the customer name.</summary>
    public string? CustomerName { get; set; }
    /// <summary>Gets or sets the order status.</summary>
    public OrderStatus OrderStatus { get; set; }
    /// <summary>Gets or sets the subtotal.</summary>
    public decimal SubTotal { get; set; }
    /// <summary>Gets or sets the discount amount.</summary>
    public decimal DiscountAmount { get; set; }
    /// <summary>Gets or sets the tax amount.</summary>
    public decimal TaxAmount { get; set; }
    /// <summary>Gets or sets the shipping amount.</summary>
    public decimal ShippingAmount { get; set; }
    /// <summary>Gets or sets the total amount.</summary>
    public decimal TotalAmount { get; set; }
    /// <summary>Gets or sets the payment status.</summary>
    public PaymentStatus PaymentStatus { get; set; }
    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }
    /// <summary>Gets or sets the order date.</summary>
    public DateTime OrderDate { get; set; }
    /// <summary>Gets or sets the voucher code.</summary>
    public string? VoucherCode { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    public string? Notes { get; set; }
    /// <summary>Gets or sets the items.</summary>
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>DTO for order item.</summary>
public class OrderItemDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the unit price.</summary>
    public decimal UnitPrice { get; set; }
    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the total price.</summary>
    public decimal TotalPrice { get; set; }
    /// <summary>Gets or sets the discount.</summary>
    public decimal Discount { get; set; }
}

/// <summary>DTO for creating an order.</summary>
public class CreateOrderRequest
{
    /// <summary>Gets or sets the shipping address ID.</summary>
    public Guid ShippingAddressId { get; set; }
    /// <summary>Gets or sets the billing address ID.</summary>
    public Guid BillingAddressId { get; set; }
    /// <summary>Gets or sets the payment method.</summary>
    public PaymentMethod PaymentMethod { get; set; }
    /// <summary>Gets or sets the voucher code.</summary>
    public string? VoucherCode { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>DTO for updating order status.</summary>
public class UpdateOrderStatusRequest
{
    /// <summary>Gets or sets the new status.</summary>
    public OrderStatus NewStatus { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>DTO for order status history.</summary>
public class OrderStatusHistoryDto
{
    /// <summary>Gets or sets the old status.</summary>
    public OrderStatus OldStatus { get; set; }
    /// <summary>Gets or sets the new status.</summary>
    public OrderStatus NewStatus { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    public string? Notes { get; set; }
    /// <summary>Gets or sets when it changed.</summary>
    public DateTime ChangedAt { get; set; }
}
