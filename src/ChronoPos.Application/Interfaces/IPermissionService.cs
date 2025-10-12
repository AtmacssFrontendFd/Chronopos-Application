using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for Permission operations
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Gets all permissions
    /// </summary>
    /// <returns>Collection of permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetAllAsync();

    /// <summary>
    /// Gets all active permissions
    /// </summary>
    /// <returns>Collection of active permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetActivePermissionsAsync();

    /// <summary>
    /// Gets permission by ID
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <returns>Permission DTO if found</returns>
    Task<PermissionDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets permission by code
    /// </summary>
    /// <param name="code">Permission code</param>
    /// <returns>Permission DTO if found</returns>
    Task<PermissionDto?> GetByCodeAsync(string code);

    /// <summary>
    /// Creates a new permission
    /// </summary>
    /// <param name="permissionDto">Permission data</param>
    /// <returns>Created permission DTO</returns>
    Task<PermissionDto> CreateAsync(CreatePermissionDto permissionDto);

    /// <summary>
    /// Updates an existing permission
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <param name="permissionDto">Updated permission data</param>
    /// <returns>Updated permission DTO</returns>
    Task<PermissionDto> UpdateAsync(int id, UpdatePermissionDto permissionDto);

    /// <summary>
    /// Deletes a permission
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if permission code exists
    /// </summary>
    /// <param name="code">Permission code</param>
    /// <param name="excludeId">Permission ID to exclude from check</param>
    /// <returns>True if code exists</returns>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    /// <summary>
    /// Gets parent permissions (permissions with no parent)
    /// </summary>
    /// <returns>Collection of parent permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetParentPermissionsAsync();

    /// <summary>
    /// Gets child permissions for a specific parent
    /// </summary>
    /// <param name="parentId">Parent permission ID</param>
    /// <returns>Collection of child permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetChildPermissionsAsync(int parentId);

    /// <summary>
    /// Gets permissions by screen name
    /// </summary>
    /// <param name="screenName">Screen name</param>
    /// <returns>Collection of permission DTOs</returns>
    Task<IEnumerable<PermissionDto>> GetByScreenNameAsync(string screenName);

    /// <summary>
    /// Gets permission with its children
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Permission DTO with children</returns>
    Task<PermissionDto?> GetPermissionWithChildrenAsync(int permissionId);

    /// <summary>
    /// Gets permissions assigned to a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of permission DTOs assigned to the role</returns>
    Task<IEnumerable<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId);
}
