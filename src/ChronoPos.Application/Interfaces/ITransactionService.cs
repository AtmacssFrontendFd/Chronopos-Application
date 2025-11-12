using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Transaction operations
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Gets all transactions
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetAllAsync();
    
    /// <summary>
    /// Gets transaction by ID
    /// </summary>
    Task<TransactionDto?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets transaction with full details
    /// </summary>
    Task<TransactionDto?> GetWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets transactions by shift ID
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetByShiftIdAsync(int shiftId);
    
    /// <summary>
    /// Gets transactions by customer ID
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetByCustomerIdAsync(int customerId);
    
    /// <summary>
    /// Gets transactions by status
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets transactions by date range
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets transaction by invoice number
    /// </summary>
    Task<TransactionDto?> GetByInvoiceNumberAsync(string invoiceNumber);
    
    /// <summary>
    /// Gets today's transactions
    /// </summary>
    Task<IEnumerable<TransactionDto>> GetTodaysTransactionsAsync();
    
    /// <summary>
    /// Creates a new transaction
    /// </summary>
    Task<TransactionDto> CreateAsync(CreateTransactionDto createTransactionDto, int currentUserId);
    
    /// <summary>
    /// Updates an existing transaction
    /// </summary>
    Task<TransactionDto> UpdateAsync(int id, UpdateTransactionDto updateTransactionDto, int currentUserId);
    
    /// <summary>
    /// Deletes a transaction
    /// </summary>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Changes transaction status
    /// </summary>
    Task<TransactionDto> ChangeStatusAsync(int id, string newStatus, int currentUserId);
    
    /// <summary>
    /// Generates invoice number for transaction
    /// </summary>
    Task<TransactionDto> GenerateInvoiceNumberAsync(int id);
}
