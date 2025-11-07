using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for StockLedger operations
/// </summary>
public interface IStockLedgerService
{
    /// <summary>
    /// Gets all stock ledger entries
    /// </summary>
    /// <returns>Collection of stock ledger DTOs</returns>
    Task<IEnumerable<StockLedgerDto>> GetAllAsync();

    /// <summary>
    /// Gets stock ledger entry by ID
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <returns>Stock ledger DTO if found</returns>
    Task<StockLedgerDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all stock ledger entries for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    Task<IEnumerable<StockLedgerDto>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Gets stock ledger entries for a product within a date range
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    Task<IEnumerable<StockLedgerDto>> GetByProductIdAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate);

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
    /// <returns>Collection of stock ledger DTOs</returns>
    Task<IEnumerable<StockLedgerDto>> GetByMovementTypeAsync(StockMovementType movementType);

    /// <summary>
    /// Gets stock ledger entries by reference
    /// </summary>
    /// <param name="referenceType">Reference type</param>
    /// <param name="referenceId">Reference ID</param>
    /// <returns>Collection of stock ledger DTOs</returns>
    Task<IEnumerable<StockLedgerDto>> GetByReferenceAsync(StockReferenceType? referenceType, int? referenceId);

    /// <summary>
    /// Gets the latest stock entry for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Latest stock ledger DTO</returns>
    Task<StockLedgerDto?> GetLatestByProductIdAsync(int productId);

    /// <summary>
    /// Creates a new stock ledger entry
    /// </summary>
    /// <param name="stockLedgerDto">Stock ledger data</param>
    /// <returns>Created stock ledger DTO</returns>
    Task<StockLedgerDto> CreateAsync(CreateStockLedgerDto stockLedgerDto);

    /// <summary>
    /// Updates an existing stock ledger entry
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <param name="stockLedgerDto">Updated stock ledger data</param>
    /// <returns>Updated stock ledger DTO</returns>
    Task<StockLedgerDto> UpdateAsync(int id, UpdateStockLedgerDto stockLedgerDto);

    /// <summary>
    /// Deletes a stock ledger entry
    /// </summary>
    /// <param name="id">Stock ledger ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);
}
