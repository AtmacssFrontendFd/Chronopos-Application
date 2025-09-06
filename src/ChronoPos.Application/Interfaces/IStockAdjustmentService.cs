using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for stock adjustment operations
/// </summary>
public interface IStockAdjustmentService
{
    /// <summary>
    /// Get paginated list of stock adjustments with filtering
    /// </summary>
    Task<PagedResult<StockAdjustmentDto>> GetStockAdjustmentsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchTerm = null,
        int? storeLocationId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    /// <summary>
    /// Get stock adjustment by ID
    /// </summary>
    Task<StockAdjustmentDto?> GetStockAdjustmentByIdAsync(int adjustmentId);
    
    /// <summary>
    /// Create new stock adjustment
    /// </summary>
    Task<StockAdjustmentDto> CreateStockAdjustmentAsync(CreateStockAdjustmentDto dto);
    
    /// <summary>
    /// Update existing stock adjustment
    /// </summary>
    Task<StockAdjustmentDto> UpdateStockAdjustmentAsync(int adjustmentId, CreateStockAdjustmentDto dto);
    
    /// <summary>
    /// Delete stock adjustment
    /// </summary>
    Task<bool> DeleteStockAdjustmentAsync(int adjustmentId);
    
    /// <summary>
    /// Approve stock adjustment
    /// </summary>
    Task<bool> ApproveStockAdjustmentAsync(int adjustmentId);
    
    /// <summary>
    /// Cancel stock adjustment
    /// </summary>
    Task<bool> CancelStockAdjustmentAsync(int adjustmentId);
    
    /// <summary>
    /// Get all adjustment reasons
    /// </summary>
    Task<List<StockAdjustmentReasonDto>> GetAdjustmentReasonsAsync();
    
    /// <summary>
    /// Get all store locations
    /// </summary>
    Task<List<StockAdjustmentSupportDto.LocationDto>> GetStoreLocationsAsync();
    
    /// <summary>
    /// Get products available for adjustment
    /// </summary>
    Task<PagedResult<ProductStockInfoDto>> GetProductsForAdjustmentAsync(
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null, 
        int? categoryId = null,
        int? storeLocationId = null);
    
    /// <summary>
    /// Generate adjustment number
    /// </summary>
    Task<string> GenerateAdjustmentNumberAsync();
}
