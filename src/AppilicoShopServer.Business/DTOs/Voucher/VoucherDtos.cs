using AppilicoShopServer.Domain.Enums;

namespace AppilicoShopServer.Business.DTOs.Voucher;

/// <summary>DTO for voucher.</summary>
public class VoucherDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the voucher type.</summary>
    public VoucherType VoucherType { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public decimal Value { get; set; }
    /// <summary>Gets or sets the value type.</summary>
    public VoucherValueType ValueType { get; set; }
    /// <summary>Gets or sets the min order amount.</summary>
    public decimal? MinOrderAmount { get; set; }
    /// <summary>Gets or sets the max redemptions.</summary>
    public int? MaxRedemptions { get; set; }
    /// <summary>Gets or sets the current redemptions.</summary>
    public int CurrentRedemptions { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the expiry date.</summary>
    public DateTime ExpiryDate { get; set; }
    /// <summary>Gets or sets whether it's active.</summary>
    public bool IsActive { get; set; }
    /// <summary>Gets or sets whether it's single use.</summary>
    public bool IsSingleUse { get; set; }
}

/// <summary>DTO for creating a voucher.</summary>
public class CreateVoucherRequest
{
    /// <summary>Gets or sets the code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the voucher type.</summary>
    public VoucherType VoucherType { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public decimal Value { get; set; }
    /// <summary>Gets or sets the value type.</summary>
    public VoucherValueType ValueType { get; set; }
    /// <summary>Gets or sets the min order amount.</summary>
    public decimal? MinOrderAmount { get; set; }
    /// <summary>Gets or sets the max redemptions.</summary>
    public int? MaxRedemptions { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the expiry date.</summary>
    public DateTime ExpiryDate { get; set; }
    /// <summary>Gets or sets whether it's single use.</summary>
    public bool IsSingleUse { get; set; }
}

/// <summary>DTO for updating a voucher.</summary>
public class UpdateVoucherRequest
{
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public decimal Value { get; set; }
    /// <summary>Gets or sets the min order amount.</summary>
    public decimal? MinOrderAmount { get; set; }
    /// <summary>Gets or sets the max redemptions.</summary>
    public int? MaxRedemptions { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the expiry date.</summary>
    public DateTime ExpiryDate { get; set; }
    /// <summary>Gets or sets whether it's active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>DTO for redeeming a voucher.</summary>
public class RedeemVoucherRequest
{
    /// <summary>Gets or sets the voucher code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the order ID.</summary>
    public Guid OrderId { get; set; }
}

/// <summary>DTO for validating a voucher.</summary>
public class ValidateVoucherRequest
{
    /// <summary>Gets or sets the code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the order amount.</summary>
    public decimal OrderAmount { get; set; }
}

/// <summary>DTO for voucher validation result.</summary>
public class VoucherValidationResult
{
    /// <summary>Gets or sets whether valid.</summary>
    public bool IsValid { get; set; }
    /// <summary>Gets or sets the discount amount.</summary>
    public decimal DiscountAmount { get; set; }
    /// <summary>Gets or sets the message.</summary>
    public string Message { get; set; } = string.Empty;
}
