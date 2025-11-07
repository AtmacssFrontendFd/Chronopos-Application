using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ServiceChargeType entity operations
/// </summary>
public interface IServiceChargeTypeRepository : IRepository<ServiceChargeType>
{
    /// <summary>
    /// Gets service charge type by code
    /// </summary>
    /// <param name="code">Service charge type code</param>
    /// <returns>ServiceChargeType entity if found</returns>
    Task<ServiceChargeType?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets service charge type by name
    /// </summary>
    /// <param name="name">Service charge type name</param>
    /// <returns>ServiceChargeType entity if found</returns>
    Task<ServiceChargeType?> GetByNameAsync(string name);

    /// <summary>
    /// Gets all active service charge types
    /// </summary>
    /// <returns>Collection of active service charge types</returns>
    Task<IEnumerable<ServiceChargeType>> GetActiveTypesAsync();

    /// <summary>
    /// Gets the default service charge type
    /// </summary>
    /// <returns>Default service charge type if found</returns>
    Task<ServiceChargeType?> GetDefaultTypeAsync();

    /// <summary>
    /// Gets service charge types with their options
    /// </summary>
    /// <returns>Collection of service charge types with options loaded</returns>
    Task<IEnumerable<ServiceChargeType>> GetTypesWithOptionsAsync();

    /// <summary>
    /// Gets service charge type with its options by ID
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <returns>ServiceChargeType with options loaded</returns>
    Task<ServiceChargeType?> GetByIdWithOptionsAsync(int id);

    /// <summary>
    /// Checks if service charge type code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Code to check</param>
    /// <param name="excludeId">Service charge type ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Checks if service charge type name exists (case-insensitive)
    /// </summary>
    /// <param name="name">Name to check</param>
    /// <param name="excludeId">Service charge type ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Gets service charge types with their options count
    /// </summary>
    /// <returns>Collection of service charge types with options counts</returns>
    Task<IEnumerable<ServiceChargeType>> GetTypesWithOptionsCountAsync();
}
