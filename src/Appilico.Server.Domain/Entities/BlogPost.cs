using Appilico.Server.Domain.Common;

namespace Appilico.Server.Domain.Entities;

/// <summary>Represents a blog post.</summary>
public class BlogPost : BaseAuditableEntity
{
    /// <summary>Gets or sets the post title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL-friendly slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the short excerpt.</summary>
    public string Excerpt { get; set; } = string.Empty;

    /// <summary>Gets or sets the full HTML/Markdown content.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the author name.</summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>Gets or sets the publish date.</summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>Gets or sets the estimated read time in minutes.</summary>
    public int ReadTimeMinutes { get; set; }

    /// <summary>Gets or sets the cover image URL.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Gets or sets comma-separated tags.</summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the post is published.</summary>
    public bool IsPublished { get; set; } = true;
}
