using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ProductBatch operations
/// </summary>
public interface IProductBatchService
{
    /// <summary>
    /// Get all product batches with filtering and pagination
    /// </summary>
    Task<PagedResult<ProductBatchDto>> GetProductBatchesAsync(
        int page = 1,
        int pageSize = 20,
        int? productId = null,
        string? batchNo = null,
        string? status = null,
        DateTime? expiryFrom = null,
        DateTime? expiryTo = null);
    
    /// <summary>
    /// Get product batch by ID
    /// </summary>
    Task<ProductBatchDto?> GetProductBatchByIdAsync(int id);
    
    /// <summary>
    /// Get product batches by product ID
    /// </summary>
    Task<List<ProductBatchDto>> GetProductBatchesByProductIdAsync(int productId);
    
    /// <summary>
    /// Get expired batches
    /// </summary>
    Task<List<ProductBatchDto>> GetExpiredBatchesAsync();
    
    /// <summary>
    /// Get batches near expiry
    /// </summary>
    Task<List<ProductBatchDto>> GetNearExpiryBatchesAsync(int days = 30);
    
    /// <summary>
    /// Get batch summary for a product
    /// </summary>
    Task<ProductBatchSummaryDto> GetProductBatchSummaryAsync(int productId);
    
    /// <summary>
    /// Create new product batch
    /// </summary>
    Task<ProductBatchDto> CreateProductBatchAsync(CreateProductBatchDto createDto);
    
    /// <summary>
    /// Update existing product batch
    /// </summary>
    Task<ProductBatchDto> UpdateProductBatchAsync(int id, CreateProductBatchDto updateDto);
    
    /// <summary>
    /// Delete product batch
    /// </summary>
    Task<bool> DeleteProductBatchAsync(int id);
    
    /// <summary>
    /// Check if batch number exists for a product
    /// </summary>
    Task<bool> BatchExistsAsync(string batchNo, int productId, int? excludeId = null);
    
    /// <summary>
    /// Update batch quantity (for inventory transactions)
    /// </summary>
    Task<bool> UpdateBatchQuantityAsync(int batchId, decimal newQuantity);
    
    /// <summary>
    /// Get available quantity for a product across all active batches
    /// </summary>
    Task<decimal> GetAvailableQuantityAsync(int productId);
}