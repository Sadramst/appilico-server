using AutoMapper;
using Microsoft.Extensions.Logging;
using AppilicoShopServer.Business.DTOs.Common;
using AppilicoShopServer.Business.DTOs.Dashboard;
using AppilicoShopServer.Business.Interfaces;
using AppilicoShopServer.Domain.Enums;
using AppilicoShopServer.Domain.Interfaces;

namespace AppilicoShopServer.Business.Services;

/// <summary>Dashboard service implementation.</summary>
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DashboardService> _logger;

    /// <summary>Initializes a new instance of DashboardService.</summary>
    public DashboardService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<SalesSummaryDto>> GetSalesSummaryAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var orders = await _unitOfWork.Orders.FindAsync(o =>
            o.OrderDate >= fromDate && o.OrderDate <= toDate &&
            o.OrderStatus != OrderStatus.Cancelled);

        var totalCustomers = await _unitOfWork.Customers.CountAsync();

        var summary = new SalesSummaryDto
        {
            TotalRevenue = orders.Sum(o => o.TotalAmount),
            TotalOrders = orders.Count,
            AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
            TotalCustomers = totalCustomers
        };

        return ApiResponse<SalesSummaryDto>.SuccessResponse(summary);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<TopProductDto>>> GetTopProductsAsync(int count = 10, DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.UtcNow.AddMonths(-1);
        var toDate = to ?? DateTime.UtcNow;

        var orders = await _unitOfWork.Orders.FindAsync(o =>
            o.OrderDate >= fromDate && o.OrderDate <= toDate &&
            o.OrderStatus != OrderStatus.Cancelled);

        // This would ideally be a database query - simplified here
        var topProducts = new List<TopProductDto>();

        return ApiResponse<List<TopProductDto>>.SuccessResponse(topProducts);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<List<RevenueChartDto>>> GetRevenueChartAsync(DateTime from, DateTime to)
    {
        var orders = await _unitOfWork.Orders.FindAsync(o =>
            o.OrderDate >= from && o.OrderDate <= to &&
            o.OrderStatus != OrderStatus.Cancelled);

        var chartData = orders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new RevenueChartDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(c => c.Date)
            .ToList();

        return ApiResponse<List<RevenueChartDto>>.SuccessResponse(chartData);
    }

    /// <inheritdoc/>
    public async Task<ApiResponse<CustomerStatsDto>> GetCustomerStatsAsync()
    {
        var totalCustomers = await _unitOfWork.Customers.CountAsync();
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newThisMonth = await _unitOfWork.Customers.CountAsync(c => c.JoinDate >= startOfMonth);

        var stats = new CustomerStatsDto
        {
            TotalCustomers = totalCustomers,
            NewCustomersThisMonth = newThisMonth,
            ActiveCustomers = totalCustomers // Simplified
        };

        return ApiResponse<CustomerStatsDto>.SuccessResponse(stats);
    }
}
