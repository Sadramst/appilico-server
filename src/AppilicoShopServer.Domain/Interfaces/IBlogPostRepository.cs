using AppilicoShopServer.Domain.Entities;

namespace AppilicoShopServer.Domain.Interfaces;

/// <summary>Repository/query abstraction for blog posts.</summary>
public interface IBlogPostRepository : IGenericRepository<BlogPost>
{
    /// <summary>Gets a page of published posts.</summary>
    Task<(IReadOnlyList<BlogPost> Items, int TotalCount)> GetPublishedAsync(string? category, int page, int pageSize);

    /// <summary>Gets a published post by slug.</summary>
    Task<BlogPost?> GetPublishedBySlugAsync(string slug);

    /// <summary>Gets related published posts in the same category.</summary>
    Task<IReadOnlyList<BlogPost>> GetRelatedPublishedAsync(string category, string excludedSlug, int take);

    /// <summary>Gets a non-deleted post for administrative edits.</summary>
    Task<BlogPost?> GetEditableByIdAsync(Guid id);
}