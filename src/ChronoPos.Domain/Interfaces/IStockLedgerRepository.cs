using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for StockLedger entity operations
/// </summary>
public interface IStockLedgerRepository : IRepository<StockLedger>
{
    /// <summary>
    /// Gets all stock ledger entries for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock ledger entries</returns>
    Task<IEnumerable<StockLedger>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets stock ledger entries for a product within a date range
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of stock ledger entries</returns>
    Task<IEnumerable<StockLedger>> GetByProductIdAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets the current balance for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Current balance</returns>
    Task<decimal> GetCurrentBalanceAsync(int productId);

    /// <summary>
    /// Gets stock ledger entries by movement type
    /// </summary>
    /// <param name="movementType">Movement type</param>
    /// <returns>Collection of stock ledger entries</returns>
    Task<IEnumerable<StockLedger>> GetByMovementTypeAsync(Domain.Enums.StockMovementType movementType);

    /// <summary>
    /// Gets stock ledger entries by reference
    /// </summary>
    /// <param name="referenceType">Reference type</param>
    /// <param name="referenceId">Reference ID</param>
    /// <returns>Collection of stock ledger entries</returns>
    Task<IEnumerable<StockLedger>> GetByReferenceAsync(Domain.Enums.StockReferenceType? referenceType, int? referenceId);

    /// <summary>
    /// Gets the latest stock entry for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Latest stock ledger entry</returns>
    Task<StockLedger?> GetLatestByProductIdAsync(int productId);
}
