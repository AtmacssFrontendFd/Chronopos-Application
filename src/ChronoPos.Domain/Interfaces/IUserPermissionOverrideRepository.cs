using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for UserPermissionOverride entity operations
/// </summary>
public interface IUserPermissionOverrideRepository : IRepository<UserPermissionOverride>
{
    /// <summary>
    /// Gets all permission overrides for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of user permission overrides</returns>
    Task<IEnumerable<UserPermissionOverride>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Gets active permission overrides for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of active overrides</returns>
    Task<IEnumerable<UserPermissionOverride>> GetActiveByUserIdAsync(int userId);

    /// <summary>
    /// Checks if a user has an override for a specific permission
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>True if override exists</returns>
    Task<bool> ExistsAsync(int userId, int permissionId);

    /// <summary>
    /// Gets a specific user permission override
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>User permission override if found</returns>
    Task<UserPermissionOverride?> GetByUserAndPermissionAsync(int userId, int permissionId);

    /// <summary>
    /// Gets all users with overrides for a specific permission
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Collection of user permission overrides</returns>
    Task<IEnumerable<UserPermissionOverride>> GetByPermissionIdAsync(int permissionId);

    /// <summary>
    /// Deletes all overrides for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Task</returns>
    Task DeleteByUserIdAsync(int userId);
}
