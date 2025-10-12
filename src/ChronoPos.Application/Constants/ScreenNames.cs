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
    public const string TRANSACTIONS = "Transactions";
    public const string MANAGEMENT = "Management";
    public const string RESERVATION = "Reservation";
    public const string ORDER_TABLE = "OrderTable";
    public const string REPORTS = "Reports";
    public const string SETTINGS = "Settings";

    // ==========================================
    // MANAGEMENT SECTION SCREENS
    // ==========================================
    
    public const string STOCK_MANAGEMENT = "StockManagement";
    public const string PRODUCT_MANAGEMENT = "ProductManagement";
    public const string CUSTOMER_MANAGEMENT = "CustomerManagement";
    public const string SUPPLIER_MANAGEMENT = "SupplierManagement";
    public const string PAYMENT_MANAGEMENT = "PaymentManagement";
    public const string SERVICE_MANAGEMENT = "ServiceManagement";
    public const string ADD_OPTIONS = "AddOptions";

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
    // ADD OPTIONS SCREENS
    // ==========================================
    
    public const string BRAND = "Brand";
    public const string CATEGORY = "Category";
    public const string PRODUCT_ATTRIBUTES = "ProductAttributes";
    public const string PRODUCT_COMBINATIONS = "ProductCombinations";
    public const string PRODUCT_GROUPING = "ProductGrouping";
    public const string PRICE_TYPES = "PriceTypes";
    public const string PAYMENT_TYPES = "PaymentTypes";
    public const string TAX_RATES = "TaxRates";
    public const string CUSTOMERS_ADD_OPTIONS = "CustomersAddOptions";
    public const string CUSTOMER_GROUPS = "CustomerGroups";
    public const string SUPPLIERS_ADD_OPTIONS = "SuppliersAddOptions";
    public const string UOM = "UOM";
    public const string SHOP = "Shop";
    public const string DISCOUNTS = "Discounts";

    // ==========================================
    // SETTINGS SCREENS
    // ==========================================
    
    public const string USER_SETTINGS = "UserSettings";
    public const string APPLICATION_SETTINGS = "ApplicationSettings";
    public const string ROLES = "Roles";
    public const string PERMISSIONS = "Permissions";

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
            TRANSACTIONS,
            MANAGEMENT,
            RESERVATION,
            ORDER_TABLE,
            REPORTS,
            SETTINGS,
            
            // Management Section Screens
            STOCK_MANAGEMENT,
            PRODUCT_MANAGEMENT,
            CUSTOMER_MANAGEMENT,
            SUPPLIER_MANAGEMENT,
            PAYMENT_MANAGEMENT,
            SERVICE_MANAGEMENT,
            ADD_OPTIONS,
            
            // Stock Management Sub-modules
            INVENTORY,
            STOCK_ADJUSTMENT,
            STOCK_TRANSFER,
            GRN,
            GOODS_RETURN,
            GOODS_REPLACE,
            
            // Add Options Screens
            BRAND,
            CATEGORY,
            PRODUCT_ATTRIBUTES,
            PRODUCT_COMBINATIONS,
            PRODUCT_GROUPING,
            PRICE_TYPES,
            PAYMENT_TYPES,
            TAX_RATES,
            CUSTOMERS_ADD_OPTIONS,
            CUSTOMER_GROUPS,
            SUPPLIERS_ADD_OPTIONS,
            UOM,
            SHOP,
            DISCOUNTS,
            
            // Settings Screens
            USER_SETTINGS,
            APPLICATION_SETTINGS,
            ROLES,
            PERMISSIONS
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
            TRANSACTIONS => "Transactions",
            MANAGEMENT => "Management",
            RESERVATION => "Reservation",
            ORDER_TABLE => "Order Table",
            REPORTS => "Reports",
            SETTINGS => "Settings",
            STOCK_MANAGEMENT => "Stock Management",
            PRODUCT_MANAGEMENT => "Product Management",
            CUSTOMER_MANAGEMENT => "Customer Management",
            SUPPLIER_MANAGEMENT => "Supplier Management",
            PAYMENT_MANAGEMENT => "Payment Management",
            SERVICE_MANAGEMENT => "Service Management",
            ADD_OPTIONS => "Add Options",
            INVENTORY => "Inventory",
            STOCK_ADJUSTMENT => "Stock Adjustment",
            STOCK_TRANSFER => "Stock Transfer",
            GRN => "Goods Received Note (GRN)",
            GOODS_RETURN => "Goods Return",
            GOODS_REPLACE => "Goods Replace",
            BRAND => "Brand",
            CATEGORY => "Category",
            PRODUCT_ATTRIBUTES => "Product Attributes",
            PRODUCT_COMBINATIONS => "Product Combinations",
            PRODUCT_GROUPING => "Product Grouping",
            PRICE_TYPES => "Price Types",
            PAYMENT_TYPES => "Payment Types",
            TAX_RATES => "Tax Rates",
            CUSTOMERS_ADD_OPTIONS => "Customers",
            CUSTOMER_GROUPS => "Customer Groups",
            SUPPLIERS_ADD_OPTIONS => "Suppliers",
            UOM => "Unit of Measure (UOM)",
            SHOP => "Shop",
            DISCOUNTS => "Discounts",
            USER_SETTINGS => "User Settings",
            APPLICATION_SETTINGS => "Application Settings",
            ROLES => "Roles",
            PERMISSIONS => "Permissions",
            _ => screenName
        };
    }
}
