using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for goods replace item operations
/// </summary>
public interface IGoodsReplaceItemService
{
    /// <summary>
    /// Get goods replace items by replace ID
    /// </summary>
    Task<List<GoodsReplaceItemDto>> GetItemsByReplaceIdAsync(int replaceId);
    
    /// <summary>
    /// Get goods replace item by ID
    /// </summary>
    Task<GoodsReplaceItemDto?> GetItemByIdAsync(int itemId);
    
    /// <summary>
    /// Get detailed goods replace item by ID
    /// </summary>
    Task<GoodsReplaceItemDetailDto?> GetItemDetailAsync(int itemId);
    
    /// <summary>
    /// Update goods replace item
    /// </summary>
    Task<GoodsReplaceItemDto> UpdateItemAsync(int itemId, UpdateGoodsReplaceItemDto dto);
    
    /// <summary>
    /// Delete goods replace item
    /// </summary>
    Task<bool> DeleteItemAsync(int itemId);
    
    /// <summary>
    /// Get summary statistics for goods replace items
    /// </summary>
    Task<GoodsReplaceItemSummaryDto> GetItemsSummaryAsync(int replaceId);
}
