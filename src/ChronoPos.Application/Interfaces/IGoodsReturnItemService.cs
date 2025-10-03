using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Interface for goods return item service operations
/// </summary>
public interface IGoodsReturnItemService
{
    /// <summary>
    /// Creates a new goods return item
    /// </summary>
    /// <param name="createDto">The goods return item data to create</param>
    /// <returns>The created goods return item</returns>
    Task<GoodsReturnItemDto> CreateAsync(CreateGoodsReturnItemDto createDto);
    
    /// <summary>
    /// Updates an existing goods return item
    /// </summary>
    /// <param name="id">The goods return item ID to update</param>
    /// <param name="updateDto">The updated goods return item data</param>
    /// <returns>The updated goods return item</returns>
    Task<GoodsReturnItemDto> UpdateAsync(int id, UpdateGoodsReturnItemDto updateDto);
    
    /// <summary>
    /// Gets a goods return item by ID
    /// </summary>
    /// <param name="id">The goods return item ID</param>
    /// <returns>The goods return item if found, null otherwise</returns>
    Task<GoodsReturnItemDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all goods return items for a specific return
    /// </summary>
    /// <param name="returnId">The goods return ID</param>
    /// <returns>List of goods return items</returns>
    Task<IEnumerable<GoodsReturnItemDto>> GetByReturnIdAsync(int returnId);
    
    /// <summary>
    /// Deletes a goods return item
    /// </summary>
    /// <param name="id">The goods return item ID to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Gets all goods return items with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated list of goods return items</returns>
    Task<(IEnumerable<GoodsReturnItemDto> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 50);
}