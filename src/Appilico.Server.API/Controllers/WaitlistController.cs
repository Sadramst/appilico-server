using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Waitlist;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Waitlist controller.</summary>
[Route("api/waitlist")]
public class WaitlistController : BaseApiController
{
    private readonly IWaitlistService _waitlistService;

    public WaitlistController(IWaitlistService waitlistService) => _waitlistService = waitlistService;

    /// <summary>Subscribe to the waitlist (public, rate limited 5/min).</summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] WaitlistSubscribeRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _waitlistService.SubscribeAsync(request, ipAddress);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get total waitlist count (public).</summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var result = await _waitlistService.GetCountAsync();
        return Ok(result);
    }

    /// <summary>Get paginated waitlist entries (Admin only).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAdminList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isNotified = null)
    {
        var result = await _waitlistService.GetAdminListAsync(page, pageSize, isNotified);
        return Ok(result);
    }

    /// <summary>Mark a waitlist entry as notified (Admin only).</summary>
    [HttpPut("{id:guid}/notify")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Notify(Guid id)
    {
        var result = await _waitlistService.NotifyAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
