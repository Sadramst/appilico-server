using Appilico.Server.Business.DTOs.Blog;
using Appilico.Server.Business.DTOs.Common;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Blog service interface.</summary>
public interface IBlogService
{
    Task<ApiResponse<PagedResult<BlogPostDto>>> GetPostsAsync(string? category, int page, int pageSize);
    Task<ApiResponse<BlogPostDto>> GetPostBySlugAsync(string slug);
    Task<ApiResponse<BlogPostDto>> CreatePostAsync(CreateBlogPostRequest request);
    Task<ApiResponse<BlogPostDto>> UpdatePostAsync(Guid id, UpdateBlogPostRequest request);
    Task<ApiResponse<bool>> DeletePostAsync(Guid id);
    Task<ApiResponse<BlogPostDto>> PublishPostAsync(Guid id);
}
