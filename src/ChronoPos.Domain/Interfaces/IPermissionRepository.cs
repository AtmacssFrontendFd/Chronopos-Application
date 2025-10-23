using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for Permission entity operations
/// </summary>
public interface IPermissionRepository : IRepository<Permission>
{
    /// <summary>
    /// Gets permission by code
    /// </summary>
    /// <param name="code">Permission code</param>
    /// <returns>Permission entity if found</returns>
    Task<Permission?> GetByCodeAsync(string code);

    /// <summary>
    /// Gets all active permissions
    /// </summary>
    /// <returns>Collection of active permissions</returns>
    Task<IEnumerable<Permission>> GetActivePermissionsAsync();

    /// <summary>
    /// Checks if permission code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Permission code to check</param>
    /// <param name="excludeId">Permission ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Gets all parent permissions
    /// </summary>
    /// <returns>Collection of parent permissions</returns>
    Task<IEnumerable<Permission>> GetParentPermissionsAsync();

    /// <summary>
    /// Gets child permissions for a parent
    /// </summary>
    /// <param name="parentId">Parent permission ID</param>
    /// <returns>Collection of child permissions</returns>
    Task<IEnumerable<Permission>> GetChildPermissionsAsync(int parentId);

    /// <summary>
    /// Gets permissions by screen name
    /// </summary>
    /// <param name="screenName">Screen name</param>
    /// <returns>Collection of permissions for the screen</returns>
    Task<IEnumerable<Permission>> GetByScreenNameAsync(string screenName);

    /// <summary>
    /// Gets permission with children
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Permission with child permissions</returns>
    Task<Permission?> GetPermissionWithChildrenAsync(int permissionId);
}
