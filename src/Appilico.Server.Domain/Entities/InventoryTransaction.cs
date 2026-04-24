using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents an inventory stock transaction.
/// </summary>
public class InventoryTransaction : BaseAuditableEntity
{
    /// <summary>Gets or sets the product ID (FK).</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the optional variant ID (FK).</summary>
    public Guid? VariantId { get; set; }

    /// <summary>Gets or sets the transaction type.</summary>
    public InventoryTransactionType TransactionType { get; set; }

    /// <summary>Gets or sets the quantity transacted.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets a reference (e.g. order number, PO number).</summary>
    public string? Reference { get; set; }

    /// <summary>Gets or sets notes about the transaction.</summary>
    public string? Notes { get; set; }

    /// <summary>Navigation property for the product.</summary>
    public virtual Product Product { get; set; } = null!;

    /// <summary>Navigation property for the variant.</summary>
    public virtual ProductVariant? Variant { get; set; }
}
