using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Appilico.Server.Business.Interfaces;
using Appilico.Server.Domain.Constants;

namespace Appilico.Server.API.Controllers;

/// <summary>Dashboard controller.</summary>
[Authorize(Roles = $"{AppConstants.Roles.Admin},{AppConstants.Roles.Manager}")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    /// <summary>Initializes DashboardController.</summary>
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Get sales summary.</summary>
    [HttpGet("sales-summary")]
    public async Task<IActionResult> GetSalesSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await _dashboardService.GetSalesSummaryAsync(from, to);
        return Ok(result);
    }

    /// <summary>Get top products.</summary>
    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 10, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _dashboardService.GetTopProductsAsync(count, from, to);
        return Ok(result);
    }

    /// <summary>Get revenue chart.</summary>
    [HttpGet("revenue-chart")]
    public async Task<IActionResult> GetRevenueChart([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await _dashboardService.GetRevenueChartAsync(from, to);
        return Ok(result);
    }

    /// <summary>Get customer stats.</summary>
    [HttpGet("customer-stats")]
    public async Task<IActionResult> GetCustomerStats()
    {
        var result = await _dashboardService.GetCustomerStatsAsync();
        return Ok(result);
    }
}
