using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Role entity operations
/// </summary>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets role by name
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <returns>Role entity if found</returns>
    public async Task<Role?> GetByNameAsync(string roleName)
    {
        return await _context.Set<Role>()
            .FirstOrDefaultAsync(r => r.RoleName.ToLower() == roleName.ToLower() && r.DeletedAt == null);
    }

    /// <summary>
    /// Gets all active roles
    /// </summary>
    /// <returns>Collection of active roles</returns>
    public async Task<IEnumerable<Role>> GetActiveRolesAsync()
    {
        return await _context.Set<Role>()
            .Where(r => r.Status == "Active" && r.DeletedAt == null)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if role name exists (case-insensitive)
    /// </summary>
    /// <param name="roleName">Role name to check</param>
    /// <param name="excludeId">Role ID to exclude from check (for updates)</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string roleName, int? excludeId = null)
    {
        var query = _context.Set<Role>()
            .Where(r => r.RoleName.ToLower() == roleName.ToLower() && r.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.RoleId != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets roles with their permission count
    /// </summary>
    /// <returns>Collection of roles with permission counts</returns>
    public async Task<IEnumerable<Role>> GetRolesWithPermissionCountAsync()
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .Where(r => r.DeletedAt == null)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets role with all its permissions
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Role with permissions</returns>
    public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.DeletedAt == null);
    }

    /// <summary>
    /// Gets all roles including related data
    /// </summary>
    /// <returns>Collection of roles with navigation properties</returns>
    public override async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .Where(r => r.DeletedAt == null)
            .OrderBy(r => r.RoleName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets role by ID including related data
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role with navigation properties</returns>
    public override async Task<Role?> GetByIdAsync(int id)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.RoleId == id && r.DeletedAt == null);
    }
}
