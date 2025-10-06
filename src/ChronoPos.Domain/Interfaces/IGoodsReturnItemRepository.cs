using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for GoodsReturnItem operations
/// </summary>
public interface IGoodsReturnItemRepository : IRepository<GoodsReturnItem>
{
    /// <summary>
    /// Gets goods return items by return ID asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<IEnumerable<GoodsReturnItem>> GetByReturnIdAsync(int returnId);
    
    /// <summary>
    /// Gets goods return items by product ID asynchronously
    /// </summary>
    /// <param name="productId">Product ID</param>
    Task<IEnumerable<GoodsReturnItem>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets goods return items by batch number asynchronously
    /// </summary>
    /// <param name="batchNo">Batch number</param>
    Task<IEnumerable<GoodsReturnItem>> GetByBatchNoAsync(string batchNo);
    
    /// <summary>
    /// Gets goods return items by reason asynchronously
    /// </summary>
    /// <param name="reason">Return reason</param>
    Task<IEnumerable<GoodsReturnItem>> GetByReasonAsync(string reason);
    
    /// <summary>
    /// Deletes all items for a specific return asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task DeleteByReturnIdAsync(int returnId);
    
    /// <summary>
    /// Gets items with product details by return ID
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<IEnumerable<GoodsReturnItem>> GetWithProductDetailsByReturnIdAsync(int returnId);
    
    /// <summary>
    /// Gets a goods return item by ID with related entities
    /// </summary>
    /// <param name="id">Return item ID</param>
    Task<GoodsReturnItem?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets all goods return items with pagination and related entities
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    Task<(IEnumerable<GoodsReturnItem> Items, int TotalCount)> GetAllWithDetailsAsync(int page, int pageSize);
}