namespace AppilicoShopServer.Business.DTOs.Brand;

/// <summary>DTO for brand.</summary>
public class BrandDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the logo URL.</summary>
    public string? LogoUrl { get; set; }
    /// <summary>Gets or sets whether the brand is active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>DTO for creating a brand.</summary>
public class CreateBrandRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the logo URL.</summary>
    public string? LogoUrl { get; set; }
}

/// <summary>DTO for updating a brand.</summary>
public class UpdateBrandRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the logo URL.</summary>
    public string? LogoUrl { get; set; }
    /// <summary>Gets or sets whether the brand is active.</summary>
    public bool IsActive { get; set; }
}
