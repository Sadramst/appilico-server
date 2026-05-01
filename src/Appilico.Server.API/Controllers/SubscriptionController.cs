using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Subscription controller.</summary>
[Route("api/subscription")]
public class SubscriptionController : BaseApiController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService) => _subscriptionService = subscriptionService;

    /// <summary>Get all available subscription plans (public).</summary>
    [HttpGet("plans")]
    public IActionResult GetPlans()
    {
        var result = _subscriptionService.GetPlans();
        return Ok(result);
    }

    /// <summary>Get the current user's subscription (JWT required).</summary>
    [HttpGet("current")]
    [Authorize]
    public async Task<IActionResult> GetCurrent()
    {
        var result = await _subscriptionService.GetCurrentAsync(GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }
}
