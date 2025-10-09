using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RolePermission entity operations
/// </summary>
public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all permissions for a specific role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of role permissions</returns>
    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(int roleId)
    {
        return await _context.Set<RolePermission>()
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.RoleId == roleId && rp.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all roles that have a specific permission
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Collection of role permissions</returns>
    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(int permissionId)
    {
        return await _context.Set<RolePermission>()
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.PermissionId == permissionId && rp.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a role-permission assignment exists
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>True if assignment exists</returns>
    public async Task<bool> ExistsAsync(int roleId, int permissionId)
    {
        return await _context.Set<RolePermission>()
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && rp.DeletedAt == null);
    }

    /// <summary>
    /// Gets active permissions for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of active role permissions</returns>
    public async Task<IEnumerable<RolePermission>> GetActiveByRoleIdAsync(int roleId)
    {
        return await _context.Set<RolePermission>()
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.RoleId == roleId && rp.Status == "Active" && rp.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes all permissions for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByRoleIdAsync(int roleId)
    {
        var rolePermissions = await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        _context.Set<RolePermission>().RemoveRange(rolePermissions);
    }

    /// <summary>
    /// Gets all role permissions including related data
    /// </summary>
    /// <returns>Collection of role permissions with navigation properties</returns>
    public override async Task<IEnumerable<RolePermission>> GetAllAsync()
    {
        return await _context.Set<RolePermission>()
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .Where(rp => rp.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Gets role permission by ID including related data
    /// </summary>
    /// <param name="id">Role Permission ID</param>
    /// <returns>Role permission with navigation properties</returns>
    public override async Task<RolePermission?> GetByIdAsync(int id)
    {
        return await _context.Set<RolePermission>()
            .Include(rp => rp.Permission)
            .Include(rp => rp.Role)
            .FirstOrDefaultAsync(rp => rp.RolePermissionId == id && rp.DeletedAt == null);
    }
}
