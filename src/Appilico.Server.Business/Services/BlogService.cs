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
    public async Task<ApiResponse<List<BlogPostDto>>> GetPostsAsync(string? category, int page, int pageSize)
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

        var dtos = posts.Select(MapToDto).ToList();
        var pagination = PaginationMeta.Create(page, pageSize, total);
        return ApiResponse<List<BlogPostDto>>.SuccessResponse(dtos, pagination);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> GetPostBySlugAsync(string slug)
    {
        var post = await _db.BlogPosts
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.IsDeleted && p.IsPublished);

        if (post == null)
            return ApiResponse<BlogPostDto>.FailResponse("Blog post not found");

        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post));
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
            PublishedAt = DateTime.UtcNow,
            ReadTimeMinutes = readTime,
            ImageUrl = request.ImageUrl,
            Tags = string.Join(",", request.Tags),
            IsPublished = request.IsPublished
        };

        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Blog post created: {Slug}", slug);
        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post), "Blog post created");
    }

    private static BlogPostDto MapToDto(BlogPost p) => new()
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
        Tags = string.IsNullOrWhiteSpace(p.Tags)
            ? new()
            : p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList()
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
