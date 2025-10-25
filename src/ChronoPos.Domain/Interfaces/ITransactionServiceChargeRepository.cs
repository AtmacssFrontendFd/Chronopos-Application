using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for TransactionServiceCharge entity operations
/// </summary>
public interface ITransactionServiceChargeRepository : IRepository<TransactionServiceCharge>
{
    /// <summary>
    /// Gets service charges by transaction ID
    /// </summary>
    Task<IEnumerable<TransactionServiceCharge>> GetByTransactionIdAsync(int transactionId);
    
    /// <summary>
    /// Gets service charges by service charge ID
    /// </summary>
    Task<IEnumerable<TransactionServiceCharge>> GetByServiceChargeIdAsync(int serviceChargeId);
    
    /// <summary>
    /// Updates a transaction service charge
    /// </summary>
    void Update(TransactionServiceCharge transactionServiceCharge);
    
    /// <summary>
    /// Deletes a transaction service charge
    /// </summary>
    void Delete(TransactionServiceCharge transactionServiceCharge);
}
