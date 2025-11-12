using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Main DTO for customer report with comprehensive customer analytics
/// </summary>
public class CustomerReportDto
{
    public CustomerSummaryDto Summary { get; set; } = new();
    public List<CustomerAnalysisDto> Customers { get; set; } = new();
    public List<CustomerRankingDto> TopCustomersByRevenue { get; set; } = new();
    public List<CustomerRankingDto> TopCustomersByPurchases { get; set; } = new();
    public List<CustomerGrowthDto> CustomerGrowthTrend { get; set; } = new();
    public List<CustomerSegmentDto> CustomerSegments { get; set; } = new();
    public int TotalRecords { get; set; }
}

/// <summary>
/// Summary statistics for customer report
/// </summary>
public class CustomerSummaryDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisPeriod { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerCustomer { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalPurchases { get; set; }
    public double CustomerRetentionRate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Detailed customer analysis with purchase patterns
/// </summary>
public class CustomerAnalysisDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int TotalPurchases { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? FirstPurchaseDate { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public int DaysSinceLastPurchase { get; set; }
    public string FavoriteCategory { get; set; } = "N/A";
    public string PreferredPaymentMethod { get; set; } = "N/A";
    public decimal CustomerLifetimeValue { get; set; }
    public string PurchaseFrequency { get; set; } = "Unknown";
    public bool IsActive { get; set; }
    public string CustomerSegment { get; set; } = "Regular";
}

/// <summary>
/// Top customers ranking for reports
/// </summary>
public class CustomerRankingDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalPurchases { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public int Rank { get; set; }
}

/// <summary>
/// Customer product preferences
/// </summary>
public class CustomerProductPreferenceDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastPurchased { get; set; }
}

/// <summary>
/// Customer growth trend over time
/// </summary>
public class CustomerGrowthDto
{
    public DateTime Date { get; set; }
    public int NewCustomers { get; set; }
    public int TotalCustomers { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>
/// Customer segmentation data
/// </summary>
public class CustomerSegmentDto
{
    public string SegmentName { get; set; } = string.Empty;
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public double PercentageOfTotal { get; set; }
}

/// <summary>
/// Customer payment method analysis
/// </summary>
public class CustomerPaymentAnalysisDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Filter criteria for customer reports
/// </summary>
public class CustomerReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? CustomerSegment { get; set; }
    public decimal? MinimumRevenue { get; set; }
    public decimal? MaximumRevenue { get; set; }
    public int? MinimumPurchases { get; set; }
    public string SortBy { get; set; } = "TotalRevenue";
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? CurrencySymbol { get; set; }
}
