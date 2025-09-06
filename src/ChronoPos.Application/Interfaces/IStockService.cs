using ChronoPos.Domain.Entities;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Interface for stock management operations
/// </summary>
public interface IStockService
{
    // Stock Level Operations
    Task<StockSaveResult> CreateStockLevelAsync(int productId, int storeId, decimal currentStock, decimal averageCost);
    Task<StockSaveResult> CreateStockMovementAsync(int productId, int storeId, StockDirection direction, decimal quantity, decimal unitCost, string referenceType, string? referenceNumber = null, string? notes = null, string createdBy = "System");
    Task<StockLevelDto?> GetStockLevelAsync(int productId, int storeId);
    Task<IEnumerable<StockLevelDto>> GetAllStockLevelsAsync(int productId);
    
    // Stock Movement Operations
    Task<bool> AdjustStockAsync(int productId, int quantity, StockDirection direction, string? reference = null, string? notes = null);
    Task<bool> ReceiveStockAsync(int productId, int quantity, decimal? unitCost = null, string? supplierName = null, string? batchNumber = null, DateTime? expiryDate = null);
    Task<bool> IssueStockAsync(int productId, int quantity, string? reference = null);
    Task<bool> TransferStockAsync(int fromProductId, int toProductId, int quantity, string? reference = null);
    
    // Stock Queries
    Task<int> GetCurrentStockAsync(int productId);
    Task<IEnumerable<StockTransaction>> GetStockHistoryAsync(int productId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    Task<IEnumerable<Product>> GetOutOfStockProductsAsync();
    Task<IEnumerable<Product>> GetOverstockProductsAsync();
    
    // Stock Alerts
    Task<IEnumerable<StockAlert>> GetActiveAlertsAsync();
    Task<bool> CreateStockAlertAsync(int productId, StockAlertType alertType, string message);
    Task<bool> MarkAlertAsReadAsync(int alertId, string readBy);
    Task CheckAndCreateAlertsAsync(int productId);
    
    // Stock Valuation
    Task<decimal> GetStockValueAsync(int? productId = null);
    Task<decimal> GetStockValueAtCostAsync(int? productId = null);
    
    // Stock Reports
    Task<IEnumerable<StockTransaction>> GetStockMovementReportAsync(DateTime fromDate, DateTime toDate, int? productId = null);
    Task<IEnumerable<Product>> GetStockValuationReportAsync();
    
    // Reorder Management
    Task<IEnumerable<Product>> GetProductsToReorderAsync();
    Task<bool> CreateReorderSuggestionAsync(int productId);
}
