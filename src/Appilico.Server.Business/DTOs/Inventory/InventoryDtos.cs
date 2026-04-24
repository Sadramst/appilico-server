using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.DTOs.Inventory;

/// <summary>DTO for inventory transaction.</summary>
public class InventoryTransactionDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string? ProductName { get; set; }
    /// <summary>Gets or sets the variant ID.</summary>
    public Guid? VariantId { get; set; }
    /// <summary>Gets or sets the transaction type.</summary>
    public InventoryTransactionType TransactionType { get; set; }
    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the reference.</summary>
    public string? Reference { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    public string? Notes { get; set; }
    /// <summary>Gets or sets the created date.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO for adjusting inventory.</summary>
public class AdjustInventoryRequest
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the variant ID.</summary>
    public Guid? VariantId { get; set; }
    /// <summary>Gets or sets the transaction type.</summary>
    public InventoryTransactionType TransactionType { get; set; }
    /// <summary>Gets or sets the quantity.</summary>
    public int Quantity { get; set; }
    /// <summary>Gets or sets the reference.</summary>
    public string? Reference { get; set; }
    /// <summary>Gets or sets the notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>DTO for low stock product.</summary>
public class LowStockProductDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the SKU.</summary>
    public string SKU { get; set; } = string.Empty;
    /// <summary>Gets or sets the current stock.</summary>
    public int StockQuantity { get; set; }
    /// <summary>Gets or sets the min stock level.</summary>
    public int MinStockLevel { get; set; }
}
