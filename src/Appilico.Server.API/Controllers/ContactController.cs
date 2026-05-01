using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Contact;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

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
