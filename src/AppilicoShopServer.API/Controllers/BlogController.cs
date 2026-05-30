using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Blog;
using AppilicoShopServer.Business.Interfaces;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Blog posts controller.</summary>
[Route("api/blog")]
public class BlogController : BaseApiController
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService) => _blogService = blogService;

    /// <summary>Get paginated blog posts, optionally filtered by category.</summary>
    [HttpGet("posts")]
    public async Task<IActionResult> GetPosts(
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9)
    {
        var result = await _blogService.GetPostsAsync(category, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get a single blog post by slug (with related posts).</summary>
    [HttpGet("posts/{slug}")]
    public async Task<IActionResult> GetPost(string slug)
    {
        var result = await _blogService.GetPostBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new blog post (Admin only).</summary>
    [HttpPost("posts")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreatePost([FromBody] CreateBlogPostRequest request)
    {
        var result = await _blogService.CreatePostAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update an existing blog post (Admin only).</summary>
    [HttpPut("posts/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdateBlogPostRequest request)
    {
        var result = await _blogService.UpdatePostAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Soft-delete a blog post (Admin only).</summary>
    [HttpDelete("posts/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var result = await _blogService.DeletePostAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Publish a draft blog post (Admin only).</summary>
    [HttpPost("posts/{id:guid}/publish")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> PublishPost(Guid id)
    {
        var result = await _blogService.PublishPostAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
