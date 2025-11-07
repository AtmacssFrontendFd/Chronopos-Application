using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Sales Report operations
/// </summary>
public interface ISalesReportService
{
    /// <summary>
    /// Generates comprehensive sales report based on filters
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Complete sales report data</returns>
    Task<SalesReportDto> GenerateSalesReportAsync(SalesReportFilterDto filter);

    /// <summary>
    /// Gets sales summary for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Summary metrics</returns>
    Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets top performing products
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="topCount">Number of top products to return</param>
    /// <returns>List of top products</returns>
    Task<List<ProductPerformanceDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int topCount = 10);

    /// <summary>
    /// Gets category performance breakdown
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of category performance metrics</returns>
    Task<List<CategoryPerformanceDto>> GetCategoryPerformanceAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets payment method breakdown
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Payment method statistics</returns>
    Task<List<PaymentMethodBreakdownDto>> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets daily sales trend
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Daily sales data</returns>
    Task<List<DailySalesDto>> GetDailySalesTrendAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets hourly sales distribution for a specific date
    /// </summary>
    /// <param name="date">Date to analyze</param>
    /// <returns>Hourly sales breakdown</returns>
    Task<List<HourlySalesDto>> GetHourlySalesDistributionAsync(DateTime date);

    /// <summary>
    /// Gets top customers by purchase amount
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="topCount">Number of top customers</param>
    /// <returns>List of top customers</returns>
    Task<List<TopCustomerDto>> GetTopCustomersAsync(DateTime startDate, DateTime endDate, int topCount = 10);

    /// <summary>
    /// Gets sales transactions with filters and pagination
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Paginated list of transactions</returns>
    Task<(List<SaleTransactionDto> Transactions, int TotalCount)> GetSalesTransactionsAsync(SalesReportFilterDto filter);

    /// <summary>
    /// Exports sales report to Excel
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>Excel file as byte array</returns>
    Task<byte[]> ExportToExcelAsync(SalesReportFilterDto filter);

    /// <summary>
    /// Exports sales report to CSV
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>CSV file as byte array</returns>
    Task<byte[]> ExportToCsvAsync(SalesReportFilterDto filter);

    /// <summary>
    /// Exports sales report to PDF
    /// </summary>
    /// <param name="filter">Filter criteria</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> ExportToPdfAsync(SalesReportFilterDto filter);
}
