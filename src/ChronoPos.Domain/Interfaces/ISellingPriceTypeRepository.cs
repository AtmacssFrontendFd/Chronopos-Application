using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for SellingPriceType entity
/// </summary>
public interface ISellingPriceTypeRepository : IRepository<SellingPriceType>
{
    /// <summary>
    /// Gets all active selling price types
    /// </summary>
    Task<IEnumerable<SellingPriceType>> GetActiveAsync();

    /// <summary>
    /// Gets a selling price type by name
    /// </summary>
    /// <param name="typeName">The type name to search for</param>
    Task<SellingPriceType?> GetByNameAsync(string typeName);

    /// <summary>
    /// Checks if a selling price type name already exists
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    Task<bool> ExistsAsync(string typeName, long? excludeId = null);

    /// <summary>
    /// Soft delete a selling price type
    /// </summary>
    /// <param name="id">The ID of the selling price type to delete</param>
    /// <param name="deletedBy">ID of the user performing the deletion</param>
    Task SoftDeleteAsync(long id, long deletedBy);

    /// <summary>
    /// Gets count of selling price types for dashboard
    /// </summary>
    Task<int> GetCountAsync();
}