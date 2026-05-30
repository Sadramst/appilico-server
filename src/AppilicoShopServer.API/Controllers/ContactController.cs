using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Contact;
using AppilicoShopServer.Business.Interfaces;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Contact form controller.</summary>
[Route("api/contact")]
public class ContactController : BaseApiController
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService) => _contactService = contactService;

    /// <summary>Submit a contact form message.</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactRequest request)
    {
        var result = await _contactService.SubmitAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
