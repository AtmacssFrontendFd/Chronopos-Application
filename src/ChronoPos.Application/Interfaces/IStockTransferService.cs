using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for stock transfer operations
/// </summary>
public interface IStockTransferService
{
    /// <summary>
    /// Get paginated list of stock transfers with filtering
    /// </summary>
    Task<PagedResult<StockTransferDto>> GetStockTransfersAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchTerm = null,
        int? fromStoreId = null,
        int? toStoreId = null,
        string? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    /// <summary>
    /// Get stock transfer by ID
    /// </summary>
    Task<StockTransferDto?> GetStockTransferByIdAsync(int transferId);
    
    /// <summary>
    /// Create new stock transfer
    /// </summary>
    Task<StockTransferDto> CreateStockTransferAsync(CreateStockTransferDto dto);
    
    /// <summary>
    /// Update existing stock transfer
    /// </summary>
    Task<StockTransferDto> UpdateStockTransferAsync(int transferId, CreateStockTransferDto dto);
    
    /// <summary>
    /// Delete stock transfer
    /// </summary>
    Task<bool> DeleteStockTransferAsync(int transferId);
    
    /// <summary>
    /// Complete stock transfer (mark as completed)
    /// </summary>
    Task<bool> CompleteStockTransferAsync(int transferId);
    
    /// <summary>
    /// Cancel stock transfer
    /// </summary>
    Task<bool> CancelStockTransferAsync(int transferId);
    
    /// <summary>
    /// Receive items for stock transfer
    /// </summary>
    Task<bool> ReceiveTransferItemsAsync(int transferId, List<StockTransferItemDto> receivedItems);
    
    /// <summary>
    /// Get all shop locations
    /// </summary>
    Task<List<ShopLocationDto>> GetShopLocationsAsync();
    
    /// <summary>
    /// Get products available for transfer from a specific location
    /// </summary>
    Task<PagedResult<ProductStockInfoDto>> GetProductsForTransferAsync(
        int fromStoreId,
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null, 
        int? categoryId = null);
    
    /// <summary>
    /// Generate transfer number
    /// </summary>
    Task<string> GenerateTransferNumberAsync();
}
