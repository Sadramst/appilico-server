using Appilico.Server.Domain.Common;
using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Domain.Entities;

/// <summary>
/// Represents a discount code/rule.
/// </summary>
public class Discount : BaseAuditableEntity
{
    /// <summary>Gets or sets the discount code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the discount name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the discount description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the discount type.</summary>
    public DiscountType DiscountType { get; set; }

    /// <summary>Gets or sets the discount value.</summary>
    public decimal Value { get; set; }

    /// <summary>Gets or sets the minimum order amount for the discount.</summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>Gets or sets the maximum discount amount cap.</summary>
    public decimal? MaxDiscountAmount { get; set; }

    /// <summary>Gets or sets the discount start date.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the discount end date.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Gets or sets the maximum usage limit.</summary>
    public int? UsageLimit { get; set; }

    /// <summary>Gets or sets the current usage count.</summary>
    public int UsedCount { get; set; }

    /// <summary>Gets or sets whether the discount is active.</summary>
    public bool IsActive { get; set; } = true;
}
