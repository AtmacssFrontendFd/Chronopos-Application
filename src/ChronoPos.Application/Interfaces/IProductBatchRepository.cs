using ChronoPos.Domain.Entities;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Repository interface for ProductBatch operations
/// </summary>
public interface IProductBatchRepository
{
    /// <summary>
    /// Get all product batches with optional filtering
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetAllAsync(
        int? productId = null,
        string? status = null,
        DateTime? expiryFrom = null,
        DateTime? expiryTo = null);
    
    /// <summary>
    /// Get product batch by ID
    /// </summary>
    Task<ProductBatch?> GetByIdAsync(int id);
    
    /// <summary>
    /// Get product batches by product ID
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Get product batch by batch number and product ID
    /// </summary>
    Task<ProductBatch?> GetByBatchNoAsync(string batchNo, int productId);
    
    /// <summary>
    /// Get expired batches
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetExpiredBatchesAsync();
    
    /// <summary>
    /// Get batches near expiry (within specified days)
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetNearExpiryBatchesAsync(int days = 30);
    
    /// <summary>
    /// Get batches by status
    /// </summary>
    Task<IEnumerable<ProductBatch>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Add new product batch
    /// </summary>
    Task<ProductBatch> AddAsync(ProductBatch productBatch);
    
    /// <summary>
    /// Update existing product batch
    /// </summary>
    Task<ProductBatch> UpdateAsync(ProductBatch productBatch);
    
    /// <summary>
    /// Delete product batch
    /// </summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Check if batch number exists for a product
    /// </summary>
    Task<bool> BatchExistsAsync(string batchNo, int productId, int? excludeId = null);
    
    /// <summary>
    /// Get total quantity for a product across all batches
    /// </summary>
    Task<decimal> GetTotalQuantityAsync(int productId);
    
    /// <summary>
    /// Update batch quantity (for inventory transactions)
    /// </summary>
    Task<bool> UpdateQuantityAsync(int batchId, decimal newQuantity);
}