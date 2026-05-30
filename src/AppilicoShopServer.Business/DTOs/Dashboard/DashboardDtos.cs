namespace AppilicoShopServer.Business.DTOs.Dashboard;

/// <summary>DTO for sales summary.</summary>
public class SalesSummaryDto
{
    /// <summary>Gets or sets the total revenue.</summary>
    public decimal TotalRevenue { get; set; }
    /// <summary>Gets or sets the total orders.</summary>
    public int TotalOrders { get; set; }
    /// <summary>Gets or sets the average order value.</summary>
    public decimal AverageOrderValue { get; set; }
    /// <summary>Gets or sets total customers.</summary>
    public int TotalCustomers { get; set; }
}

/// <summary>DTO for top product.</summary>
public class TopProductDto
{
    /// <summary>Gets or sets the product ID.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the product name.</summary>
    public string ProductName { get; set; } = string.Empty;
    /// <summary>Gets or sets the total sold.</summary>
    public int TotalSold { get; set; }
    /// <summary>Gets or sets the total revenue.</summary>
    public decimal TotalRevenue { get; set; }
}

/// <summary>DTO for revenue chart data point.</summary>
public class RevenueChartDto
{
    /// <summary>Gets or sets the date.</summary>
    public DateTime Date { get; set; }
    /// <summary>Gets or sets the revenue.</summary>
    public decimal Revenue { get; set; }
    /// <summary>Gets or sets the order count.</summary>
    public int OrderCount { get; set; }
}

/// <summary>DTO for customer statistics.</summary>
public class CustomerStatsDto
{
    /// <summary>Gets or sets total customers.</summary>
    public int TotalCustomers { get; set; }
    /// <summary>Gets or sets new customers this month.</summary>
    public int NewCustomersThisMonth { get; set; }
    /// <summary>Gets or sets active customers.</summary>
    public int ActiveCustomers { get; set; }
}
