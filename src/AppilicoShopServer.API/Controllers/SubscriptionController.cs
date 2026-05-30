using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Subscription;
using AppilicoShopServer.Business.Interfaces;

namespace AppilicoShopServer.API.Controllers;

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

    /// <summary>Upgrade the current user's subscription plan.</summary>
    [HttpPost("upgrade")]
    [Authorize]
    public async Task<IActionResult> Upgrade([FromBody] UpgradeSubscriptionRequest request)
    {
        var result = await _subscriptionService.UpgradeAsync(GetUserId(), request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Cancel the current user's subscription.</summary>
    [HttpPost("cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel([FromBody] CancelSubscriptionRequest request)
    {
        var result = await _subscriptionService.CancelAsync(GetUserId(), request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Stripe webhook handler — verifies signature and processes events.</summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        var result = await _subscriptionService.HandleStripeWebhookAsync(payload, signature);
        return result.Success ? Ok() : BadRequest();
    }
}
