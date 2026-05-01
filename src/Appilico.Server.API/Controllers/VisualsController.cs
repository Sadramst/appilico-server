using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.Interfaces;

namespace Appilico.Server.API.Controllers;

/// <summary>Power BI custom visuals controller.</summary>
[Route("api/visuals")]
[Authorize]
public class VisualsController : BaseApiController
{
    private readonly IVisualService _visualService;

    public VisualsController(IVisualService visualService) => _visualService = visualService;

    /// <summary>Get all active visuals.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _visualService.GetAllAsync();
        return Ok(result);
    }
}
