using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Blog;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Entities;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Blog service implementation.</summary>
public class BlogService : IBlogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlogService> _logger;

    public BlogService(IUnitOfWork unitOfWork, ILogger<BlogService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<PagedResult<BlogPostDto>>> GetPostsAsync(string? category, int page, int pageSize)
    {
        var normalized = PaginationRequest.Normalize(page, pageSize);
        var (posts, total) = await _unitOfWork.BlogPosts.GetPublishedAsync(category, normalized.Page, normalized.PageSize);

        return ApiResponse<PagedResult<BlogPostDto>>.SuccessResponse(
            PagedResult<BlogPostDto>.Create(posts.Select(post => MapToDto(post)).ToList(), normalized.Page, normalized.PageSize, total));
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> GetPostBySlugAsync(string slug)
    {
        var post = await _unitOfWork.BlogPosts.GetPublishedBySlugAsync(slug);

        if (post == null)
            return ApiResponse<BlogPostDto>.FailResponse("Blog post not found");

        var dto = MapToDto(post);

        dto.RelatedPosts = (await _unitOfWork.BlogPosts.GetRelatedPublishedAsync(post.Category, slug, 2))
            .Select(relatedPost => MapToDto(relatedPost, includeRelated: false))
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

        await _unitOfWork.BlogPosts.AddAsync(post);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Blog post created: {Slug}", slug);
        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post), "Blog post created");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> UpdatePostAsync(Guid id, UpdateBlogPostRequest request)
    {
        var post = await _unitOfWork.BlogPosts.GetEditableByIdAsync(id);
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

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<BlogPostDto>.SuccessResponse(MapToDto(post), "Blog post updated");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<bool>> DeletePostAsync(Guid id)
    {
        var post = await _unitOfWork.BlogPosts.GetEditableByIdAsync(id);
        if (post == null) return ApiResponse<bool>.FailResponse("Blog post not found");

        post.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Blog post deleted");
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<BlogPostDto>> PublishPostAsync(Guid id)
    {
        var post = await _unitOfWork.BlogPosts.GetEditableByIdAsync(id);
        if (post == null) return ApiResponse<BlogPostDto>.FailResponse("Blog post not found");

        post.IsPublished = true;
        if (post.PublishedAt == null)
            post.PublishedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
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
