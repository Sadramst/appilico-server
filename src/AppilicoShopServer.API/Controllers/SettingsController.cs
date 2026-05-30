using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppilicoShopServer.Business.DTOs.Settings;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Constants;

namespace AppilicoShopServer.API.Controllers;

/// <summary>Settings controller.</summary>
[Authorize(Roles = AppConstants.Roles.Admin)]
public class SettingsController : BaseApiController
{
    private readonly ISettingsService _settingsService;

    /// <summary>Initializes SettingsController.</summary>
    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>Get all settings.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _settingsService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get settings by group.</summary>
    [HttpGet("group/{group}")]
    public async Task<IActionResult> GetByGroup(string group)
    {
        var result = await _settingsService.GetByGroupAsync(group);
        return Ok(result);
    }

    /// <summary>Get setting by key.</summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var result = await _settingsService.GetByKeyAsync(key);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update settings.</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request)
    {
        var result = await _settingsService.UpdateAsync(request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
