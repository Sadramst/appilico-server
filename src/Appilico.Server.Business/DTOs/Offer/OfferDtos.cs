using Appilico.Server.Domain.Enums;

namespace Appilico.Server.Business.DTOs.Offer;

/// <summary>DTO for special offer.</summary>
public class SpecialOfferDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the banner image URL.</summary>
    public string? BannerImageUrl { get; set; }
    /// <summary>Gets or sets the offer type.</summary>
    public OfferType OfferType { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the end date.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Gets or sets whether it's active.</summary>
    public bool IsActive { get; set; }
    /// <summary>Gets or sets the offer products.</summary>
    public List<SpecialOfferProductDto> Products { get; set; } = new();
}

/// <summary>DTO for special offer product.</summary>
public class SpecialOfferProductDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the offer price.</summary>
    public decimal OfferPrice { get; set; }
    /// <summary>Gets or sets the original price.</summary>
    public decimal OriginalPrice { get; set; }
    /// <summary>Gets or sets the max quantity per customer.</summary>
    public int? MaxQuantityPerCustomer { get; set; }
}

/// <summary>DTO for creating a special offer.</summary>
public class CreateSpecialOfferRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the offer type.</summary>
    public OfferType OfferType { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the end date.</summary>
    public DateTime EndDate { get; set; }
}

/// <summary>DTO for updating a special offer.</summary>
public class UpdateSpecialOfferRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the start date.</summary>
    public DateTime StartDate { get; set; }
    /// <summary>Gets or sets the end date.</summary>
    public DateTime EndDate { get; set; }
    /// <summary>Gets or sets whether it's active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>DTO for adding products to offer.</summary>
public class AddOfferProductsRequest
{
    /// <summary>Gets or sets the products.</summary>
    public List<OfferProductItem> Products { get; set; } = new();
}

/// <summary>DTO for a single offer product item.</summary>
public class OfferProductItem
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the offer price.</summary>
    public decimal OfferPrice { get; set; }
    /// <summary>Gets or sets the max quantity per customer.</summary>
    public int? MaxQuantityPerCustomer { get; set; }
}
