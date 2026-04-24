using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Voucher;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Vouchers controller.</summary>
public class VouchersController : BaseApiController
{
    private readonly IVoucherService _voucherService;

    /// <summary>Initializes VouchersController.</summary>
    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    /// <summary>Get all vouchers.</summary>
    [HttpGet]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _voucherService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Get voucher by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _voucherService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create voucher.</summary>
    [HttpPost]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Create([FromBody] CreateVoucherRequest request)
    {
        var result = await _voucherService.CreateAsync(request, GetUserId());
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update voucher.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVoucherRequest request)
    {
        var result = await _voucherService.UpdateAsync(id, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete voucher.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _voucherService.DeleteAsync(id, GetUserId());
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Validate voucher code.</summary>
    [HttpPost("validate")]
    [Authorize]
    public async Task<IActionResult> Validate([FromBody] ValidateVoucherRequest request)
    {
        var result = await _voucherService.ValidateAsync(request);
        return Ok(result);
    }
}
