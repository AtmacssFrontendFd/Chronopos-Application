using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for User entity operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>Collection of user DTOs</returns>
    Task<IEnumerable<UserDto>> GetAllAsync();

    /// <summary>
    /// Gets user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User DTO if found</returns>
    Task<UserDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets user by email
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User DTO if found</returns>
    Task<UserDto?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all active users
    /// </summary>
    /// <returns>Collection of active user DTOs</returns>
    Task<IEnumerable<UserDto>> GetActiveUsersAsync();

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="createDto">User creation data</param>
    /// <returns>Created user DTO</returns>
    Task<UserDto> CreateAsync(CreateUserDto createDto);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateDto">User update data</param>
    /// <returns>Updated user DTO</returns>
    Task<UserDto> UpdateAsync(int id, UpdateUserDto updateDto);

    /// <summary>
    /// Deletes a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Checks if email exists
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">User ID to exclude from check</param>
    /// <returns>True if email exists</returns>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);

    /// <summary>
    /// Gets users with their role information
    /// </summary>
    /// <returns>Collection of users with roles</returns>
    Task<IEnumerable<UserDto>> GetUsersWithRolesAsync();

    /// <summary>
    /// Gets users by role ID
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>Collection of users with the specified role</returns>
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId);

    /// <summary>
    /// Updates user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="updatePasswordDto">Password update data</param>
    Task UpdatePasswordAsync(int userId, UpdatePasswordDto updatePasswordDto);

    /// <summary>
    /// Validates user credentials
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>User DTO if credentials are valid</returns>
    Task<UserDto?> ValidateCredentialsAsync(string email, string password);
}
