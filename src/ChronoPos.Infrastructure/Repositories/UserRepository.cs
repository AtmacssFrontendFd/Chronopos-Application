using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for User entity operations
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ChronoPosDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User entity if found</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.Deleted);
    }

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <returns>Collection of active users</returns>
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _context.Set<User>()
            .Where(u => !u.Deleted)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if email exists (case-insensitive)
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">User ID to exclude from check (for updates)</param>
    /// <returns>True if email exists</returns>
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        var query = _context.Set<User>()
            .Where(u => u.Email.ToLower() == email.ToLower() && !u.Deleted);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Gets users with their role information
    /// </summary>
    /// <returns>Collection of users with roles</returns>
    public async Task<IEnumerable<User>> GetUsersWithRolesAsync()
    {
        return await _context.Set<User>()
            .Where(u => !u.Deleted)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets user by ID with role information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User with role information</returns>
    public async Task<User?> GetUserWithRoleAsync(int userId)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted);
    }

    /// <summary>
    /// Gets users by role ID
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of users with the specified role</returns>
    public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
    {
        return await _context.Set<User>()
            .Where(u => u.RolePermissionId == roleId && !u.Deleted)
            .OrderBy(u => u.FullName)
            .ToListAsync();
    }
}
