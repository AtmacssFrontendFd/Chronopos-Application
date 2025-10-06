using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for StockTransferItem operations
/// </summary>
public interface IStockTransferItemService
{
    /// <summary>
    /// Gets all stock transfer items
    /// </summary>
    /// <returns>Collection of stock transfer item DTOs</returns>
    Task<IEnumerable<StockTransferItemDto>> GetAllAsync();

    /// <summary>
    /// Gets stock transfer item by ID
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>Stock transfer item DTO if found</returns>
    Task<StockTransferItemDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets stock transfer items by transfer ID
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of stock transfer item DTOs</returns>
    Task<IEnumerable<StockTransferItemDto>> GetByTransferIdAsync(int transferId);

    /// <summary>
    /// Gets stock transfer items by product ID
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock transfer item DTOs</returns>
    Task<IEnumerable<StockTransferItemDto>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets stock transfer items by status
    /// </summary>
    /// <param name="status">Item status</param>
    /// <returns>Collection of stock transfer item DTOs</returns>
    Task<IEnumerable<StockTransferItemDto>> GetByStatusAsync(string status);

    /// <summary>
    /// Creates a new stock transfer item
    /// </summary>
    /// <param name="createItemDto">Stock transfer item data</param>
    /// <returns>Created stock transfer item DTO</returns>
    Task<StockTransferItemDto> CreateAsync(CreateStockTransferItemDto createItemDto);

    /// <summary>
    /// Updates an existing stock transfer item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="updateItemDto">Updated item data</param>
    /// <returns>Updated stock transfer item DTO</returns>
    Task<StockTransferItemDto> UpdateAsync(int id, UpdateStockTransferItemDto updateItemDto);

    /// <summary>
    /// Updates item status
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="statusDto">Status update data</param>
    /// <returns>Updated stock transfer item DTO</returns>
    Task<StockTransferItemDto> UpdateStatusAsync(int id, StockTransferItemStatusDto statusDto);

    /// <summary>
    /// Updates item quantities (received and damaged)
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <param name="quantityDto">Quantity update data</param>
    /// <returns>Updated stock transfer item DTO</returns>
    Task<StockTransferItemDto> UpdateQuantitiesAsync(int id, StockTransferItemQuantityDto quantityDto);

    /// <summary>
    /// Deletes a stock transfer item
    /// </summary>
    /// <param name="id">Item ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Gets pending items for a specific transfer
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of pending item DTOs</returns>
    Task<IEnumerable<StockTransferItemDto>> GetPendingItemsAsync(int transferId);

    /// <summary>
    /// Gets items with full details including product and transfer info
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <returns>Collection of detailed item DTOs</returns>
    Task<IEnumerable<StockTransferItemDetailDto>> GetItemsWithDetailsAsync(int transferId);

    /// <summary>
    /// Checks if any items exist for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if items exist</returns>
    Task<bool> HasItemsForProductAsync(int productId);

    /// <summary>
    /// Bulk update item statuses
    /// </summary>
    /// <param name="updates">Collection of status updates</param>
    /// <returns>Number of items updated</returns>
    Task<int> BulkUpdateStatusAsync(IEnumerable<StockTransferItemBulkStatusDto> updates);

    /// <summary>
    /// Receive multiple items in a transfer
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    /// <param name="receivedItems">Collection of received item data</param>
    /// <returns>True if all items were updated successfully</returns>
    Task<bool> ReceiveItemsAsync(int transferId, IEnumerable<StockTransferItemReceiveDto> receivedItems);
}