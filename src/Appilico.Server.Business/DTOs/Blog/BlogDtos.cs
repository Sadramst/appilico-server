namespace Appilico.Server.Business.DTOs.Blog;

/// <summary>Blog post DTO returned to clients.</summary>
public class BlogPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public int ReadTimeMinutes { get; set; }
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>Request DTO for creating a blog post.</summary>
public class CreateBlogPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsPublished { get; set; } = true;
}
