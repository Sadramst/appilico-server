using AppilicoShopServer.Domain.Common;
using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Domain.Entities;

/// <summary>
/// Represents a voucher that can be redeemed by customers.
/// </summary>
public class Voucher : BaseAuditableEntity
{
    /// <summary>Gets or sets the voucher code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the voucher description.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the voucher type.</summary>
    public VoucherType VoucherType { get; set; }

    /// <summary>Gets or sets the voucher value.</summary>
    public decimal Value { get; set; }

    /// <summary>Gets or sets the value type (percentage or fixed).</summary>
    public VoucherValueType ValueType { get; set; }

    /// <summary>Gets or sets the minimum order amount.</summary>
    public decimal? MinOrderAmount { get; set; }

    /// <summary>Gets or sets the maximum number of redemptions.</summary>
    public int? MaxRedemptions { get; set; }

    /// <summary>Gets or sets the current redemption count.</summary>
    public int CurrentRedemptions { get; set; }

    /// <summary>Gets or sets the voucher start date.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Gets or sets the voucher expiry date.</summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>Gets or sets whether the voucher is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether the voucher is single use.</summary>
    public bool IsSingleUse { get; set; }

    /// <summary>Navigation property for redemptions.</summary>
    public virtual ICollection<VoucherRedemption> Redemptions { get; set; } = new List<VoucherRedemption>();
}
