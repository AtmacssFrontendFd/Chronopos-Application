using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Role entity operations
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets role by name
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>Role entity if found</returns>
    Task<Role?> GetByNameAsync(string roleName);

    /// <summary>
    /// Gets all active roles
    /// </summary>
    /// <returns>Collection of active roles</returns>
    Task<IEnumerable<Role>> GetActiveRolesAsync();

    /// <summary>
    /// Checks if role name exists (case-insensitive)
    /// </summary>
    /// <param name="roleName">Role name to check</param>
    /// <param name="excludeId">Role ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    Task<bool> NameExistsAsync(string roleName, int? excludeId = null);

    /// <summary>
    /// Gets roles with their permission count
    /// </summary>
    /// <returns>Collection of roles with permission counts</returns>
    Task<IEnumerable<Role>> GetRolesWithPermissionCountAsync();

    /// <summary>
    /// Gets role with all its permissions
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Role with permissions</returns>
    Task<Role?> GetRoleWithPermissionsAsync(int roleId);
}
