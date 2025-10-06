using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for StockTransfer operations
/// </summary>
public interface IStockTransferRepository : IRepository<StockTransfer>
{
    /// <summary>
    /// Gets stock transfers by status asynchronously
    /// </summary>
    /// <param name="status">Transfer status</param>
    Task<IEnumerable<StockTransfer>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets stock transfers by from store ID asynchronously
    /// </summary>
    /// <param name="fromStoreId">From store ID</param>
    Task<IEnumerable<StockTransfer>> GetByFromStoreAsync(int fromStoreId);
    
    /// <summary>
    /// Gets stock transfers by to store ID asynchronously
    /// </summary>
    /// <param name="toStoreId">To store ID</param>
    Task<IEnumerable<StockTransfer>> GetByToStoreAsync(int toStoreId);
    
    /// <summary>
    /// Gets stock transfers by date range asynchronously
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    Task<IEnumerable<StockTransfer>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets the next transfer number
    /// </summary>
    Task<string> GetNextTransferNumberAsync();
    
    /// <summary>
    /// Gets transfer with items by ID asynchronously
    /// </summary>
    /// <param name="transferId">Transfer ID</param>
    Task<StockTransfer?> GetWithItemsByIdAsync(int transferId);
}
