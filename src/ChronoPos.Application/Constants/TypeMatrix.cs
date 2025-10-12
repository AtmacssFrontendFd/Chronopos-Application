namespace ChronoPos.Application.Constants;

/// <summary>
/// Defines all valid type matrix (permission actions) for the system.
/// These constants ensure consistency in permission checks.
/// </summary>
public static class TypeMatrix
{
    // ==========================================
    // UI HELPER CONSTANTS (Not stored in DB)
    // ==========================================
    
    /// <summary>
    /// UI display constant for selecting all operations in dropdown.
    /// This is NOT stored in database - it expands to all individual operations.
    /// </summary>
    public const string ALL_OPERATIONS = "-- All Operations --";

    // ==========================================
    // CRUD OPERATIONS
    // ==========================================
    
    /// <summary>
    /// Create new records
    /// </summary>
    public const string CREATE = "Create";
    
    /// <summary>
    /// Read/View records
    /// </summary>
    public const string READ = "Read";
    
    /// <summary>
    /// Update existing records
    /// </summary>
    public const string UPDATE = "Update";
    
    /// <summary>
    /// Delete records
    /// </summary>
    public const string DELETE = "Delete";
    
    /// <summary>
    /// View screen/module (different from Read - this is for screen access)
    /// </summary>
    public const string VIEW = "View";
    
    /// <summary>
    /// Export data to file (Excel, PDF, etc.)
    /// </summary>
    public const string EXPORT = "Export";
    
    /// <summary>
    /// Import data from file
    /// </summary>
    public const string IMPORT = "Import";
    
    /// <summary>
    /// Print reports or documents
    /// </summary>
    public const string PRINT = "Print";

    // ==========================================
    // SPECIAL ACCESS TYPES
    // ==========================================
    
    /// <summary>
    /// Full access to all operations (Admin)
    /// </summary>
    public const string FULL_ACCESS = "FullAccess";
    
    /// <summary>
    /// View only access (no modifications)
    /// </summary>
    public const string VIEW_ONLY = "ViewOnly";
    
    /// <summary>
    /// No access to the screen/module
    /// </summary>
    public const string NO_ACCESS = "NoAccess";

    // ==========================================
    // HELPER METHODS
    // ==========================================
    
    /// <summary>
    /// Returns all available type matrix values
    /// </summary>
    public static List<string> GetAllTypeMatrixValues()
    {
        return new List<string>
        {
            CREATE,
            READ,
            UPDATE,
            DELETE,
            VIEW,
            EXPORT,
            IMPORT,
            PRINT
        };
    }

    /// <summary>
    /// Returns all type matrix values INCLUDING the "All Operations" UI helper.
    /// Use this for populating dropdowns in the UI.
    /// </summary>
    public static List<string> GetAllTypeMatrixValuesWithAllOption()
    {
        var operations = new List<string> { ALL_OPERATIONS };
        operations.AddRange(GetAllTypeMatrixValues());
        return operations;
    }

    /// <summary>
    /// Returns special access types
    /// </summary>
    public static List<string> GetSpecialAccessTypes()
    {
        return new List<string>
        {
            FULL_ACCESS,
            VIEW_ONLY,
            NO_ACCESS
        };
    }

    /// <summary>
    /// Validates if a type matrix value is valid
    /// </summary>
    public static bool IsValidTypeMatrix(string typeMatrix)
    {
        var allTypes = GetAllTypeMatrixValues();
        allTypes.AddRange(GetSpecialAccessTypes());
        return allTypes.Contains(typeMatrix, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all permissions for full access
    /// </summary>
    public static List<string> GetFullAccessPermissions()
    {
        return new List<string>
        {
            CREATE,
            READ,
            UPDATE,
            DELETE,
            VIEW,
            EXPORT,
            IMPORT,
            PRINT
        };
    }

    /// <summary>
    /// Gets all permissions for view only access
    /// </summary>
    public static List<string> GetViewOnlyPermissions()
    {
        return new List<string>
        {
            READ,
            VIEW,
            EXPORT,
            PRINT
        };
    }

    /// <summary>
    /// Gets display name for UI
    /// </summary>
    public static string GetDisplayName(string typeMatrix)
    {
        return typeMatrix switch
        {
            ALL_OPERATIONS => "-- All Operations --",
            CREATE => "Create",
            READ => "Read",
            UPDATE => "Update",
            DELETE => "Delete",
            VIEW => "View",
            EXPORT => "Export",
            IMPORT => "Import",
            PRINT => "Print",
            FULL_ACCESS => "Full Access",
            VIEW_ONLY => "View Only",
            NO_ACCESS => "No Access",
            _ => typeMatrix
        };
    }

    /// <summary>
    /// Gets description for UI tooltip
    /// </summary>
    public static string GetDescription(string typeMatrix)
    {
        return typeMatrix switch
        {
            ALL_OPERATIONS => "Grants permission to all operations on selected screen(s)",
            CREATE => "Permission to create new records",
            READ => "Permission to read/fetch records",
            UPDATE => "Permission to update existing records",
            DELETE => "Permission to delete records",
            VIEW => "Permission to view/access the screen",
            EXPORT => "Permission to export data to file",
            IMPORT => "Permission to import data from file",
            PRINT => "Permission to print reports/documents",
            FULL_ACCESS => "Full access to all operations on this screen",
            VIEW_ONLY => "View only access, cannot make changes",
            NO_ACCESS => "No access to this screen",
            _ => typeMatrix
        };
    }
}
