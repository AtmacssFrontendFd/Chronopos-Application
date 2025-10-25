namespace ChronoPos.Application.Constants;

/// <summary>
/// Defines all valid screen names for permission system.
/// These constants ensure consistency across the application.
/// </summary>
public static class ScreenNames
{
    // ==========================================
    // UI HELPER CONSTANTS (Not stored in DB)
    // ==========================================
    
    /// <summary>
    /// UI display constant for selecting all screens in dropdown.
    /// This is NOT stored in database - it expands to all individual screens.
    /// </summary>
    public const string ALL_SCREENS = "-- All Screens --";

    // ==========================================
    // MAIN VIEWS (Top-Level Navigation)
    // ==========================================
    
    public const string DASHBOARD = "Dashboard";
    public const string SALES_WINDOW = "SalesWindow";  // Previously TRANSACTIONS
    public const string TRANSACTION = "Transaction";    // Transaction history
    public const string BACK_OFFICE = "BackOffice";     // Previously MANAGEMENT
    public const string RESERVATION = "Reservation";
    public const string REPORTS = "Reports";
    public const string SETTINGS = "Settings";
    
    // Legacy constants (for backward compatibility - deprecated)
    [Obsolete("Use SALES_WINDOW instead")]
    public const string TRANSACTIONS = "Transactions";
    [Obsolete("Use BACK_OFFICE instead")]
    public const string MANAGEMENT = "Management";

    // ==========================================
    // MANAGEMENT SECTION SCREENS
    // ==========================================
    
    public const string STOCK_MANAGEMENT = "StockManagement";
    public const string PRODUCT_MANAGEMENT = "ProductManagement";
    public const string CUSTOMER_MANAGEMENT = "CustomerManagement";
    public const string SUPPLIER_MANAGEMENT = "SupplierManagement";

    // ==========================================
    // STOCK MANAGEMENT SUB-MODULES (Tabs)
    // ==========================================
    
    public const string INVENTORY = "Inventory";
    public const string STOCK_ADJUSTMENT = "StockAdjustment";
    public const string STOCK_TRANSFER = "StockTransfer";
    public const string GRN = "GRN";
    public const string GOODS_RETURN = "GoodsReturn";
    public const string GOODS_REPLACE = "GoodsReplace";

    // ==========================================
    // OTHERS SCREENS (Formerly "Add Options")
    // ==========================================
    
    public const string BRAND = "Brand";
    public const string CATEGORY = "Category";
    public const string PRODUCT_ATTRIBUTES = "ProductAttributes";
    public const string PRODUCT_MODIFIERS = "ProductModifiers";
    public const string PRODUCT_COMBINATIONS = "ProductCombinations";
    public const string PRODUCT_GROUPING = "ProductGrouping";
    public const string PRICE_TYPES = "PriceTypes";
    public const string PAYMENT_TYPES = "PaymentTypes";
    public const string TAX_RATES = "TaxRates";
    public const string UOM = "UOM";
    public const string SHOP = "Shop";
    public const string DISCOUNTS = "Discounts";
    public const string CURRENCY = "Currency";
    
    // ==========================================
    // CUSTOMER & SUPPLIER SCREENS
    // ==========================================
    
    public const string CUSTOMERS = "Customers";
    public const string CUSTOMER_GROUPS = "CustomerGroups";
    public const string SUPPLIERS = "Suppliers";

    // ==========================================
    // SETTINGS SCREENS
    // ==========================================
    
    public const string CLIENT_SETTINGS = "ClientSettings";        // Previously USER_SETTINGS
    public const string GLOBAL_SETTINGS = "GlobalSettings";        // Previously APPLICATION_SETTINGS
    public const string ADD_OPTIONS = "AddOptions";                // Moved from Management, displays as "Others"
    public const string ROLES = "Roles";
    public const string PERMISSIONS = "Permissions";
    public const string SERVICES = "Services";                     // New Services screen
    
    // Legacy constants (for backward compatibility - deprecated)
    [Obsolete("Use CLIENT_SETTINGS instead")]
    public const string USER_SETTINGS = "UserSettings";
    [Obsolete("Use GLOBAL_SETTINGS instead")]
    public const string APPLICATION_SETTINGS = "ApplicationSettings";

    // ==========================================
    // HELPER: Get All Screen Names
    // ==========================================
    
    /// <summary>
    /// Returns all available screen names as a list
    /// </summary>
    public static List<string> GetAllScreenNames()
    {
        return new List<string>
        {
            // Main Views
            DASHBOARD,
            SALES_WINDOW,
            TRANSACTION,
            BACK_OFFICE,
            RESERVATION,
            REPORTS,
            SETTINGS,
            
            // Management Section Screens
            STOCK_MANAGEMENT,
            PRODUCT_MANAGEMENT,
            CUSTOMER_MANAGEMENT,
            SUPPLIER_MANAGEMENT,
            
            // Stock Management Sub-modules
            INVENTORY,
            STOCK_ADJUSTMENT,
            STOCK_TRANSFER,
            GRN,
            GOODS_RETURN,
            GOODS_REPLACE,
            
            // Others Screens (Formerly Add Options)
            BRAND,
            CATEGORY,
            PRODUCT_ATTRIBUTES,
            PRODUCT_MODIFIERS,
            PRODUCT_COMBINATIONS,
            PRODUCT_GROUPING,
            PRICE_TYPES,
            PAYMENT_TYPES,
            TAX_RATES,
            UOM,
            SHOP,
            DISCOUNTS,
            CURRENCY,
            
            // Customer & Supplier Screens
            CUSTOMERS,
            CUSTOMER_GROUPS,
            SUPPLIERS,
            
            // Settings Screens
            CLIENT_SETTINGS,
            GLOBAL_SETTINGS,
            ADD_OPTIONS,
            ROLES,
            PERMISSIONS,
            SERVICES
        };
    }

    /// <summary>
    /// Returns all screen names INCLUDING the "All Screens" UI helper.
    /// Use this for populating dropdowns in the UI.
    /// </summary>
    public static List<string> GetAllScreenNamesWithAllOption()
    {
        var screens = new List<string> { ALL_SCREENS };
        screens.AddRange(GetAllScreenNames());
        return screens;
    }

    /// <summary>
    /// Validates if a screen name is valid
    /// </summary>
    public static bool IsValidScreenName(string screenName)
    {
        return GetAllScreenNames().Contains(screenName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets screen display name for UI
    /// </summary>
    public static string GetDisplayName(string screenName)
    {
        return screenName switch
        {
            ALL_SCREENS => "-- All Screens --",
            DASHBOARD => "Dashboard",
            SALES_WINDOW => "Add Sales",
            TRANSACTION => "Transaction",
            BACK_OFFICE => "Management",
            RESERVATION => "Reservation",
            REPORTS => "Reports",
            SETTINGS => "Settings",
            STOCK_MANAGEMENT => "Stock Management",
            PRODUCT_MANAGEMENT => "Product Management",
            CUSTOMER_MANAGEMENT => "Customer Management",
            SUPPLIER_MANAGEMENT => "Supplier Management",
            INVENTORY => "Inventory",
            STOCK_ADJUSTMENT => "Stock Adjustment",
            STOCK_TRANSFER => "Stock Transfer",
            GRN => "Goods Received Note (GRN)",
            GOODS_RETURN => "Goods Return",
            GOODS_REPLACE => "Goods Replace",
            BRAND => "Brand",
            CATEGORY => "Category",
            PRODUCT_ATTRIBUTES => "Product Attributes",
            PRODUCT_MODIFIERS => "Product Modifiers",
            PRODUCT_COMBINATIONS => "Product Combinations",
            PRODUCT_GROUPING => "Product Groups",
            PRICE_TYPES => "Price Types",
            PAYMENT_TYPES => "Payment Types",
            TAX_RATES => "Tax Rates",
            UOM => "Unit of Measure (UOM)",
            SHOP => "Shop",
            DISCOUNTS => "Discounts",
            CURRENCY => "Currency",
            CUSTOMERS => "Customers",
            CUSTOMER_GROUPS => "Customer Groups",
            SUPPLIERS => "Suppliers",
            CLIENT_SETTINGS => "Client Settings",
            GLOBAL_SETTINGS => "Global Settings",
            ADD_OPTIONS => "Others",
            ROLES => "Roles",
            PERMISSIONS => "Permissions",
            SERVICES => "Services",
            _ => screenName
        };
    }
}
