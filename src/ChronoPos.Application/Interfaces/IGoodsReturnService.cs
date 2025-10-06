using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for goods return operations
/// </summary>
public interface IGoodsReturnService
{
    /// <summary>
    /// Creates a new goods return asynchronously
    /// </summary>
    /// <param name="dto">Create goods return DTO</param>
    Task<GoodsReturnDto?> CreateGoodsReturnAsync(CreateGoodsReturnDto dto);
    
    /// <summary>
    /// Updates an existing goods return asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    /// <param name="dto">Update goods return DTO</param>
    Task<GoodsReturnDto?> UpdateGoodsReturnAsync(int returnId, CreateGoodsReturnDto dto);
    
    /// <summary>
    /// Gets a goods return by ID asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<GoodsReturnDto?> GetGoodsReturnByIdAsync(int returnId);
    
    /// <summary>
    /// Gets goods returns with filtering and search asynchronously
    /// </summary>
    /// <param name="searchTerm">Search term for return number or supplier</param>
    /// <param name="supplierId">Optional supplier filter</param>
    /// <param name="storeId">Optional store filter</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    Task<IEnumerable<GoodsReturnDto>> GetGoodsReturnsAsync(
        string? searchTerm = null,
        int? supplierId = null,
        int? storeId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    /// <summary>
    /// Deletes a goods return asynchronously
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<bool> DeleteGoodsReturnAsync(int returnId);
    
    /// <summary>
    /// Posts a goods return (marks it as Posted and processes stock changes)
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<bool> PostGoodsReturnAsync(int returnId);
    
    /// <summary>
    /// Cancels a goods return
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<bool> CancelGoodsReturnAsync(int returnId);
    
    /// <summary>
    /// Gets goods return items by return ID
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<IEnumerable<GoodsReturnItemDto>> GetGoodsReturnItemsAsync(int returnId);
    
    /// <summary>
    /// Validates if a goods return can be edited
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<bool> CanEditGoodsReturnAsync(int returnId);
    
    /// <summary>
    /// Validates if a goods return can be deleted
    /// </summary>
    /// <param name="returnId">Return ID</param>
    Task<bool> CanDeleteGoodsReturnAsync(int returnId);
    
    /// <summary>
    /// Gets next return number
    /// </summary>
    Task<string> GetNextReturnNumberAsync();
}