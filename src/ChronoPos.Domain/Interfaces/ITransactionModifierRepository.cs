using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for TransactionModifier entity operations
/// </summary>
public interface ITransactionModifierRepository : IRepository<TransactionModifier>
{
    /// <summary>
    /// Gets modifiers by transaction product ID
    /// </summary>
    Task<IEnumerable<TransactionModifier>> GetByTransactionProductIdAsync(int transactionProductId);
    
    /// <summary>
    /// Gets modifiers by product modifier ID
    /// </summary>
    Task<IEnumerable<TransactionModifier>> GetByProductModifierIdAsync(int productModifierId);
    
    /// <summary>
    /// Updates a transaction modifier
    /// </summary>
    void Update(TransactionModifier transactionModifier);
    
    /// <summary>
    /// Deletes a transaction modifier
    /// </summary>
    void Delete(TransactionModifier transactionModifier);
}
