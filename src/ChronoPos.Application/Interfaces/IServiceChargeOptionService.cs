using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ServiceChargeOption operations
/// </summary>
public interface IServiceChargeOptionService
{
    /// <summary>
    /// Gets all service charge options
    /// </summary>
    /// <returns>Collection of service charge option DTOs</returns>
    Task<IEnumerable<ServiceChargeOptionDto>> GetAllAsync();

    /// <summary>
    /// Gets all active service charge options
    /// </summary>
    /// <returns>Collection of active service charge option DTOs</returns>
    Task<IEnumerable<ServiceChargeOptionDto>> GetActiveOptionsAsync();

    /// <summary>
    /// Gets service charge option by ID
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <returns>Service charge option DTO if found</returns>
    Task<ServiceChargeOptionDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets service charge options by service charge type ID
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Collection of service charge option DTOs</returns>
    Task<IEnumerable<ServiceChargeOptionDto>> GetByServiceChargeTypeIdAsync(int serviceChargeTypeId);

    /// <summary>
    /// Gets active service charge options by service charge type ID
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Collection of active service charge option DTOs</returns>
    Task<IEnumerable<ServiceChargeOptionDto>> GetActiveByServiceChargeTypeIdAsync(int serviceChargeTypeId);

    /// <summary>
    /// Gets service charge options by language ID
    /// </summary>
    /// <param name="languageId">Language ID</param>
    /// <returns>Collection of service charge option DTOs</returns>
    Task<IEnumerable<ServiceChargeOptionDto>> GetByLanguageIdAsync(int languageId);

    /// <summary>
    /// Gets service charge option with related entities
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <returns>Service charge option DTO with related entities</returns>
    Task<ServiceChargeOptionDto?> GetByIdWithRelatedAsync(int id);

    /// <summary>
    /// Creates a new service charge option
    /// </summary>
    /// <param name="createDto">Service charge option data</param>
    /// <param name="userId">ID of user creating the option</param>
    /// <returns>Created service charge option DTO</returns>
    Task<ServiceChargeOptionDto> CreateAsync(CreateServiceChargeOptionDto createDto, int? userId = null);

    /// <summary>
    /// Updates an existing service charge option
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <param name="updateDto">Updated service charge option data</param>
    /// <param name="userId">ID of user updating the option</param>
    /// <returns>Updated service charge option DTO</returns>
    Task<ServiceChargeOptionDto> UpdateAsync(int id, UpdateServiceChargeOptionDto updateDto, int? userId = null);

    /// <summary>
    /// Deletes a service charge option (soft delete)
    /// </summary>
    /// <param name="id">Service charge option ID</param>
    /// <param name="userId">ID of user deleting the option</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id, int? userId = null);

    /// <summary>
    /// Checks if option name exists for a specific service charge type
    /// </summary>
    /// <param name="name">Option name to check</param>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <param name="excludeId">Option ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsForTypeAsync(string name, int serviceChargeTypeId, int? excludeId = null);

    /// <summary>
    /// Deletes all options for a specific service charge type
    /// </summary>
    /// <param name="serviceChargeTypeId">Service charge type ID</param>
    /// <returns>Number of deleted options</returns>
    Task<int> DeleteByServiceChargeTypeIdAsync(int serviceChargeTypeId);
}
