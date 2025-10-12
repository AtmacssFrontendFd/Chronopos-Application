namespace ChronoPos.Application.Models;

/// <summary>
/// Represents a cached permission for a user
/// Stored in memory for fast permission checks
/// </summary>
public class CachedPermission
{
    /// <summary>
    /// Permission ID
    /// </summary>
    public int PermissionId { get; set; }

    /// <summary>
    /// Permission code (e.g., "STOCK_MANAGEMENT", "GRN.CREATE")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Permission name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Screen name (single value, not comma-separated)
    /// </summary>
    public string? ScreenName { get; set; }

    /// <summary>
    /// Type matrix (single value, not comma-separated)
    /// </summary>
    public string? TypeMatrix { get; set; }

    /// <summary>
    /// Whether this is a parent permission
    /// </summary>
    public bool IsParent { get; set; }

    /// <summary>
    /// Parent permission ID if this is a child
    /// </summary>
    public int? ParentPermissionId { get; set; }
}

/// <summary>
/// Represents user's permission cache
/// Structure: ScreenName -> TypeMatrix -> List of Permissions
/// </summary>
public class UserPermissionCache
{
    /// <summary>
    /// User ID this cache belongs to
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User's role ID
    /// </summary>
    public int? RoleId { get; set; }

    /// <summary>
    /// User's role name
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// All permissions for the user
    /// Key: Permission Code (e.g., "GRN.CREATE")
    /// Value: CachedPermission object
    /// </summary>
    public Dictionary<string, CachedPermission> AllPermissions { get; set; } = new();

    /// <summary>
    /// Screen-based permission lookup
    /// Key: Screen Name (e.g., "StockManagement")
    /// Value: Dictionary of TypeMatrix -> List of Permissions
    /// </summary>
    public Dictionary<string, Dictionary<string, List<CachedPermission>>> ScreenPermissions { get; set; } = new();

    /// <summary>
    /// When this cache was created
    /// </summary>
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cache expiration time (default 1 hour)
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(1);

    /// <summary>
    /// Checks if cache is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    /// <summary>
    /// Checks if user has a specific permission by code
    /// </summary>
    public bool HasPermission(string permissionCode)
    {
        return AllPermissions.ContainsKey(permissionCode.ToUpperInvariant());
    }

    /// <summary>
    /// Checks if user has permission for a screen and type matrix
    /// Supports wildcard: "-- All Screens --" and "-- All Operations --"
    /// </summary>
    public bool HasPermission(string screenName, string typeMatrix)
    {
        // Check if user has "-- All Screens --" permission (universal screen access)
        if (ScreenPermissions.TryGetValue(Constants.ScreenNames.ALL_SCREENS, out var allScreensDict))
        {
            // If user has "-- All Operations --" for "-- All Screens --", grant access
            if (allScreensDict.ContainsKey(Constants.TypeMatrix.ALL_OPERATIONS))
                return true;

            // If user has the specific operation for "-- All Screens --", grant access
            if (allScreensDict.ContainsKey(typeMatrix))
                return true;
        }

        // Check if user has permission for the specific screen
        if (ScreenPermissions.TryGetValue(screenName, out var typeMatrixDict))
        {
            // If user has "-- All Operations --" for this screen, grant access
            if (typeMatrixDict.ContainsKey(Constants.TypeMatrix.ALL_OPERATIONS))
                return true;

            // Check for specific operation
            return typeMatrixDict.ContainsKey(typeMatrix);
        }

        return false;
    }

    /// <summary>
    /// Gets all permissions for a specific screen
    /// Includes "-- All Screens --" permissions if applicable
    /// </summary>
    public List<CachedPermission> GetScreenPermissions(string screenName)
    {
        var permissions = new List<CachedPermission>();

        // Add "-- All Screens --" permissions if they exist
        if (ScreenPermissions.TryGetValue(Constants.ScreenNames.ALL_SCREENS, out var allScreensDict))
        {
            permissions.AddRange(allScreensDict.Values.SelectMany(p => p));
        }

        // Add specific screen permissions
        if (ScreenPermissions.TryGetValue(screenName, out var typeMatrixDict))
        {
            permissions.AddRange(typeMatrixDict.Values.SelectMany(p => p));
        }

        return permissions.Distinct().ToList();
    }

    /// <summary>
    /// Gets all type matrix values user has for a specific screen
    /// If user has "-- All Operations --", returns all available operations
    /// </summary>
    public List<string> GetScreenTypeMatrixValues(string screenName)
    {
        // Check if user has "-- All Screens --" permission
        if (ScreenPermissions.TryGetValue(Constants.ScreenNames.ALL_SCREENS, out var allScreensDict))
        {
            // If "-- All Operations --" is granted for all screens, return all operations
            if (allScreensDict.ContainsKey(Constants.TypeMatrix.ALL_OPERATIONS))
            {
                return Constants.TypeMatrix.GetAllTypeMatrixValues();
            }

            // Otherwise return the specific operations for "-- All Screens --"
            var allScreensOps = allScreensDict.Keys.ToList();
            
            // Also check specific screen permissions
            if (ScreenPermissions.TryGetValue(screenName, out var typeMatrixDict))
            {
                allScreensOps.AddRange(typeMatrixDict.Keys);
                return allScreensOps.Distinct().ToList();
            }
            
            return allScreensOps;
        }

        // Check specific screen permissions
        if (ScreenPermissions.TryGetValue(screenName, out var screenTypeMatrixDict))
        {
            // If "-- All Operations --" is granted for this screen, return all operations
            if (screenTypeMatrixDict.ContainsKey(Constants.TypeMatrix.ALL_OPERATIONS))
            {
                return Constants.TypeMatrix.GetAllTypeMatrixValues();
            }

            return screenTypeMatrixDict.Keys.ToList();
        }

        return new List<string>();
    }

    /// <summary>
    /// Checks if user has any permission for a screen
    /// Returns true if user has "-- All Screens --" permission
    /// </summary>
    public bool HasAnyScreenPermission(string screenName)
    {
        // Check if user has "-- All Screens --" permission (universal access)
        if (ScreenPermissions.ContainsKey(Constants.ScreenNames.ALL_SCREENS))
            return true;

        // Check specific screen permission
        return ScreenPermissions.ContainsKey(screenName);
    }

    /// <summary>
    /// Checks if user has full access to a screen (all CRUD operations)
    /// Returns true if user has "-- All Operations --" for this screen or "-- All Screens --"
    /// </summary>
    public bool HasFullAccess(string screenName)
    {
        // Check if user has "-- All Screens --" with "-- All Operations --"
        if (ScreenPermissions.TryGetValue(Constants.ScreenNames.ALL_SCREENS, out var allScreensDict))
        {
            if (allScreensDict.ContainsKey(Constants.TypeMatrix.ALL_OPERATIONS))
                return true;
        }

        // Check specific screen
        if (!ScreenPermissions.TryGetValue(screenName, out var typeMatrixDict))
            return false;

        // Check if user has "-- All Operations --" for this specific screen
        if (typeMatrixDict.ContainsKey(Constants.TypeMatrix.ALL_OPERATIONS))
            return true;

        // Check if user has all required operations
        var requiredPermissions = new[] { "Create", "Read", "Update", "Delete", "View" };
        return requiredPermissions.All(p => typeMatrixDict.ContainsKey(p));
    }

    /// <summary>
    /// Checks if user has view-only access to a screen
    /// </summary>
    public bool HasViewOnlyAccess(string screenName)
    {
        if (!ScreenPermissions.TryGetValue(screenName, out var typeMatrixDict))
            return false;

        var hasView = typeMatrixDict.ContainsKey("View") || typeMatrixDict.ContainsKey("Read");
        var hasCreate = typeMatrixDict.ContainsKey("Create");
        var hasUpdate = typeMatrixDict.ContainsKey("Update");
        var hasDelete = typeMatrixDict.ContainsKey("Delete");

        return hasView && !hasCreate && !hasUpdate && !hasDelete;
    }
}
