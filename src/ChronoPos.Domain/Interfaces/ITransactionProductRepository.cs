using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for TransactionProduct entity operations
/// </summary>
public interface ITransactionProductRepository : IRepository<TransactionProduct>
{
    /// <summary>
    /// Gets transaction products by transaction ID
    /// </summary>
    Task<IEnumerable<TransactionProduct>> GetByTransactionIdAsync(int transactionId);
    
    /// <summary>
    /// Gets transaction products by product ID
    /// </summary>
    Task<IEnumerable<TransactionProduct>> GetByProductIdAsync(int productId);
    
    /// <summary>
    /// Gets transaction products by status
    /// </summary>
    Task<IEnumerable<TransactionProduct>> GetByStatusAsync(string status);
    
    /// <summary>
    /// Gets transaction product with modifiers
    /// </summary>
    Task<TransactionProduct?> GetWithModifiersAsync(int id);
    
    /// <summary>
    /// Gets all transaction products with details
    /// </summary>
    Task<IEnumerable<TransactionProduct>> GetAllWithDetailsAsync();
    
    /// <summary>
    /// Updates a transaction product
    /// </summary>
    void Update(TransactionProduct transactionProduct);
    
    /// <summary>
    /// Deletes a transaction product
    /// </summary>
    void Delete(TransactionProduct transactionProduct);
}
