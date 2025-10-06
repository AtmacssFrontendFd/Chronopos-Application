using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for CustomerGroupRelation entity operations
/// </summary>
public interface ICustomerGroupRelationRepository : IRepository<CustomerGroupRelation>
{
    /// <summary>
    /// Gets all relations for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>Collection of customer group relations</returns>
    Task<IEnumerable<CustomerGroupRelation>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets all relations for a specific customer group
    /// </summary>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <returns>Collection of customer group relations</returns>
    Task<IEnumerable<CustomerGroupRelation>> GetByCustomerGroupIdAsync(int customerGroupId);

    /// <summary>
    /// Gets all active relations
    /// </summary>
    /// <returns>Collection of active customer group relations</returns>
    Task<IEnumerable<CustomerGroupRelation>> GetActiveRelationsAsync();

    /// <summary>
    /// Checks if a relation exists between customer and customer group
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <param name="excludeId">Relation ID to exclude from check (for updates)</param>
    /// <returns>True if relation exists</returns>
    Task<bool> RelationExistsAsync(int customerId, int customerGroupId, int? excludeId = null);

    /// <summary>
    /// Gets relation by customer and customer group
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="customerGroupId">Customer group ID</param>
    /// <returns>CustomerGroupRelation if found</returns>
    Task<CustomerGroupRelation?> GetByCustomerAndGroupAsync(int customerId, int customerGroupId);

    /// <summary>
    /// Gets all relations with customer and group details
    /// </summary>
    /// <returns>Collection of relations with navigation properties</returns>
    Task<IEnumerable<CustomerGroupRelation>> GetAllWithDetailsAsync();
}
