using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Comprehensive Sales Report Data Transfer Object
/// </summary>
public class SalesReportDto
{
    public SalesSummaryDto Summary { get; set; } = new();
    public List<SaleTransactionDto> Transactions { get; set; } = new();
    public List<ProductPerformanceDto> TopProducts { get; set; } = new();
    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();
    public List<PaymentMethodBreakdownDto> PaymentBreakdown { get; set; } = new();
    public List<DailySalesDto> DailyTrend { get; set; } = new();
    public List<HourlySalesDto> HourlyDistribution { get; set; } = new();
    public int TotalRecords { get; set; }
}

/// <summary>
/// Sales Report Filter DTO
/// </summary>
public class SalesReportFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? CustomerId { get; set; }
    public int? CategoryId { get; set; }
    public int? ProductId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public SaleStatus? Status { get; set; }
    public decimal? MinimumAmount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "SaleDate";
    public bool SortDescending { get; set; } = true;
    public string CurrencySymbol { get; set; } = "€"; // Active currency symbol for exports
}

/// <summary>
/// Sales Summary DTO with aggregated metrics
/// </summary>
public class SalesSummaryDto
{
    public decimal TotalSalesAmount { get; set; }
    public int TotalTransactions { get; set; }
    public decimal AverageTransactionValue { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal TotalDiscountGiven { get; set; }
    public decimal TotalTaxCollected { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetRevenue { get; set; }
    public decimal RefundAmount { get; set; }
    public int RefundCount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Product Performance DTO
/// </summary>
public class ProductPerformanceDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal AveragePrice { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Category Performance DTO
/// </summary>
public class CategoryPerformanceDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal RevenuePercentage { get; set; }
    public int TransactionCount { get; set; }
}

/// <summary>
/// Payment Method Breakdown DTO
/// </summary>
public class PaymentMethodBreakdownDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Percentage { get; set; }
    public decimal AverageTransactionValue { get; set; }
}

/// <summary>
/// Daily Sales DTO for trend analysis
/// </summary>
public class DailySalesDto
{
    public DateTime Date { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public decimal TotalSales { get; set; }
    public int TransactionCount { get; set; }
    public int ItemsSold { get; set; }
    public decimal AverageValue { get; set; }
}

/// <summary>
/// Individual Sale Transaction DTO for grid display
/// </summary>
public class SaleTransactionDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public SaleStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
