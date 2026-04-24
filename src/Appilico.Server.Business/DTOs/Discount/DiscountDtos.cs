using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.DTOs.Discount;

/// <summary>DTO for discount.</summary>
public class DiscountDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the discount type.</summary>
    public DiscountType DiscountType { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public decimal Value { get; set; }
    /// <summary>Gets or sets the min order amount.</summary>
    public decimal? MinOrderAmount { get; set; }
    /// <summary>Gets or sets the max discount amount.</summary>
    public decimal? MaxDiscountAmount { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the end date.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Gets or sets the usage limit.</summary>
    public int? UsageLimit { get; set; }
    /// <summary>Gets or sets the used count.</summary>
    public int UsedCount { get; set; }
    /// <summary>Gets or sets whether it's active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>DTO for creating a discount.</summary>
public class CreateDiscountRequest
{
    /// <summary>Gets or sets the code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the discount type.</summary>
    public DiscountType DiscountType { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public decimal Value { get; set; }
    /// <summary>Gets or sets the min order amount.</summary>
    public decimal? MinOrderAmount { get; set; }
    /// <summary>Gets or sets the max discount amount.</summary>
    public decimal? MaxDiscountAmount { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the end date.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Gets or sets the usage limit.</summary>
    public int? UsageLimit { get; set; }
}

/// <summary>DTO for updating a discount.</summary>
public class UpdateDiscountRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public decimal Value { get; set; }
    /// <summary>Gets or sets the min order amount.</summary>
    public decimal? MinOrderAmount { get; set; }
    /// <summary>Gets or sets the max discount amount.</summary>
    public decimal? MaxDiscountAmount { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the end date.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Gets or sets the usage limit.</summary>
    public int? UsageLimit { get; set; }
    /// <summary>Gets or sets whether it's active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>DTO for validating a discount.</summary>
public class ValidateDiscountRequest
{
    /// <summary>Gets or sets the code.</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Gets or sets the order amount.</summary>
    public decimal OrderAmount { get; set; }
}

/// <summary>DTO for discount validation result.</summary>
public class DiscountValidationResult
{
    /// <summary>Gets or sets whether the discount is valid.</summary>
    public bool IsValid { get; set; }
    /// <summary>Gets or sets the discount amount.</summary>
    public decimal DiscountAmount { get; set; }
    /// <summary>Gets or sets the message.</summary>
    public string Message { get; set; } = string.Empty;
}
