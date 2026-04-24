using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.DTOs.Payment;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Payments controller.</summary>
[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    /// <summary>Initializes PaymentsController.</summary>
    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Get payments for an order.</summary>
    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var result = await _paymentService.GetByOrderAsync(orderId);
        return Ok(result);
    }

    /// <summary>Get payment by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _paymentService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Process payment.</summary>
    [HttpPost]
    public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentRequest request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Create refund.</summary>
    [HttpPost("{paymentId:guid}/refunds")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> CreateRefund(Guid paymentId, [FromBody] CreateRefundRequest request)
    {
        var result = await _paymentService.CreateRefundAsync(paymentId, request, GetUserId());
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get refunds for an order.</summary>
    [HttpGet("order/{orderId:guid}/refunds")]
    [Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
    public async Task<IActionResult> GetRefundsByOrder(Guid orderId)
    {
        var result = await _paymentService.GetRefundsByOrderAsync(orderId);
        return Ok(result);
    }
}
