namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Product repository
    /// </summary>
    IProductRepository Products { get; }
    
    /// <summary>
    /// Category repository
    /// </summary>
    IRepository<ChronoPos.Domain.Entities.Category> Categories { get; }
    
    /// <summary>
    /// Customer repository
    /// </summary>
    IRepository<ChronoPos.Domain.Entities.Customer> Customers { get; }
    
    /// <summary>
    /// Sale repository
    /// </summary>
    ISaleRepository Sales { get; }
    
    /// <summary>
    /// Sale item repository
    /// </summary>
    IRepository<ChronoPos.Domain.Entities.SaleItem> SaleItems { get; }
    
    /// <summary>
    /// Saves all changes asynchronously
    /// </summary>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Begins a database transaction asynchronously
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commits the current transaction asynchronously
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rolls back the current transaction asynchronously
    /// </summary>
    Task RollbackTransactionAsync();
}
