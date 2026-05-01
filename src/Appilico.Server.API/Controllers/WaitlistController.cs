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

    /// <summary>Subscribe to the waitlist.</summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] WaitlistSubscribeRequest request)
    {
        var result = await _waitlistService.SubscribeAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get total waitlist count (public).</summary>
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var result = await _waitlistService.GetCountAsync();
        return Ok(result);
    }
}
