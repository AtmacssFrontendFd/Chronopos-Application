using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for RolePermission entity operations
/// </summary>
public interface IRolePermissionRepository : IRepository<RolePermission>
{
    /// <summary>
    /// Gets all permissions for a specific role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of role permissions</returns>
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId);

    /// <summary>
    /// Gets all roles that have a specific permission
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Collection of role permissions</returns>
    Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId);

    /// <summary>
    /// Checks if a role-permission assignment exists
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>True if assignment exists</returns>
    Task<bool> ExistsAsync(int roleId, int permissionId);

    /// <summary>
    /// Gets active permissions for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of active role permissions</returns>
    Task<IEnumerable<RolePermission>> GetActiveByRoleIdAsync(int roleId);

    /// <summary>
    /// Deletes all permissions for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Task</returns>
    Task DeleteByRoleIdAsync(int roleId);
}
