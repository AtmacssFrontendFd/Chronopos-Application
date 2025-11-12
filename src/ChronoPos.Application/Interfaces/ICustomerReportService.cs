using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for customer reporting and analytics
/// </summary>
public interface ICustomerReportService
{
    /// <summary>
    /// Generate comprehensive customer report with filters
    /// </summary>
    Task<CustomerReportDto> GenerateCustomerReportAsync(CustomerReportFilterDto filter);

    /// <summary>
    /// Get customer summary statistics
    /// </summary>
    Task<CustomerSummaryDto> GetCustomerSummaryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get detailed customer analysis list
    /// </summary>
    Task<(List<CustomerAnalysisDto> Customers, int TotalCount)> GetCustomerAnalysisAsync(CustomerReportFilterDto filter);

    /// <summary>
    /// Get top customers by revenue
    /// </summary>
    Task<List<CustomerRankingDto>> GetTopCustomersByRevenueAsync(DateTime startDate, DateTime endDate, int topCount = 10);

    /// <summary>
    /// Get top customers by purchase count
    /// </summary>
    Task<List<CustomerRankingDto>> GetTopCustomersByPurchasesAsync(DateTime startDate, DateTime endDate, int topCount = 10);

    /// <summary>
    /// Get customer product preferences
    /// </summary>
    Task<List<CustomerProductPreferenceDto>> GetCustomerProductPreferencesAsync(int customerId, int topCount = 10);

    /// <summary>
    /// Get customer payment method preferences
    /// </summary>
    Task<List<CustomerPaymentAnalysisDto>> GetCustomerPaymentAnalysisAsync(int customerId);

    /// <summary>
    /// Get customer growth trend over time
    /// </summary>
    Task<List<CustomerGrowthDto>> GetCustomerGrowthTrendAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get customer segmentation data
    /// </summary>
    Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Export customer report to Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync(CustomerReportFilterDto filter);

    /// <summary>
    /// Export customer report to CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync(CustomerReportFilterDto filter);

    /// <summary>
    /// Export customer report to PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync(CustomerReportFilterDto filter);
}
