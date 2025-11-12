using ChronoPos.Application.DTOs.Inventory;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Inventory Report operations
/// Provides comprehensive inventory reporting and analytics
/// </summary>
public interface IInventoryReportService
{
    /// <summary>
    /// Report 1: Gets stock summary showing opening, inward, outward, and closing stock
    /// </summary>
    /// <param name="startDate">Start date for the period</param>
    /// <param name="endDate">End date for the period</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="productId">Optional product filter</param>
    /// <returns>Collection of stock summary data</returns>
    Task<IEnumerable<StockSummaryDto>> GetStockSummaryAsync(
        DateTime startDate, 
        DateTime endDate, 
        int? categoryId = null,
        int? productId = null);
    
    /// <summary>
    /// Report 2: Gets detailed stock ledger/movement history for a product
    /// </summary>
    /// <param name="productId">Product ID (required)</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of stock ledger entries</returns>
    Task<IEnumerable<StockLedgerReportDto>> GetStockLedgerAsync(
        int? productId, 
        DateTime startDate, 
        DateTime endDate);
    
    /// <summary>
    /// Report 3: Gets current stock valuation report
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="includeZeroStock">Include products with zero stock</param>
    /// <returns>Collection of stock valuation data</returns>
    Task<IEnumerable<StockValuationDto>> GetStockValuationAsync(
        int? categoryId = null,
        bool includeZeroStock = false);
    
    /// <summary>
    /// Report 4: Gets low stock/reorder report
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="showOnlyOutOfStock">Show only out of stock items</param>
    /// <returns>Collection of low stock items</returns>
    Task<IEnumerable<LowStockDto>> GetLowStockReportAsync(
        int? categoryId = null,
        bool showOnlyOutOfStock = false);
    
    /// <summary>
    /// Report 5: Gets goods received note (GRN) report
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="supplierId">Optional supplier filter</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>Collection of GRN data</returns>
    Task<IEnumerable<GoodsReceivedReportDto>> GetGoodsReceivedReportAsync(
        DateTime startDate, 
        DateTime endDate,
        int? supplierId = null,
        string? status = null);
    
    /// <summary>
    /// Report 6: Gets stock adjustment report
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>Collection of stock adjustment data</returns>
    Task<IEnumerable<StockAdjustmentReportDto>> GetStockAdjustmentReportAsync(
        DateTime startDate, 
        DateTime endDate,
        string? status = null);
    
    /// <summary>
    /// Report 7: Gets stock transfer report
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>Collection of stock transfer data</returns>
    Task<IEnumerable<StockTransferReportDto>> GetStockTransferReportAsync(
        DateTime startDate, 
        DateTime endDate,
        string? status = null);
    
    /// <summary>
    /// Report 8: Gets stock aging report
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="minDays">Minimum days in stock</param>
    /// <returns>Collection of stock aging data</returns>
    Task<IEnumerable<StockAgingDto>> GetStockAgingReportAsync(
        int? categoryId = null,
        int minDays = 0);
    
    /// <summary>
    /// Report 9: Gets goods return/replacement report
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="supplierId">Optional supplier filter</param>
    /// <param name="type">Optional type filter (Return/Replace)</param>
    /// <returns>Collection of goods return data</returns>
    Task<IEnumerable<GoodsReturnReportDto>> GetGoodsReturnReportAsync(
        DateTime startDate, 
        DateTime endDate,
        int? supplierId = null,
        string? type = null);
    
    /// <summary>
    /// Report 10: Gets inventory summary by category
    /// </summary>
    /// <returns>Collection of category summary data</returns>
    Task<IEnumerable<InventorySummaryDto>> GetInventorySummaryByCategoryAsync();
    
    /// <summary>
    /// Exports inventory report to Excel
    /// </summary>
    /// <param name="filter">Report filter</param>
    /// <returns>Excel file as byte array</returns>
    Task<byte[]> ExportToExcelAsync(InventoryReportFilterDto filter);
    
    /// <summary>
    /// Exports inventory report to PDF
    /// </summary>
    /// <param name="filter">Report filter</param>
    /// <returns>PDF file as byte array</returns>
    Task<byte[]> ExportToPdfAsync(InventoryReportFilterDto filter);
    
    /// <summary>
    /// Exports inventory report to CSV
    /// </summary>
    /// <param name="filter">Report filter</param>
    /// <returns>CSV file as byte array</returns>
    Task<byte[]> ExportToCsvAsync(InventoryReportFilterDto filter);
}
