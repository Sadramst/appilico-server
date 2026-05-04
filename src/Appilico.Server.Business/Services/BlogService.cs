using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Appilico.Server.Business.DTOs.Blog;
using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.DataAccess.Data;
using Appilico.Server.Domain.Entities;

namespace Appilico.Server.Business.Services;

/// <summary>Blog service implementation.</summary>
public class BlogService : IBlogService
{
    private readonly AppDbContext _db;
    private readonly ILogger<BlogService> _logger;

    public BlogService(AppDbContext db, ILogger<BlogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PagedResult<BlogPostDto>>> GetPostsAsync(string? category, int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = _db.BlogPosts
            .Where(p => !p.IsDeleted && p.IsPublished)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        var total = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ApiResponse<PagedResult<BlogPostDto>>.SuccessResponse(
            PagedResult<BlogPostDto>.Create(posts.Select(p => MapToDto(p)).ToList(), page, pageSize, total));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> GetPostBySlugAsync(string slug)
    {
        var post = await _db.BlogPosts
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted && p.IsPublished);

        if (post == null)
            return ApiResponse<BlogPostDto>.FailResponse("Blog post not found");

        var dto = MapToDto(post);

        // Add up to 2 related posts (same category, different slug)
        dto.RelatedPosts = (await _db.BlogPosts
            .Where(p => p.Category == post.Category && p.Slug != slug && !p.IsDeleted && p.IsPublished)
            .OrderByDescending(p => p.PublishedAt)
            .Take(2)
            .ToListAsync())
            .Select(p => MapToDto(p, includeRelated: false))
            .ToList();

        return ApiResponse<BlogPostDto>.SuccessResponse(dto);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> CreatePostAsync(CreateBlogPostRequest request)
    {
        var slug = GenerateSlug(request.Title);
        var wordCount = request.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var readTime = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));

        var post = new BlogPost
        {
            Title = request.Title,
            Slug = slug,
            Excerpt = request.Excerpt,
            Content = request.Content,
            Category = request.Category,
            Author = request.Author,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            ReadTimeMinutes = readTime,
            ImageUrl = request.ImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            Tags = string.Join(",", request.Tags),
            IsPublished = request.IsPublished
        };

        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Blog post created: {Slug}", slug);
        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post), "Blog post created");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> UpdatePostAsync(Guid id, UpdateBlogPostRequest request)
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (post == null) return ApiResponse<BlogPostDto>.FailResponse("Blog post not found");

        if (request.Title != null)
        {
            post.Title = request.Title;
            post.Slug = GenerateSlug(request.Title);
        }
        if (request.Excerpt != null) post.Excerpt = request.Excerpt;
        if (request.Content != null)
        {
            post.Content = request.Content;
            var wordCount = request.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            post.ReadTimeMinutes = Math.Max(1, (int)Math.Ceiling(wordCount / 200.0));
        }
        if (request.Category != null) post.Category = request.Category;
        if (request.Author != null) post.Author = request.Author;
        if (request.ImageUrl != null) post.ImageUrl = request.ImageUrl;
        if (request.ThumbnailUrl != null) post.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Tags != null) post.Tags = string.Join(",", request.Tags);
        if (request.IsPublished.HasValue)
        {
            post.IsPublished = request.IsPublished.Value;
            if (request.IsPublished.Value && post.PublishedAt == null)
                post.PublishedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post), "Blog post updated");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeletePostAsync(Guid id)
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (post == null) return ApiResponse<bool>.FailResponse("Blog post not found");

        post.IsDeleted = true;
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Blog post deleted");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> PublishPostAsync(Guid id)
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (post == null) return ApiResponse<BlogPostDto>.FailResponse("Blog post not found");

        post.IsPublished = true;
        if (post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post), "Blog post published");
    }

    private static BlogPostDto MapToDto(BlogPost p, bool includeRelated = true) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Excerpt = p.Excerpt,
        Content = p.Content,
        Category = p.Category,
        Author = p.Author,
        PublishedAt = p.PublishedAt,
        ReadTimeMinutes = p.ReadTimeMinutes,
        ImageUrl = p.ImageUrl,
        ThumbnailUrl = p.ThumbnailUrl,
        IsPublished = p.IsPublished,
        Tags = string.IsNullOrWhiteSpace(p.Tags)
            ? new()
            : p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
        RelatedPosts = new()
    };

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
