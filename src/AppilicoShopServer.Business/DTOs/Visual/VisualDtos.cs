namespace AppilicoShopServer.Business.DTOs.Visual;

/// <summary>DTO for a Power BI custom visual (list view).</summary>
public class VisualDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string RequiredPlan { get; set; } = string.Empty;
    public int DownloadCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>DTO for a visual detail view.</summary>
public class VisualDetailDto : VisualDto
{
    public string? FullDescription { get; set; }
    public List<string> PreviewImageUrls { get; set; } = new();
    public string? TechnicalSpecs { get; set; }
    public string? DataRequirements { get; set; }
    public string? PowerBIVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Request DTO for creating or updating a visual.</summary>
public class UpsertVisualRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? FullDescription { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public List<string> PreviewImageUrls { get; set; } = new();
    public string RequiredPlan { get; set; } = "Free";
    public List<string> Tags { get; set; } = new();
    public string? TechnicalSpecs { get; set; }
    public string? DataRequirements { get; set; }
    public string? PowerBIVersion { get; set; }
    public string? DemoUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Response DTO for a visual download.</summary>
public class VisualDownloadResponseDto
{
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>Query parameters for filtering visuals.</summary>
public class VisualFilterQuery
{
    public string? Category { get; set; }
    public string? RequiredPlan { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
