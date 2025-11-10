using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ServiceChargeType operations
/// </summary>
public interface IServiceChargeTypeService
{
    /// <summary>
    /// Gets all service charge types
    /// </summary>
    /// <returns>Collection of service charge type DTOs</returns>
    Task<IEnumerable<ServiceChargeTypeDto>> GetAllAsync();

    /// <summary>
    /// Gets all active service charge types
    /// </summary>
    /// <returns>Collection of active service charge type DTOs</returns>
    Task<IEnumerable<ServiceChargeTypeDto>> GetActiveTypesAsync();

    /// <summary>
    /// Gets service charge type by ID
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <returns>Service charge type DTO if found</returns>
    Task<ServiceChargeTypeDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets service charge type by code
    /// </summary>
    /// <param name="code">Service charge type code</param>
    /// <returns>Service charge type DTO if found</returns>
    Task<ServiceChargeTypeDto?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets service charge type by name
    /// </summary>
    /// <param name="name">Service charge type name</param>
    /// <returns>Service charge type DTO if found</returns>
    Task<ServiceChargeTypeDto?> GetByNameAsync(string name);

    /// <summary>
    /// Gets the default service charge type
    /// </summary>
    /// <returns>Default service charge type DTO if found</returns>
    Task<ServiceChargeTypeDto?> GetDefaultTypeAsync();

    /// <summary>
    /// Gets service charge types with their options
    /// </summary>
    /// <returns>Collection of service charge type DTOs with options</returns>
    Task<IEnumerable<ServiceChargeTypeDto>> GetTypesWithOptionsAsync();

    /// <summary>
    /// Gets service charge type with its options by ID
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <returns>Service charge type DTO with options</returns>
    Task<ServiceChargeTypeDto?> GetByIdWithOptionsAsync(int id);

    /// <summary>
    /// Creates a new service charge type
    /// </summary>
    /// <param name="createDto">Service charge type data</param>
    /// <param name="userId">ID of user creating the type</param>
    /// <returns>Created service charge type DTO</returns>
    Task<ServiceChargeTypeDto> CreateAsync(CreateServiceChargeTypeDto createDto, int? userId = null);

    /// <summary>
    /// Updates an existing service charge type
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <param name="updateDto">Updated service charge type data</param>
    /// <param name="userId">ID of user updating the type</param>
    /// <returns>Updated service charge type DTO</returns>
    Task<ServiceChargeTypeDto> UpdateAsync(int id, UpdateServiceChargeTypeDto updateDto, int? userId = null);

    /// <summary>
    /// Deletes a service charge type (soft delete)
    /// </summary>
    /// <param name="id">Service charge type ID</param>
    /// <param name="userId">ID of user deleting the type</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id, int? userId = null);

    /// <summary>
    /// Checks if service charge type code exists
    /// </summary>
    /// <param name="code">Code to check</param>
    /// <param name="excludeId">Service charge type ID to exclude from check</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Checks if service charge type name exists
    /// </summary>
    /// <param name="name">Name to check</param>
    /// <param name="excludeId">Service charge type ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Gets service charge types with their options count
    /// </summary>
    /// <returns>Collection of service charge types with options counts</returns>
    Task<IEnumerable<ServiceChargeTypeDto>> GetTypesWithOptionsCountAsync();
}
