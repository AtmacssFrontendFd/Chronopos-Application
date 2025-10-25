using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ExchangeTransaction entity operations
/// </summary>
public interface IExchangeTransactionRepository : IRepository<ExchangeTransaction>
{
    /// <summary>
    /// Gets exchange transactions by selling transaction ID
    /// </summary>
    Task<IEnumerable<ExchangeTransaction>> GetBySellingTransactionIdAsync(int sellingTransactionId);
    
    /// <summary>
    /// Gets exchange transactions by customer ID
    /// </summary>
    Task<IEnumerable<ExchangeTransaction>> GetByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Gets exchange transactions by shift ID
    /// </summary>
    Task<IEnumerable<ExchangeTransaction>> GetByShiftIdAsync(int shiftId);
    
    /// <summary>
    /// Gets exchange transaction with details
    /// </summary>
    Task<ExchangeTransaction?> GetWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets exchange transactions for a specific date range
    /// </summary>
    Task<IEnumerable<ExchangeTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets exchange transactions by transaction ID (alias for GetBySellingTransactionIdAsync)
    /// </summary>
    Task<IEnumerable<ExchangeTransaction>> GetByTransactionIdAsync(int transactionId);
    
    /// <summary>
    /// Gets all exchange transactions with details
    /// </summary>
    Task<IEnumerable<ExchangeTransaction>> GetAllWithDetailsAsync();
    
    /// <summary>
    /// Gets exchange transaction by ID with details
    /// </summary>
    Task<ExchangeTransaction?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Updates an exchange transaction
    /// </summary>
    void Update(ExchangeTransaction exchangeTransaction);
    
    /// <summary>
    /// Deletes an exchange transaction
    /// </summary>
    void Delete(ExchangeTransaction exchangeTransaction);
}
