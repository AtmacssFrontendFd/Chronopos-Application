using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for RefundTransactionProduct entity operations
/// </summary>
public interface IRefundTransactionProductRepository : IRepository<RefundTransactionProduct>
{
    /// <summary>
    /// Gets refund products by refund transaction ID
    /// </summary>
    Task<IEnumerable<RefundTransactionProduct>> GetByRefundTransactionIdAsync(int refundTransactionId);
    
    /// <summary>
    /// Gets refund products by transaction product ID
    /// </summary>
    Task<IEnumerable<RefundTransactionProduct>> GetByTransactionProductIdAsync(int transactionProductId);
    
    /// <summary>
    /// Updates a refund transaction product
    /// </summary>
    void Update(RefundTransactionProduct refundTransactionProduct);
    
    /// <summary>
    /// Deletes a refund transaction product
    /// </summary>
    void Delete(RefundTransactionProduct refundTransactionProduct);
}
