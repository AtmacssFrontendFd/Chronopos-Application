using ChronoPos.Domain.Entities;

namespace ChronoPos.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User entity if found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <returns>Collection of active users</returns>
    Task<IEnumerable<User>> GetActiveUsersAsync();

    /// <summary>
    /// Checks if email exists (case-insensitive)
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">User ID to exclude from check (for updates)</param>
    /// <returns>True if email exists</returns>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    /// <summary>
    /// Gets users with their role information
    /// </summary>
    /// <returns>Collection of users with roles</returns>
    Task<IEnumerable<User>> GetUsersWithRolesAsync();

    /// <summary>
    /// Gets user by ID with role information
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User with role information</returns>
    Task<User?> GetUserWithRoleAsync(int userId);

    /// <summary>
    /// Gets users by role ID
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of users with the specified role</returns>
    Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
}
