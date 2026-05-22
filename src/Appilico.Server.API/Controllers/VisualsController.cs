using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Visual;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Power BI custom visuals controller.</summary>
[Route("api/visuals")]
public class VisualsController : BaseApiController
{
    private readonly IVisualService _visualService;

    public VisualsController(IVisualService visualService) => _visualService = visualService;

    /// <summary>Get paginated visuals with optional filters (category, plan, search).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? requiredPlan,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = new VisualFilterQuery
        {
            Category = category,
            RequiredPlan = requiredPlan,
            Search = search,
            Page = page,
            PageSize = pageSize
        };
        var result = await _visualService.GetPagedAsync(query);
        return Ok(result);
    }

    /// <summary>Get a visual by ID (detail view).</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _visualService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Download a visual file (checks subscription tier).</summary>
    [HttpGet("{id:guid}/download")]
    [Authorize]
    public async Task<IActionResult> Download(Guid id)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var result = await _visualService.DownloadAsync(id, GetUserId(), ipAddress);
        if (!result.Success)
        {
            if (result.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase))
                return StatusCode(StatusCodes.Status503ServiceUnavailable, result);
            return result.Message.Contains("Upgrade") ? Forbid() : NotFound();
        }
        return Ok(result);
    }

    /// <summary>Create a new visual (Admin only).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] UpsertVisualRequest request)
    {
        var result = await _visualService.CreateAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update an existing visual (Admin only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertVisualRequest request)
    {
        var result = await _visualService.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Soft-delete a visual (Admin only — sets IsActive=false).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _visualService.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
