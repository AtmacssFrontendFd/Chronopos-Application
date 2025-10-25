using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ExchangeTransactionProduct entity operations
/// </summary>
public interface IExchangeTransactionProductRepository : IRepository<ExchangeTransactionProduct>
{
    /// <summary>
    /// Gets exchange products by exchange transaction ID
    /// </summary>
    Task<IEnumerable<ExchangeTransactionProduct>> GetByExchangeTransactionIdAsync(int exchangeTransactionId);
    
    /// <summary>
    /// Gets exchange products by original transaction product ID
    /// </summary>
    Task<IEnumerable<ExchangeTransactionProduct>> GetByOriginalTransactionProductIdAsync(int originalTransactionProductId);
    
    /// <summary>
    /// Gets exchange products by new product ID
    /// </summary>
    Task<IEnumerable<ExchangeTransactionProduct>> GetByNewProductIdAsync(int newProductId);
    
    /// <summary>
    /// Updates an exchange transaction product
    /// </summary>
    void Update(ExchangeTransactionProduct exchangeTransactionProduct);
    
    /// <summary>
    /// Deletes an exchange transaction product
    /// </summary>
    void Delete(ExchangeTransactionProduct exchangeTransactionProduct);
}
