using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Unit of Measurement operations
/// </summary>
public interface IUomRepository : IRepository<UnitOfMeasurement>
{
    /// <summary>
    /// Gets a UOM by its identifier asynchronously (long version)
    /// </summary>
    /// <param name="id">UOM identifier</param>
    Task<UnitOfMeasurement?> GetByIdAsync(long id);
    
    /// <summary>
    /// Checks if a UOM exists asynchronously (long version)
    /// </summary>
    /// <param name="id">UOM identifier</param>
    Task<bool> ExistsAsync(long id);
    
    /// <summary>
    /// Deletes a UOM asynchronously (long version)
    /// </summary>
    /// <param name="id">UOM identifier</param>
    Task DeleteAsync(long id);
    
    /// <summary>
    /// Gets all active UOMs asynchronously
    /// </summary>
    Task<IEnumerable<UnitOfMeasurement>> GetActiveUomsAsync();
    
    /// <summary>
    /// Gets UOMs by type (Base or Derived) asynchronously
    /// </summary>
    /// <param name="type">UOM type (Base or Derived)</param>
    Task<IEnumerable<UnitOfMeasurement>> GetUomsByTypeAsync(string type);
    
    /// <summary>
    /// Gets UOMs by category title asynchronously
    /// </summary>
    /// <param name="categoryTitle">Category title</param>
    Task<IEnumerable<UnitOfMeasurement>> GetUomsByCategoryAsync(string categoryTitle);
    
    /// <summary>
    /// Gets all derived UOMs for a base UOM asynchronously
    /// </summary>
    /// <param name="baseUomId">Base UOM ID</param>
    Task<IEnumerable<UnitOfMeasurement>> GetDerivedUomsAsync(long baseUomId);
    
    /// <summary>
    /// Gets a UOM by its abbreviation asynchronously
    /// </summary>
    /// <param name="abbreviation">UOM abbreviation</param>
    Task<UnitOfMeasurement?> GetByAbbreviationAsync(string abbreviation);
    
    /// <summary>
    /// Checks if a UOM name exists asynchronously
    /// </summary>
    /// <param name="name">UOM name</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null);
    
    /// <summary>
    /// Checks if a UOM abbreviation exists asynchronously
    /// </summary>
    /// <param name="abbreviation">UOM abbreviation</param>
    /// <param name="excludeId">ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByAbbreviationAsync(string abbreviation, long? excludeId = null);
    
    /// <summary>
    /// Soft deletes a UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="deletedBy">User ID performing the deletion</param>
    Task SoftDeleteAsync(long id, int deletedBy);
    
    /// <summary>
    /// Restores a soft-deleted UOM asynchronously
    /// </summary>
    /// <param name="id">UOM ID</param>
    /// <param name="restoredBy">User ID performing the restoration</param>
    Task RestoreAsync(long id, int restoredBy);
    
    /// <summary>
    /// Gets UOMs with pagination support asynchronously
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <param name="includeInactive">Include inactive UOMs</param>
    Task<(IEnumerable<UnitOfMeasurement> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string? searchTerm = null, 
        bool includeInactive = false);
}