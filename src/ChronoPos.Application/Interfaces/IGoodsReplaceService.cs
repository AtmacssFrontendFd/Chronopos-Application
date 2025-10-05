using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for goods replace operations
/// </summary>
public interface IGoodsReplaceService
{
    /// <summary>
    /// Get paginated list of goods replaces with filtering
    /// </summary>
    Task<PagedResult<GoodsReplaceDto>> GetGoodsReplacesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchTerm = null,
        int? supplierId = null,
        int? storeId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    /// <summary>
    /// Get goods replace by ID
    /// </summary>
    Task<GoodsReplaceDto?> GetGoodsReplaceByIdAsync(int replaceId);
    
    /// <summary>
    /// Create new goods replace
    /// </summary>
    Task<GoodsReplaceDto> CreateGoodsReplaceAsync(CreateGoodsReplaceDto dto);
    
    /// <summary>
    /// Update existing goods replace
    /// </summary>
    Task<GoodsReplaceDto> UpdateGoodsReplaceAsync(int replaceId, CreateGoodsReplaceDto dto);
    
    /// <summary>
    /// Delete goods replace
    /// </summary>
    Task<bool> DeleteGoodsReplaceAsync(int replaceId);
    
    /// <summary>
    /// Post goods replace (mark as posted/completed)
    /// </summary>
    Task<bool> PostGoodsReplaceAsync(int replaceId);
    
    /// <summary>
    /// Cancel goods replace
    /// </summary>
    Task<bool> CancelGoodsReplaceAsync(int replaceId);
    
    /// <summary>
    /// Get all suppliers
    /// </summary>
    Task<List<SupplierDto>> GetSuppliersAsync();
    
    /// <summary>
    /// Get all stores
    /// </summary>
    Task<List<ShopLocationDto>> GetStoresAsync();
    
    /// <summary>
    /// Get products available for replacement
    /// </summary>
    Task<PagedResult<ProductStockInfoDto>> GetProductsForReplaceAsync(
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null, 
        int? categoryId = null);
    
    /// <summary>
    /// Generate replace number
    /// </summary>
    Task<string> GenerateReplaceNumberAsync();
    
    /// <summary>
    /// Get goods returns by supplier (for linking)
    /// </summary>
    Task<List<GoodsReturnDto>> GetGoodsReturnsBySupplierAsync(int supplierId);
}
