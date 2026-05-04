using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Newsletter;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Newsletter subscription controller.</summary>
[Route("api/newsletter")]
public class NewsletterController : BaseApiController
{
    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService) => _newsletterService = newsletterService;

    /// <summary>Subscribe to the newsletter (idempotent).</summary>
    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterSubscribeRequest request)
    {
        var result = await _newsletterService.SubscribeAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Unsubscribe from the newsletter.</summary>
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] NewsletterUnsubscribeRequest request)
    {
        var result = await _newsletterService.UnsubscribeAsync(request);
        return Ok(result);
    }
}
