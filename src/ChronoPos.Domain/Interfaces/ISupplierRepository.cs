using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Supplier entities with specific business operations
/// </summary>
public interface ISupplierRepository
{
    /// <summary>
    /// Gets all suppliers asynchronously
    /// </summary>
    Task<IEnumerable<Supplier>> GetAllAsync();
    
    /// <summary>
    /// Gets a supplier by its identifier asynchronously
    /// </summary>
    /// <param name="id">Supplier identifier</param>
    Task<Supplier?> GetByIdAsync(long id);
    
    /// <summary>
    /// Adds a new supplier asynchronously
    /// </summary>
    /// <param name="supplier">Supplier to add</param>
    Task<Supplier> AddAsync(Supplier supplier);
    
    /// <summary>
    /// Updates an existing supplier asynchronously
    /// </summary>
    /// <param name="supplier">Supplier to update</param>
    Task UpdateAsync(Supplier supplier);
    
    /// <summary>
    /// Deletes a supplier asynchronously (soft delete)
    /// </summary>
    /// <param name="id">Supplier identifier</param>
    Task DeleteAsync(long id);
    
    /// <summary>
    /// Checks if a supplier exists asynchronously
    /// </summary>
    /// <param name="id">Supplier identifier</param>
    Task<bool> ExistsAsync(long id);
    
    /// <summary>
    /// Gets all active suppliers asynchronously
    /// </summary>
    Task<IEnumerable<Supplier>> GetActiveAsync();
    
    /// <summary>
    /// Searches suppliers by company name, email, mobile, etc. asynchronously
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    Task<IEnumerable<Supplier>> SearchSuppliersAsync(string searchTerm);
    
    /// <summary>
    /// Gets supplier by email asynchronously
    /// </summary>
    /// <param name="email">Email address</param>
    Task<Supplier?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets total count of active suppliers asynchronously
    /// </summary>
    Task<int> GetTotalCountAsync();
}