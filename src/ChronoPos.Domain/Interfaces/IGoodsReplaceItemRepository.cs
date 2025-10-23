using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for GoodsReplaceItem operations
/// </summary>
public interface IGoodsReplaceItemRepository : IRepository<GoodsReplaceItem>
{
    /// <summary>
    /// Gets goods replace items by replace ID asynchronously
    /// </summary>
    /// <param name="replaceId">Replace ID</param>
    Task<IEnumerable<GoodsReplaceItem>> GetByReplaceIdAsync(int replaceId);
    
    /// <summary>
    /// Gets goods replace items by product ID asynchronously
    /// </summary>
    /// <param name="productId">Product ID</param>
    Task<IEnumerable<GoodsReplaceItem>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets goods replace item with details by ID asynchronously
    /// </summary>
    /// <param name="itemId">Item ID</param>
    Task<GoodsReplaceItem?> GetWithDetailsAsync(int itemId);
}
