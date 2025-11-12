using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Transaction entity operations
/// </summary>
public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Gets transactions by shift ID
    /// </summary>
    Task<IEnumerable<Transaction>> GetByShiftIdAsync(int shiftId);
    
    /// <summary>
    /// Gets transactions by customer ID
    /// </summary>
    Task<IEnumerable<Transaction>> GetByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Gets transactions by status
    /// </summary>
    Task<IEnumerable<Transaction>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets transactions for a specific date range
    /// </summary>
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets transaction by invoice number
    /// </summary>
    Task<Transaction?> GetByInvoiceNumberAsync(string invoiceNumber);
    
    /// <summary>
    /// Gets transactions by table ID
    /// </summary>
    Task<IEnumerable<Transaction>> GetByTableIdAsync(int tableId);
    
    /// <summary>
    /// Gets transactions with all related data
    /// </summary>
    Task<Transaction?> GetWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets today's transactions
    /// </summary>
    Task<IEnumerable<Transaction>> GetTodaysTransactionsAsync();
    
    /// <summary>
    /// Gets all transactions with full details
    /// </summary>
    Task<IEnumerable<Transaction>> GetAllWithDetailsAsync();
    
    /// <summary>
    /// Gets transaction by ID with full details
    /// </summary>
    Task<Transaction?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Updates a transaction
    /// </summary>
    void Update(Transaction transaction);
    
    /// <summary>
    /// Deletes a transaction
    /// </summary>
    void Delete(Transaction transaction);
}
