namespace ChronoPos.Application.Constants;

/// <summary>
/// Maps screens to their available operations.
/// This ensures that permissions are only created for operations that make sense for each screen.
/// </summary>
public static class ScreenOperationsMap
{
    /// <summary>
    /// Gets the list of available operations for a specific screen.
    /// Returns all operations if screen is not explicitly mapped.
    /// </summary>
    public static List<string> GetAvailableOperations(string screenName)
    {
        // Normalize screen name
        var normalized = screenName?.Trim();
        
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return TypeMatrix.GetAllTypeMatrixValues();
        }

        // Return mapped operations or all operations if not found
        return ScreenToOperationsMap.TryGetValue(normalized, out var operations)
            ? operations
            : TypeMatrix.GetAllTypeMatrixValues();
    }

    /// <summary>
    /// Gets the list of available operations for multiple screens.
    /// Returns intersection of operations available to all selected screens.
    /// </summary>
    public static List<string> GetAvailableOperationsForMultipleScreens(List<string> screenNames)
    {
        if (screenNames == null || screenNames.Count == 0)
        {
            return TypeMatrix.GetAllTypeMatrixValues();
        }

        // If "-- All Screens --" is selected, return all operations
        if (screenNames.Contains(ScreenNames.ALL_SCREENS))
        {
            return TypeMatrix.GetAllTypeMatrixValues();
        }

        // Get intersection of operations available to all screens
        var availableOperations = GetAvailableOperations(screenNames[0]).ToHashSet();
        
        foreach (var screenName in screenNames.Skip(1))
        {
            var ops = GetAvailableOperations(screenName);
            availableOperations.IntersectWith(ops);
        }

        return availableOperations.OrderBy(o => o).ToList();
    }

    /// <summary>
    /// Maps each screen to its available operations.
    /// Screens not in this map will have all operations available.
    /// </summary>
    private static readonly Dictionary<string, List<string>> ScreenToOperationsMap = new()
    {
        // ==========================================
        // MAIN VIEWS (Top-Level Navigation)
        // ==========================================
        
        // Dashboard - View only (no data modification)
        [ScreenNames.DASHBOARD] = new List<string>
        {
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Sales Window - Create sales, view sales
        [ScreenNames.SALES_WINDOW] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Transaction - Full CRUD + special operations
        [ScreenNames.TRANSACTION] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Back Office - View only (navigation screen)
        [ScreenNames.BACK_OFFICE] = new List<string>
        {
            TypeMatrix.VIEW
        },

        // Reservation - Full CRUD
        [ScreenNames.RESERVATION] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Reports - View, Export, Print only
        [ScreenNames.REPORTS] = new List<string>
        {
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Settings - View only (navigation screen)
        [ScreenNames.SETTINGS] = new List<string>
        {
            TypeMatrix.VIEW
        },

        // ==========================================
        // MANAGEMENT SECTION SCREENS
        // ==========================================
        
        // Stock Management - View only (navigation screen)
        [ScreenNames.STOCK_MANAGEMENT] = new List<string>
        {
            TypeMatrix.VIEW
        },

        // Product Management - Full CRUD + Import/Export
        [ScreenNames.PRODUCT_MANAGEMENT] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.IMPORT,
            TypeMatrix.PRINT
        },

        // Customer Management - View only (navigation screen)
        [ScreenNames.CUSTOMER_MANAGEMENT] = new List<string>
        {
            TypeMatrix.VIEW
        },

        // Supplier Management - View only (navigation screen)
        [ScreenNames.SUPPLIER_MANAGEMENT] = new List<string>
        {
            TypeMatrix.VIEW
        },

        // ==========================================
        // STOCK MANAGEMENT SUB-MODULES
        // ==========================================
        
        // Inventory - View, Export only
        [ScreenNames.INVENTORY] = new List<string>
        {
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Stock Adjustment - Full CRUD + Import/Export
        [ScreenNames.STOCK_ADJUSTMENT] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.IMPORT,
            TypeMatrix.PRINT
        },

        // Stock Transfer - Full CRUD
        [ScreenNames.STOCK_TRANSFER] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // GRN (Goods Received Note) - Full CRUD
        [ScreenNames.GRN] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Goods Return - Full CRUD
        [ScreenNames.GOODS_RETURN] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Goods Replace - Full CRUD
        [ScreenNames.GOODS_REPLACE] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // ==========================================
        // OTHERS SCREENS (Add Options)
        // ==========================================
        
        // Brand - Full CRUD + Import/Export
        [ScreenNames.BRAND] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.IMPORT,
            TypeMatrix.PRINT
        },

        // Category - Full CRUD + Import/Export
        [ScreenNames.CATEGORY] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.IMPORT,
            TypeMatrix.PRINT
        },

        // Product Attributes - Full CRUD
        [ScreenNames.PRODUCT_ATTRIBUTES] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Product Modifiers - Full CRUD
        [ScreenNames.PRODUCT_MODIFIERS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Product Combinations - Full CRUD
        [ScreenNames.PRODUCT_COMBINATIONS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Product Grouping - Full CRUD
        [ScreenNames.PRODUCT_GROUPING] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Price Types - Full CRUD
        [ScreenNames.PRICE_TYPES] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Payment Types - Full CRUD
        [ScreenNames.PAYMENT_TYPES] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Tax Rates - Full CRUD
        [ScreenNames.TAX_RATES] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // UOM - Full CRUD
        [ScreenNames.UOM] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Shop - Full CRUD
        [ScreenNames.SHOP] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Discounts - Full CRUD
        [ScreenNames.DISCOUNTS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Currency - Full CRUD
        [ScreenNames.CURRENCY] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // ==========================================
        // CUSTOMER & SUPPLIER SCREENS
        // ==========================================

        // Customers - Full CRUD + Import/Export
        [ScreenNames.CUSTOMERS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.IMPORT,
            TypeMatrix.PRINT
        },

        // Customer Groups - Full CRUD
        [ScreenNames.CUSTOMER_GROUPS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Suppliers - Full CRUD + Import/Export
        [ScreenNames.SUPPLIERS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.IMPORT,
            TypeMatrix.PRINT
        },

        // ==========================================
        // SETTINGS SCREENS
        // ==========================================
        
        // Client Settings - Full CRUD
        [ScreenNames.CLIENT_SETTINGS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Global Settings - Full CRUD
        [ScreenNames.GLOBAL_SETTINGS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Add Options - View only (navigation screen)
        [ScreenNames.ADD_OPTIONS] = new List<string>
        {
            TypeMatrix.VIEW
        },

        // Roles - Full CRUD
        [ScreenNames.ROLES] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Permissions - Full CRUD
        [ScreenNames.PERMISSIONS] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        },

        // Services - Full CRUD
        [ScreenNames.SERVICES] = new List<string>
        {
            TypeMatrix.CREATE,
            TypeMatrix.READ,
            TypeMatrix.UPDATE,
            TypeMatrix.DELETE,
            TypeMatrix.VIEW,
            TypeMatrix.EXPORT,
            TypeMatrix.PRINT
        }
    };

    /// <summary>
    /// Checks if an operation is valid for a given screen.
    /// </summary>
    public static bool IsOperationValidForScreen(string screenName, string operation)
    {
        var availableOperations = GetAvailableOperations(screenName);
        return availableOperations.Contains(operation, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a description of why certain operations are available for a screen.
    /// </summary>
    public static string GetScreenOperationsDescription(string screenName)
    {
        return screenName switch
        {
            ScreenNames.DASHBOARD => "Dashboard is view-only. Users can view dashboard data, export reports, and print.",
            ScreenNames.SALES_WINDOW => "Sales Window allows creating new sales and viewing sales history.",
            ScreenNames.REPORTS => "Reports screen is for viewing, exporting, and printing reports only.",
            ScreenNames.BACK_OFFICE => "Back Office is a navigation screen with view-only access.",
            ScreenNames.STOCK_MANAGEMENT => "Stock Management is a navigation screen with view-only access.",
            ScreenNames.CUSTOMER_MANAGEMENT => "Customer Management is a navigation screen with view-only access.",
            ScreenNames.SUPPLIER_MANAGEMENT => "Supplier Management is a navigation screen with view-only access.",
            ScreenNames.SETTINGS => "Settings is a navigation screen with view-only access.",
            ScreenNames.ADD_OPTIONS => "Add Options is a navigation screen with view-only access.",
            ScreenNames.INVENTORY => "Inventory is view-only for checking stock levels.",
            _ => $"Full CRUD operations available for {ScreenNames.GetDisplayName(screenName)}."
        };
    }
}
