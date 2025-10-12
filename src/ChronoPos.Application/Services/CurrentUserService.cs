using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Models;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service to track and manage the currently logged-in user with permission caching (UMAC)
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserPermissionOverrideRepository _userPermissionOverrideRepository;

    private int? _currentUserId;
    private UserDto? _currentUser;
    private UserPermissionCache? _permissionCache;

    public CurrentUserService(
        IUserService userService,
        IRoleService roleService,
        IPermissionService permissionService,
        IRolePermissionRepository rolePermissionRepository,
        IUserPermissionOverrideRepository userPermissionOverrideRepository)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        _userPermissionOverrideRepository = userPermissionOverrideRepository ?? throw new ArgumentNullException(nameof(userPermissionOverrideRepository));
    }

    /// <summary>
    /// Gets the current logged-in user ID
    /// </summary>
    public int? CurrentUserId => _currentUserId;

    /// <summary>
    /// Gets the current logged-in user details
    /// </summary>
    public UserDto? CurrentUser => _currentUser;

    /// <summary>
    /// Gets the current user's permission cache
    /// </summary>
    public UserPermissionCache? PermissionCache => _permissionCache;

    /// <summary>
    /// Checks if a user is currently logged in
    /// </summary>
    public bool IsUserLoggedIn => _currentUserId.HasValue && _currentUser != null;

    /// <summary>
    /// Sets the current logged-in user and loads their permissions
    /// </summary>
    public async Task SetCurrentUserAsync(int userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {userId} not found");
        }

        _currentUserId = userId;
        _currentUser = user;

        // Load user permissions into cache
        await LoadUserPermissionsAsync(userId);

        ChronoPos.Application.Logging.AppLogger.Log(
            $"CurrentUserService: User set - ID: {userId}, Name: {user.FullName ?? "Unknown"}, Permissions Cached: {_permissionCache?.AllPermissions.Count ?? 0}");
    }

    /// <summary>
    /// Clears the current user and their permission cache (logout)
    /// </summary>
    public void ClearCurrentUser()
    {
        ChronoPos.Application.Logging.AppLogger.Log(
            $"CurrentUserService: Clearing current user (was: {_currentUser?.FullName ?? "None"})");

        _currentUserId = null;
        _currentUser = null;
        _permissionCache = null;
    }

    /// <summary>
    /// Refreshes the current user's data and permissions
    /// </summary>
    public async Task RefreshCurrentUserAsync()
    {
        if (!_currentUserId.HasValue)
        {
            return;
        }

        try
        {
            ChronoPos.Application.Logging.AppLogger.Log(
                $"CurrentUserService: Refreshing user data for ID: {_currentUserId.Value}");

            var user = await _userService.GetByIdAsync(_currentUserId.Value);
            if (user != null)
            {
                _currentUser = user;
                
                // Reload permissions
                await LoadUserPermissionsAsync(_currentUserId.Value);
                
                ChronoPos.Application.Logging.AppLogger.Log(
                    $"CurrentUserService: User data refreshed successfully, Permissions: {_permissionCache?.AllPermissions.Count ?? 0}");
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log(
                $"CurrentUserService: Error refreshing user data - {ex.Message}");
        }
    }

    /// <summary>
    /// Loads all permissions for the user and builds the permission cache
    /// This implements the UMAC (User Management Access Control) caching strategy
    /// </summary>
    private async Task LoadUserPermissionsAsync(int userId)
    {
        try
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Initialize permission cache
            _permissionCache = new UserPermissionCache
            {
                UserId = userId,
                RoleId = user.RolePermissionId,
                CachedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1) // Cache expires in 1 hour
            };

            // Get role permissions
            var rolePermissions = new List<PermissionDto>();
            if (user.RolePermissionId > 0)
            {
                var role = await _roleService.GetByIdAsync(user.RolePermissionId);
                if (role != null)
                {
                    _permissionCache.RoleName = role.RoleName;
                    rolePermissions = (await _permissionService.GetPermissionsByRoleIdAsync(user.RolePermissionId)).ToList();
                }
            }

            // Get user permission overrides
            var userOverrides = await _userPermissionOverrideRepository.GetActiveByUserIdAsync(userId);
            
            // Get all permissions to resolve IDs
            var allPermissions = (await _permissionService.GetAllAsync()).ToList();
            var permissionDict = allPermissions.ToDictionary(p => p.PermissionId, p => p);

            // Build final permission list
            // Start with role permissions
            var finalPermissions = new Dictionary<int, PermissionDto>();
            foreach (var permission in rolePermissions)
            {
                finalPermissions[permission.PermissionId] = permission;
            }

            // Apply user overrides
            foreach (var userOverride in userOverrides)
            {
                if (permissionDict.TryGetValue(userOverride.PermissionId, out var permission))
                {
                    if (userOverride.IsAllowed)
                    {
                        // Grant: Add permission
                        finalPermissions[permission.PermissionId] = permission;
                    }
                    else
                    {
                        // Revoke: Remove permission
                        finalPermissions.Remove(permission.PermissionId);
                    }
                }
            }

            // Build the cache structure
            foreach (var permission in finalPermissions.Values)
            {
                // Create cached permission
                var cachedPermission = new CachedPermission
                {
                    PermissionId = permission.PermissionId,
                    Code = permission.Code,
                    Name = permission.Name,
                    ScreenName = permission.ScreenName,
                    TypeMatrix = permission.TypeMatrix,
                    IsParent = permission.IsParent,
                    ParentPermissionId = permission.ParentPermissionId
                };

                // Add to AllPermissions dictionary by code
                _permissionCache.AllPermissions[permission.Code.ToUpperInvariant()] = cachedPermission;

                // Parse ScreenName and TypeMatrix (comma-separated values)
                var screenNames = ParseCommaSeparated(permission.ScreenName);
                var typeMatrixValues = ParseCommaSeparated(permission.TypeMatrix);

                // Build ScreenPermissions structure
                foreach (var screenName in screenNames)
                {
                    if (string.IsNullOrWhiteSpace(screenName))
                        continue;

                    if (!_permissionCache.ScreenPermissions.ContainsKey(screenName))
                    {
                        _permissionCache.ScreenPermissions[screenName] = new Dictionary<string, List<CachedPermission>>();
                    }

                    foreach (var typeMatrix in typeMatrixValues)
                    {
                        if (string.IsNullOrWhiteSpace(typeMatrix))
                            continue;

                        if (!_permissionCache.ScreenPermissions[screenName].ContainsKey(typeMatrix))
                        {
                            _permissionCache.ScreenPermissions[screenName][typeMatrix] = new List<CachedPermission>();
                        }

                        _permissionCache.ScreenPermissions[screenName][typeMatrix].Add(cachedPermission);
                    }
                }
            }

            ChronoPos.Application.Logging.AppLogger.Log(
                $"CurrentUserService: Loaded {_permissionCache.AllPermissions.Count} permissions for user {userId}, " +
                $"Covering {_permissionCache.ScreenPermissions.Count} screens");
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log(
                $"CurrentUserService: Error loading permissions for user {userId} - {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Parses comma-separated values and returns as list
    /// </summary>
    private List<string> ParseCommaSeparated(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    // ==========================================
    // PERMISSION CHECK METHODS
    // ==========================================

    /// <summary>
    /// Checks if current user has a specific permission by code
    /// </summary>
    public bool HasPermission(string permissionCode)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            ChronoPos.Application.Logging.AppLogger.Log(
                "CurrentUserService: Permission cache is null or expired");
            return false;
        }

        return _permissionCache.HasPermission(permissionCode);
    }

    /// <summary>
    /// Checks if current user has permission for a screen and type matrix
    /// </summary>
    public bool HasPermission(string screenName, string typeMatrix)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            ChronoPos.Application.Logging.AppLogger.Log(
                "CurrentUserService: Permission cache is null or expired");
            return false;
        }

        return _permissionCache.HasPermission(screenName, typeMatrix);
    }

    /// <summary>
    /// Checks if current user has any permission for a screen
    /// </summary>
    public bool HasAnyScreenPermission(string screenName)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            return false;
        }

        return _permissionCache.HasAnyScreenPermission(screenName);
    }

    /// <summary>
    /// Gets all permissions for a specific screen
    /// </summary>
    public List<CachedPermission> GetScreenPermissions(string screenName)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            return new List<CachedPermission>();
        }

        return _permissionCache.GetScreenPermissions(screenName);
    }

    /// <summary>
    /// Gets all type matrix values user has for a specific screen
    /// </summary>
    public List<string> GetScreenTypeMatrixValues(string screenName)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            return new List<string>();
        }

        return _permissionCache.GetScreenTypeMatrixValues(screenName);
    }

    /// <summary>
    /// Checks if current user has full access to a screen
    /// </summary>
    public bool HasFullAccess(string screenName)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            return false;
        }

        return _permissionCache.HasFullAccess(screenName);
    }

    /// <summary>
    /// Checks if current user has view-only access to a screen
    /// </summary>
    public bool HasViewOnlyAccess(string screenName)
    {
        if (_permissionCache == null || _permissionCache.IsExpired)
        {
            return false;
        }

        return _permissionCache.HasViewOnlyAccess(screenName);
    }
}
