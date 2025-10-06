using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for CustomerGroupRelation operations
/// </summary>
public interface ICustomerGroupRelationService
{
    /// <summary>
    /// Gets all customer group relations
    /// </summary>
    /// <returns>Collection of customer group relation DTOs</returns>
    Task<IEnumerable<CustomerGroupRelationDto>> GetAllAsync();

    /// <summary>
    /// Gets all active customer group relations
    /// </summary>
    /// <returns>Collection of active customer group relation DTOs</returns>
    Task<IEnumerable<CustomerGroupRelationDto>> GetActiveRelationsAsync();

    /// <summary>
    /// Gets customer group relation by ID
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <returns>Customer group relation DTO if found</returns>
    Task<CustomerGroupRelationDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all relations for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of customer group relation DTOs</returns>
    Task<IEnumerable<CustomerGroupRelationDto>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets all relations for a specific customer group
    /// </summary>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <returns>Collection of customer group relation DTOs</returns>
    Task<IEnumerable<CustomerGroupRelationDto>> GetByCustomerGroupIdAsync(int customerGroupId);

    /// <summary>
    /// Creates a new customer group relation
    /// </summary>
    /// <param name="relationDto">Relation data</param>
    /// <returns>Created customer group relation DTO</returns>
    Task<CustomerGroupRelationDto> CreateAsync(CreateCustomerGroupRelationDto relationDto);

    /// <summary>
    /// Updates an existing customer group relation
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <param name="relationDto">Updated relation data</param>
    /// <returns>Updated customer group relation DTO</returns>
    Task<CustomerGroupRelationDto> UpdateAsync(int id, UpdateCustomerGroupRelationDto relationDto);

    /// <summary>
    /// Deletes a customer group relation
    /// </summary>
    /// <param name="id">Relation ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a relation exists between customer and customer group
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <param name="excludeId">Relation ID to exclude from check</param>
    /// <returns>True if relation exists</returns>
    Task<bool> RelationExistsAsync(int customerId, int customerGroupId, int? excludeId = null);

    /// <summary>
    /// Gets all relations with customer and group details
    /// </summary>
    /// <returns>Collection of relations with details</returns>
    Task<IEnumerable<CustomerGroupRelationDto>> GetAllWithDetailsAsync();
}
