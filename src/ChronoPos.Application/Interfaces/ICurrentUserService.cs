using ChronoPos.Application.DTOs;
using ChronoPos.Application.Models;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service to track and manage the currently logged-in user with permission caching
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current logged-in user ID
    /// </summary>
    int? CurrentUserId { get; }

    /// <summary>
    /// Gets the current logged-in user details
    /// </summary>
    UserDto? CurrentUser { get; }

    /// <summary>
    /// Gets the current user's permission cache
    /// </summary>
    UserPermissionCache? PermissionCache { get; }

    /// <summary>
    /// Sets the current logged-in user
    /// </summary>
    Task SetCurrentUserAsync(int userId);

    /// <summary>
    /// Clears the current user (logout)
    /// </summary>
    void ClearCurrentUser();

    /// <summary>
    /// Refreshes the current user's data
    /// </summary>
    Task RefreshCurrentUserAsync();

    /// <summary>
    /// Checks if a user is currently logged in
    /// </summary>
    bool IsUserLoggedIn { get; }

    // ==========================================
    // PERMISSION CHECK METHODS
    // ==========================================

    /// <summary>
    /// Checks if current user has a specific permission by code
    /// </summary>
    bool HasPermission(string permissionCode);

    /// <summary>
    /// Checks if current user has permission for a screen and type matrix
    /// </summary>
    bool HasPermission(string screenName, string typeMatrix);

    /// <summary>
    /// Checks if current user has any permission for a screen
    /// </summary>
    bool HasAnyScreenPermission(string screenName);

    /// <summary>
    /// Gets all permissions for a specific screen
    /// </summary>
    List<CachedPermission> GetScreenPermissions(string screenName);

    /// <summary>
    /// Gets all type matrix values user has for a specific screen
    /// </summary>
    List<string> GetScreenTypeMatrixValues(string screenName);

    /// <summary>
    /// Checks if current user has full access to a screen
    /// </summary>
    bool HasFullAccess(string screenName);

    /// <summary>
    /// Checks if current user has view-only access to a screen
    /// </summary>
    bool HasViewOnlyAccess(string screenName);
}
