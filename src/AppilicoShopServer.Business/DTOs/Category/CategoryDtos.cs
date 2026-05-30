namespace AppilicoShopServer.Business.DTOs.Category;

/// <summary>DTO for category.</summary>
public class CategoryDto
{
    /// <summary>Gets or sets the ID.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the image URL.</summary>
    public string? ImageUrl { get; set; }
    /// <summary>Gets or sets the parent category ID.</summary>
    public Guid? ParentCategoryId { get; set; }
    /// <summary>Gets or sets the sort order.</summary>
    public int SortOrder { get; set; }
    /// <summary>Gets or sets whether the category is active.</summary>
    public bool IsActive { get; set; }
    /// <summary>Gets or sets subcategories.</summary>
    public List<CategoryDto> SubCategories { get; set; } = new();
}

/// <summary>DTO for creating a category.</summary>
public class CreateCategoryRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the parent category ID.</summary>
    public Guid? ParentCategoryId { get; set; }
    /// <summary>Gets or sets the sort order.</summary>
    public int SortOrder { get; set; }
}

/// <summary>DTO for updating a category.</summary>
public class UpdateCategoryRequest
{
    /// <summary>Gets or sets the name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the description.</summary>
    public string? Description { get; set; }
    /// <summary>Gets or sets the parent category ID.</summary>
    public Guid? ParentCategoryId { get; set; }
    /// <summary>Gets or sets the sort order.</summary>
    public int SortOrder { get; set; }
    /// <summary>Gets or sets whether the category is active.</summary>
    public bool IsActive { get; set; }
}
