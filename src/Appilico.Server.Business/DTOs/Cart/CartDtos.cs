namespace Appilico.Server.Business.DTOs.Cart;

/// <summary>DTO for cart.</summary>
public class CartDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the customer ID.</summary>
    public Guid? CustomerId { get; set; }
    /// <summary>Gets or sets the items.</summary>
    public List<CartItemDto> Items { get; set; } = new();
    /// <summary>Gets or sets the total.</summary>
    public decimal Total { get; set; }
}

/// <summary>DTO for cart item.</summary>
public class CartItemDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the image URL.</summary>
    public string? ImageUrl { get; set; }
    /// <summary>Gets or sets the variant ID.</summary>
    public Guid? VariantId { get; set; }
    /// <summary>Gets or sets the variant name.</summary>
    public string? VariantName { get; set; }
    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the unit price.</summary>
    public decimal UnitPrice { get; set; }
    /// <summary>Gets or sets the line total.</summary>
    public decimal LineTotal { get; set; }
}

/// <summary>DTO for adding to cart.</summary>
public class AddToCartRequest
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the variant ID.</summary>
    public Guid? VariantId { get; set; }
    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; } = 1;
}

/// <summary>DTO for updating cart item.</summary>
public class UpdateCartItemRequest
{
    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }
}
