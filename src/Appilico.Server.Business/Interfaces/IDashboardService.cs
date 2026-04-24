using Appilico.Server.Business.DTOs.Common;
using Appilico.Server.Business.DTOs.Dashboard;

namespace Appilico.Server.Business.Interfaces;

/// <summary>Dashboard service interface.</summary>
public interface IDashboardService
{
    /// <summary>Gets sales summary.</summary>
    Task<ApiResponse<SalesSummaryDto>> GetSalesSummaryAsync(DateTime? from = null, DateTime? to = null);

    /// <summary>Gets top products.</summary>
    Task<ApiResponse<List<TopProductDto>>> GetTopProductsAsync(int count = 10, DateTime? from = null, DateTime? to = null);

    /// <summary>Gets revenue chart data.</summary>
    Task<ApiResponse<List<RevenueChartDto>>> GetRevenueChartAsync(DateTime from, DateTime to);

    /// <summary>Gets customer statistics.</summary>
    Task<ApiResponse<CustomerStatsDto>> GetCustomerStatsAsync();
}
