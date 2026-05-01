using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Blog;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

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
        [FromQuery] int pageSize = 10)
    {
        var result = await _blogService.GetPostsAsync(category, page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get a single blog post by slug.</summary>
    [HttpGet("posts/{slug}")]
    public async Task<IActionResult> GetPost(string slug)
    {
        var result = await _blogService.GetPostBySlugAsync(slug);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new blog post (Admin only).</summary>
    [HttpPost("posts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePost([FromBody] CreateBlogPostRequest request)
    {
        var result = await _blogService.CreatePostAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
