using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Permission entity operations
/// </summary>
public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets permission by code
    /// </summary>
    /// <param name="code">Permission code</param>
    /// <returns>Permission entity if found</returns>
    public async Task<Permission?> GetByCodeAsync(string code)
    {
        return await _context.Set<Permission>()
            .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower() && p.DeletedAt == null);
    }

    /// <summary>
    /// Gets all active permissions
    /// </summary>
    /// <returns>Collection of active permissions</returns>
    public async Task<IEnumerable<Permission>> GetActivePermissionsAsync()
    {
        return await _context.Set<Permission>()
            .Where(p => p.Status == "Active" && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if permission code exists (case-insensitive)
    /// </summary>
    /// <param name="code">Permission code to check</param>
    /// <param name="excludeId">Permission ID to exclude from check (for updates)</param>
    /// <returns>True if code exists</returns>
    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Set<Permission>()
            .Where(p => p.Code.ToLower() == code.ToLower() && p.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PermissionId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets all parent permissions
    /// </summary>
    /// <returns>Collection of parent permissions</returns>
    public async Task<IEnumerable<Permission>> GetParentPermissionsAsync()
    {
        return await _context.Set<Permission>()
            .Where(p => p.IsParent && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets child permissions for a parent
    /// </summary>
    /// <param name="parentId">Parent permission ID</param>
    /// <returns>Collection of child permissions</returns>
    public async Task<IEnumerable<Permission>> GetChildPermissionsAsync(int parentId)
    {
        return await _context.Set<Permission>()
            .Where(p => p.ParentPermissionId == parentId && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets permissions by screen name
    /// </summary>
    /// <param name="screenName">Screen name</param>
    /// <returns>Collection of permissions for the screen</returns>
    public async Task<IEnumerable<Permission>> GetByScreenNameAsync(string screenName)
    {
        return await _context.Set<Permission>()
            .Where(p => p.ScreenName == screenName && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets permission with children
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Permission with child permissions</returns>
    public async Task<Permission?> GetPermissionWithChildrenAsync(int permissionId)
    {
        return await _context.Set<Permission>()
            .Include(p => p.ChildPermissions)
            .Include(p => p.ParentPermission)
            .FirstOrDefaultAsync(p => p.PermissionId == permissionId && p.DeletedAt == null);
    }

    /// <summary>
    /// Gets all permissions including related data
    /// </summary>
    /// <returns>Collection of permissions with navigation properties</returns>
    public override async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Set<Permission>()
            .Include(p => p.ChildPermissions)
            .Include(p => p.ParentPermission)
            .Where(p => p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets permission by ID including related data
    /// </summary>
    /// <param name="id">Permission ID</param>
    /// <returns>Permission with navigation properties</returns>
    public override async Task<Permission?> GetByIdAsync(int id)
    {
        return await _context.Set<Permission>()
            .Include(p => p.ChildPermissions)
            .Include(p => p.ParentPermission)
            .FirstOrDefaultAsync(p => p.PermissionId == id && p.DeletedAt == null);
    }
}
