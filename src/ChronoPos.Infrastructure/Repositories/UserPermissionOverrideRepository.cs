using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserPermissionOverride entity operations
/// </summary>
public class UserPermissionOverrideRepository : Repository<UserPermissionOverride>, IUserPermissionOverrideRepository
{
    public UserPermissionOverrideRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all permission overrides for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of user permission overrides</returns>
    public async Task<IEnumerable<UserPermissionOverride>> GetByUserIdAsync(int userId)
    {
        return await _context.Set<UserPermissionOverride>()
            .Include(upo => upo.User)
            .Include(upo => upo.Permission)
            .Where(upo => upo.UserId == userId && upo.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active permission overrides for a user (within valid date range)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Collection of active user permission overrides</returns>
    public async Task<IEnumerable<UserPermissionOverride>> GetActiveByUserIdAsync(int userId)
    {
        var currentDate = DateTime.UtcNow;
        return await _context.Set<UserPermissionOverride>()
            .Include(upo => upo.User)
            .Include(upo => upo.Permission)
            .Where(upo => upo.UserId == userId 
                && upo.DeletedAt == null
                && (upo.ValidFrom == null || upo.ValidFrom <= currentDate)
                && (upo.ValidTo == null || upo.ValidTo >= currentDate))
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a user-permission override exists
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>True if override exists</returns>
    public async Task<bool> ExistsAsync(int userId, int permissionId)
    {
        return await _context.Set<UserPermissionOverride>()
            .AnyAsync(upo => upo.UserId == userId && upo.PermissionId == permissionId && upo.DeletedAt == null);
    }

    /// <summary>
    /// Gets a specific user permission override
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>User permission override if found</returns>
    public async Task<UserPermissionOverride?> GetByUserAndPermissionAsync(int userId, int permissionId)
    {
        return await _context.Set<UserPermissionOverride>()
            .Include(upo => upo.User)
            .Include(upo => upo.Permission)
            .FirstOrDefaultAsync(upo => upo.UserId == userId && upo.PermissionId == permissionId && upo.DeletedAt == null);
    }

    /// <summary>
    /// Gets all users that have overrides for a specific permission
    /// </summary>
    /// <param name="permissionId">Permission ID</param>
    /// <returns>Collection of user permission overrides</returns>
    public async Task<IEnumerable<UserPermissionOverride>> GetByPermissionIdAsync(int permissionId)
    {
        return await _context.Set<UserPermissionOverride>()
            .Include(upo => upo.User)
            .Include(upo => upo.Permission)
            .Where(upo => upo.PermissionId == permissionId && upo.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Deletes all permission overrides for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Task</returns>
    public async Task DeleteByUserIdAsync(int userId)
    {
        var userOverrides = await _context.Set<UserPermissionOverride>()
            .Where(upo => upo.UserId == userId)
            .ToListAsync();

        _context.Set<UserPermissionOverride>().RemoveRange(userOverrides);
    }

    /// <summary>
    /// Gets all user permission overrides including related data
    /// </summary>
    /// <returns>Collection of user permission overrides with navigation properties</returns>
    public override async Task<IEnumerable<UserPermissionOverride>> GetAllAsync()
    {
        return await _context.Set<UserPermissionOverride>()
            .Include(upo => upo.User)
            .Include(upo => upo.Permission)
            .Where(upo => upo.DeletedAt == null)
            .ToListAsync();
    }

    /// <summary>
    /// Gets user permission override by ID including related data
    /// </summary>
    /// <param name="id">User Permission Override ID</param>
    /// <returns>User permission override with navigation properties</returns>
    public override async Task<UserPermissionOverride?> GetByIdAsync(int id)
    {
        return await _context.Set<UserPermissionOverride>()
            .Include(upo => upo.User)
            .Include(upo => upo.Permission)
            .FirstOrDefaultAsync(upo => upo.UserPermissionOverrideId == id && upo.DeletedAt == null);
    }
}
