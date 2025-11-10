using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for ServiceChargeOption entity operations
/// </summary>
public interface IServiceChargeOptionRepository : IRepository<ServiceChargeOption>
{
    /// <summary>
    /// Gets service charge options by service charge type ID
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Collection of service charge options</returns>
    Task<IEnumerable<ServiceChargeOption>> GetByServiceChargeTypeIdAsync(int serviceChargeTypeId);

    /// <summary>
    /// Gets active service charge options by service charge type ID
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Collection of active service charge options</returns>
    Task<IEnumerable<ServiceChargeOption>> GetActiveByServiceChargeTypeIdAsync(int serviceChargeTypeId);

    /// <summary>
    /// Gets service charge option by name and type ID
    /// </summary>
    /// <param name="name">Option name</param>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>ServiceChargeOption entity if found</returns>
    Task<ServiceChargeOption?> GetByNameAndTypeAsync(string name, int serviceChargeTypeId);

    /// <summary>
    /// Gets service charge options by language ID
    /// </summary>
    /// <param name="languageId">Language ID</param>
    /// <returns>Collection of service charge options</returns>
    Task<IEnumerable<ServiceChargeOption>> GetByLanguageIdAsync(int languageId);

    /// <summary>
    /// Gets all active service charge options
    /// </summary>
    /// <returns>Collection of active service charge options</returns>
    Task<IEnumerable<ServiceChargeOption>> GetActiveOptionsAsync();

    /// <summary>
    /// Gets service charge option with related entities loaded
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <returns>ServiceChargeOption with related entities loaded</returns>
    Task<ServiceChargeOption?> GetByIdWithRelatedAsync(int id);

    /// <summary>
    /// Checks if option name exists for a specific service charge type (case-insensitive)
    /// </summary>
    /// <param name="name">Option name to check</param>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <param name="excludeId">Option ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsForTypeAsync(string name, int serviceChargeTypeId, int? excludeId = null);

    /// <summary>
    /// Deletes all options for a specific service charge type
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Number of deleted options</returns>
    Task<int> DeleteByServiceChargeTypeIdAsync(int serviceChargeTypeId);
}
