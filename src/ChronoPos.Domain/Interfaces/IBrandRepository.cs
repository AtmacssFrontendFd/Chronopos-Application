using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Brand entity operations
/// </summary>
public interface IBrandRepository : IRepository<Brand>
{
    /// <summary>
    /// Gets brand by name
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <returns>Brand entity if found</returns>
    Task<Brand?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all active brands
    /// </summary>
    /// <returns>Collection of active brands</returns>
    Task<IEnumerable<Brand>> GetActiveBrandsAsync();

    /// <summary>
    /// Checks if brand name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Brand name to check</param>
    /// <param name="excludeId">Brand ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Gets brands with their product count
    /// </summary>
    /// <returns>Collection of brands with product counts</returns>
    Task<IEnumerable<Brand>> GetBrandsWithProductCountAsync();
}
