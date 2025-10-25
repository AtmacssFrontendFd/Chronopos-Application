using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for RefundTransaction entity operations
/// </summary>
public interface IRefundTransactionRepository : IRepository<RefundTransaction>
{
    /// <summary>
    /// Gets refund transactions by selling transaction ID
    /// </summary>
    Task<IEnumerable<RefundTransaction>> GetBySellingTransactionIdAsync(int sellingTransactionId);
    
    /// <summary>
    /// Gets refund transactions by customer ID
    /// </summary>
    Task<IEnumerable<RefundTransaction>> GetByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Gets refund transactions by shift ID
    /// </summary>
    Task<IEnumerable<RefundTransaction>> GetByShiftIdAsync(int shiftId);
    
    /// <summary>
    /// Gets refund transaction with details
    /// </summary>
    Task<RefundTransaction?> GetWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets refund transactions for a specific date range
    /// </summary>
    Task<IEnumerable<RefundTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets refund transactions by transaction ID (alias for GetBySellingTransactionIdAsync)
    /// </summary>
    Task<IEnumerable<RefundTransaction>> GetByTransactionIdAsync(int transactionId);
    
    /// <summary>
    /// Gets all refund transactions with details
    /// </summary>
    Task<IEnumerable<RefundTransaction>> GetAllWithDetailsAsync();
    
    /// <summary>
    /// Gets refund transaction by ID with details
    /// </summary>
    Task<RefundTransaction?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Updates a refund transaction
    /// </summary>
    void Update(RefundTransaction refundTransaction);
    
    /// <summary>
    /// Deletes a refund transaction
    /// </summary>
    void Delete(RefundTransaction refundTransaction);
}
