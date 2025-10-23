namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets all entities asynchronously
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Gets an entity by its identifier asynchronously
    /// </summary>
    /// <param name="id">Entity identifier</param>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Adds a new entity asynchronously
    /// </summary>
    /// <param name="entity">Entity to add</param>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity asynchronously
    /// </summary>
    /// <param name="entity">Entity to update</param>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity asynchronously
    /// </summary>
    /// <param name="id">Entity identifier</param>
    Task DeleteAsync(int id);
    
    /// <summary>
    /// Checks if an entity exists asynchronously
    /// </summary>
    /// <param name="id">Entity identifier</param>
    Task<bool> ExistsAsync(int id);
}
