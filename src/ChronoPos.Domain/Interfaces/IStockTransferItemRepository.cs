using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for StockTransferItem entity operations
/// </summary>
public interface IStockTransferItemRepository : IRepository<StockTransferItem>
{
    /// <summary>
    /// Gets stock transfer items by transfer ID
    /// </summary>
    /// <param name="transferId">Stock transfer ID</param>
    /// <returns>Collection of stock transfer items</returns>
    Task<IEnumerable<StockTransferItem>> GetByTransferIdAsync(int transferId);
    
    /// <summary>
    /// Gets stock transfer items by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock transfer items</returns>
    Task<IEnumerable<StockTransferItem>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets stock transfer items by status
    /// </summary>
    /// <param name="status">Item status</param>
    /// <returns>Collection of stock transfer items</returns>
    Task<IEnumerable<StockTransferItem>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Updates the status of a stock transfer item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="status">New status</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateStatusAsync(int itemId, string status);
    
    /// <summary>
    /// Updates quantities for a stock transfer item
    /// </summary>
    /// <param name="itemId">Item ID</param>
    /// <param name="quantityReceived">Quantity received</param>
    /// <param name="damagedQty">Damaged quantity</param>
    /// <returns>True if update was successful</returns>
    Task<bool> UpdateQuantitiesAsync(int itemId, decimal quantityReceived, decimal damagedQty);
    
    /// <summary>
    /// Gets pending items for a specific transfer
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of pending items</returns>
    Task<IEnumerable<StockTransferItem>> GetPendingItemsAsync(int transferId);
    
    /// <summary>
    /// Gets items with product and UOM details
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of items with related data</returns>
    Task<IEnumerable<StockTransferItem>> GetItemsWithDetailsAsync(int transferId);
    
    /// <summary>
    /// Checks if any items exist for a product across all transfers
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if items exist</returns>
    Task<bool> HasItemsForProductAsync(int productId);
}