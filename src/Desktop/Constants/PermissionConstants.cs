using System.Collections.Generic;

namespace ChronoPos.Desktop.Constants
{
    /// <summary>
    /// Central permission constants for the ChronoPos application.
    /// These constants match the permission codes in the database.
    /// Use these for permission checks throughout the application.
    /// 
    /// HIERARCHY:
    /// Level 1: Views (Main Navigation) - VIEW.*
    /// Level 2: Screens (Pages) - SCREEN.*
    /// Level 3: Sub-Modules (Tabs within screens) - SUBMODULE.*
    /// Level 4: Actions (CRUD operations) - *.CREATE, *.READ, etc.
    /// </summary>
    public static class PermissionConstants
    {
        /// <summary>
        /// Main navigation views (Sidebar items in MainWindow)
        /// </summary>
        public static class Views
        {
            public const string Dashboard = "VIEW.DASHBOARD";
            public const string Transactions = "VIEW.TRANSACTIONS";
            public const string Management = "VIEW.MANAGEMENT";
            public const string Reservation = "VIEW.RESERVATION";
            public const string OrderTable = "VIEW.ORDER_TABLE";
            public const string Reports = "VIEW.REPORTS";
            public const string Settings = "VIEW.SETTINGS";
        }

        /// <summary>
        /// Screens under Management container
        /// </summary>
        public static class ManagementScreens
        {
            public const string Stock = "SCREEN.STOCK_MANAGEMENT";
            public const string Product = "SCREEN.PRODUCT_MANAGEMENT";
            public const string AddOptions = "SCREEN.ADD_OPTIONS";
        }

        /// <summary>
        /// Screens under AddOptions container (14 screens)
        /// </summary>
        public static class AddOptionsScreens
        {
            public const string Brand = "SCREEN.BRAND";
            public const string Category = "SCREEN.CATEGORY";
            public const string ProductAttributes = "SCREEN.PRODUCT_ATTRIBUTES";
            public const string ProductCombinations = "SCREEN.PRODUCT_COMBINATIONS";
            public const string ProductGrouping = "SCREEN.PRODUCT_GROUPING";
            public const string PriceTypes = "SCREEN.PRICE_TYPES";
            public const string PaymentTypes = "SCREEN.PAYMENT_TYPES";
            public const string TaxRates = "SCREEN.TAX_RATES";
            public const string Customers = "SCREEN.CUSTOMERS";
            public const string CustomerGroups = "SCREEN.CUSTOMER_GROUPS";
            public const string Suppliers = "SCREEN.SUPPLIERS";
            public const string Shop = "SCREEN.SHOP";
            public const string Discounts = "SCREEN.DISCOUNTS";
            public const string UOM = "SCREEN.UOM";
        }

        /// <summary>
        /// Screens under Settings container
        /// </summary>
        public static class SettingsScreens
        {
            public const string UserSettings = "SCREEN.USER_SETTINGS";
            public const string ApplicationSettings = "SCREEN.APP_SETTINGS";
            public const string Roles = "SCREEN.ROLES";
            public const string Permissions = "SCREEN.PERMISSIONS";
        }

        /// <summary>
        /// Sub-modules within Stock Management screen
        /// </summary>
        public static class StockSubModules
        {
            public const string Inventory = "SUBMODULE.INVENTORY";
            public const string StockAdjustment = "SUBMODULE.STOCK_ADJUSTMENT";
            public const string StockTransfer = "SUBMODULE.STOCK_TRANSFER";
            public const string GRN = "SUBMODULE.GRN";
            public const string GoodsReturn = "SUBMODULE.GOODS_RETURN";
            public const string GoodsReplace = "SUBMODULE.GOODS_REPLACE";
        }

        // ===============================================
        // ACTION-LEVEL PERMISSIONS (Granular CRUD)
        // ===============================================

        /// <summary>
        /// Dashboard permissions
        /// </summary>
        public static class Dashboard
        {
            public const string View = "DASHBOARD.VIEW";
            public const string Export = "DASHBOARD.EXPORT";
        }

        /// <summary>
        /// Transactions screen permissions
        /// </summary>
        public static class Transactions
        {
            public const string View = "TRANSACTIONS.VIEW";
            public const string Create = "TRANSACTIONS.CREATE";
            public const string Update = "TRANSACTIONS.UPDATE";
            public const string Delete = "TRANSACTIONS.DELETE";
            public const string Export = "TRANSACTIONS.EXPORT";
            public const string Print = "TRANSACTIONS.PRINT";
        }

        /// <summary>
        /// Reservation screen permissions
        /// </summary>
        public static class Reservation
        {
            public const string View = "RESERVATION.VIEW";
            public const string Create = "RESERVATION.CREATE";
            public const string Update = "RESERVATION.UPDATE";
            public const string Delete = "RESERVATION.DELETE";
            public const string Print = "RESERVATION.PRINT";
        }

        /// <summary>
        /// Order Table screen permissions
        /// </summary>
        public static class OrderTable
        {
            public const string View = "ORDER_TABLE.VIEW";
            public const string Create = "ORDER_TABLE.CREATE";
            public const string Update = "ORDER_TABLE.UPDATE";
            public const string Delete = "ORDER_TABLE.DELETE";
        }

        /// <summary>
        /// Reports screen permissions
        /// </summary>
        public static class Reports
        {
            public const string View = "REPORTS.VIEW";
            public const string Export = "REPORTS.EXPORT";
            public const string Print = "REPORTS.PRINT";
        }

        // --- STOCK MANAGEMENT SUB-MODULES ---

        /// <summary>
        /// Inventory sub-module permissions
        /// </summary>
        public static class Inventory
        {
            public const string View = "INVENTORY.VIEW";
            public const string Export = "INVENTORY.EXPORT";
            public const string Print = "INVENTORY.PRINT";
        }

        /// <summary>
        /// Stock Adjustment sub-module permissions
        /// </summary>
        public static class StockAdjustment
        {
            public const string View = "STOCK_ADJUSTMENT.VIEW";
            public const string Create = "STOCK_ADJUSTMENT.CREATE";
            public const string Read = "STOCK_ADJUSTMENT.READ";
            public const string Update = "STOCK_ADJUSTMENT.UPDATE";
            public const string Delete = "STOCK_ADJUSTMENT.DELETE";
            public const string Export = "STOCK_ADJUSTMENT.EXPORT";
            public const string Print = "STOCK_ADJUSTMENT.PRINT";
        }

        /// <summary>
        /// Stock Transfer sub-module permissions
        /// </summary>
        public static class StockTransfer
        {
            public const string View = "STOCK_TRANSFER.VIEW";
            public const string Create = "STOCK_TRANSFER.CREATE";
            public const string Read = "STOCK_TRANSFER.READ";
            public const string Update = "STOCK_TRANSFER.UPDATE";
            public const string Delete = "STOCK_TRANSFER.DELETE";
            public const string Export = "STOCK_TRANSFER.EXPORT";
            public const string Print = "STOCK_TRANSFER.PRINT";
        }

        /// <summary>
        /// GRN (Goods Receipt Note) sub-module permissions
        /// </summary>
        public static class GRN
        {
            public const string View = "GRN.VIEW";
            public const string Create = "GRN.CREATE";
            public const string Read = "GRN.READ";
            public const string Update = "GRN.UPDATE";
            public const string Delete = "GRN.DELETE";
            public const string Export = "GRN.EXPORT";
            public const string Print = "GRN.PRINT";
        }

        /// <summary>
        /// Goods Return sub-module permissions
        /// </summary>
        public static class GoodsReturn
        {
            public const string View = "GOODS_RETURN.VIEW";
            public const string Create = "GOODS_RETURN.CREATE";
            public const string Read = "GOODS_RETURN.READ";
            public const string Update = "GOODS_RETURN.UPDATE";
            public const string Delete = "GOODS_RETURN.DELETE";
            public const string Export = "GOODS_RETURN.EXPORT";
            public const string Print = "GOODS_RETURN.PRINT";
        }

        /// <summary>
        /// Goods Replace sub-module permissions
        /// </summary>
        public static class GoodsReplace
        {
            public const string View = "GOODS_REPLACE.VIEW";
            public const string Create = "GOODS_REPLACE.CREATE";
            public const string Read = "GOODS_REPLACE.READ";
            public const string Update = "GOODS_REPLACE.UPDATE";
            public const string Delete = "GOODS_REPLACE.DELETE";
            public const string Export = "GOODS_REPLACE.EXPORT";
            public const string Print = "GOODS_REPLACE.PRINT";
        }

        // --- PRODUCT MANAGEMENT SCREEN ---

        /// <summary>
        /// Product Management screen permissions
        /// </summary>
        public static class Product
        {
            public const string View = "PRODUCT.VIEW";
            public const string Create = "PRODUCT.CREATE";
            public const string Read = "PRODUCT.READ";
            public const string Update = "PRODUCT.UPDATE";
            public const string Delete = "PRODUCT.DELETE";
            public const string Export = "PRODUCT.EXPORT";
            public const string Import = "PRODUCT.IMPORT";
            public const string Print = "PRODUCT.PRINT";
        }

        // --- ADD OPTIONS SCREENS (14 screens) ---

        /// <summary>
        /// Brand screen permissions
        /// </summary>
        public static class Brand
        {
            public const string View = "BRAND.VIEW";
            public const string Create = "BRAND.CREATE";
            public const string Read = "BRAND.READ";
            public const string Update = "BRAND.UPDATE";
            public const string Delete = "BRAND.DELETE";
            public const string Export = "BRAND.EXPORT";
            public const string Import = "BRAND.IMPORT";
        }

        /// <summary>
        /// Category screen permissions
        /// </summary>
        public static class Category
        {
            public const string View = "CATEGORY.VIEW";
            public const string Create = "CATEGORY.CREATE";
            public const string Read = "CATEGORY.READ";
            public const string Update = "CATEGORY.UPDATE";
            public const string Delete = "CATEGORY.DELETE";
            public const string Export = "CATEGORY.EXPORT";
            public const string Import = "CATEGORY.IMPORT";
        }

        /// <summary>
        /// Product Attributes screen permissions
        /// </summary>
        public static class ProductAttributes
        {
            public const string View = "PRODUCT_ATTRIBUTES.VIEW";
            public const string Create = "PRODUCT_ATTRIBUTES.CREATE";
            public const string Read = "PRODUCT_ATTRIBUTES.READ";
            public const string Update = "PRODUCT_ATTRIBUTES.UPDATE";
            public const string Delete = "PRODUCT_ATTRIBUTES.DELETE";
        }

        /// <summary>
        /// Product Combinations screen permissions
        /// </summary>
        public static class ProductCombinations
        {
            public const string View = "PRODUCT_COMBINATIONS.VIEW";
            public const string Create = "PRODUCT_COMBINATIONS.CREATE";
            public const string Read = "PRODUCT_COMBINATIONS.READ";
            public const string Update = "PRODUCT_COMBINATIONS.UPDATE";
            public const string Delete = "PRODUCT_COMBINATIONS.DELETE";
        }

        /// <summary>
        /// Product Grouping screen permissions
        /// </summary>
        public static class ProductGrouping
        {
            public const string View = "PRODUCT_GROUPING.VIEW";
            public const string Create = "PRODUCT_GROUPING.CREATE";
            public const string Read = "PRODUCT_GROUPING.READ";
            public const string Update = "PRODUCT_GROUPING.UPDATE";
            public const string Delete = "PRODUCT_GROUPING.DELETE";
        }

        /// <summary>
        /// Price Types screen permissions
        /// </summary>
        public static class PriceTypes
        {
            public const string View = "PRICE_TYPES.VIEW";
            public const string Create = "PRICE_TYPES.CREATE";
            public const string Read = "PRICE_TYPES.READ";
            public const string Update = "PRICE_TYPES.UPDATE";
            public const string Delete = "PRICE_TYPES.DELETE";
        }

        /// <summary>
        /// Payment Types screen permissions
        /// </summary>
        public static class PaymentTypes
        {
            public const string View = "PAYMENT_TYPES.VIEW";
            public const string Create = "PAYMENT_TYPES.CREATE";
            public const string Read = "PAYMENT_TYPES.READ";
            public const string Update = "PAYMENT_TYPES.UPDATE";
            public const string Delete = "PAYMENT_TYPES.DELETE";
        }

        /// <summary>
        /// Tax Rates screen permissions
        /// </summary>
        public static class TaxRates
        {
            public const string View = "TAX_RATES.VIEW";
            public const string Create = "TAX_RATES.CREATE";
            public const string Read = "TAX_RATES.READ";
            public const string Update = "TAX_RATES.UPDATE";
            public const string Delete = "TAX_RATES.DELETE";
        }

        /// <summary>
        /// Customers screen (AddOptions) permissions
        /// </summary>
        public static class CustomersAddOptions
        {
            public const string View = "CUSTOMERS_ADD_OPTIONS.VIEW";
            public const string Create = "CUSTOMERS_ADD_OPTIONS.CREATE";
            public const string Read = "CUSTOMERS_ADD_OPTIONS.READ";
            public const string Update = "CUSTOMERS_ADD_OPTIONS.UPDATE";
            public const string Delete = "CUSTOMERS_ADD_OPTIONS.DELETE";
            public const string Export = "CUSTOMERS_ADD_OPTIONS.EXPORT";
            public const string Import = "CUSTOMERS_ADD_OPTIONS.IMPORT";
        }

        /// <summary>
        /// Customer Groups screen permissions
        /// </summary>
        public static class CustomerGroups
        {
            public const string View = "CUSTOMER_GROUPS.VIEW";
            public const string Create = "CUSTOMER_GROUPS.CREATE";
            public const string Read = "CUSTOMER_GROUPS.READ";
            public const string Update = "CUSTOMER_GROUPS.UPDATE";
            public const string Delete = "CUSTOMER_GROUPS.DELETE";
        }

        /// <summary>
        /// Suppliers screen (AddOptions) permissions
        /// </summary>
        public static class SuppliersAddOptions
        {
            public const string View = "SUPPLIERS_ADD_OPTIONS.VIEW";
            public const string Create = "SUPPLIERS_ADD_OPTIONS.CREATE";
            public const string Read = "SUPPLIERS_ADD_OPTIONS.READ";
            public const string Update = "SUPPLIERS_ADD_OPTIONS.UPDATE";
            public const string Delete = "SUPPLIERS_ADD_OPTIONS.DELETE";
            public const string Export = "SUPPLIERS_ADD_OPTIONS.EXPORT";
            public const string Import = "SUPPLIERS_ADD_OPTIONS.IMPORT";
        }

        /// <summary>
        /// Shop/Store screen permissions
        /// </summary>
        public static class Shop
        {
            public const string View = "SHOP.VIEW";
            public const string Create = "SHOP.CREATE";
            public const string Read = "SHOP.READ";
            public const string Update = "SHOP.UPDATE";
            public const string Delete = "SHOP.DELETE";
        }

        /// <summary>
        /// Discounts screen permissions
        /// </summary>
        public static class Discounts
        {
            public const string View = "DISCOUNTS.VIEW";
            public const string Create = "DISCOUNTS.CREATE";
            public const string Read = "DISCOUNTS.READ";
            public const string Update = "DISCOUNTS.UPDATE";
            public const string Delete = "DISCOUNTS.DELETE";
        }

        /// <summary>
        /// UOM (Unit of Measure) screen permissions
        /// </summary>
        public static class UOM
        {
            public const string View = "UOM.VIEW";
            public const string Create = "UOM.CREATE";
            public const string Read = "UOM.READ";
            public const string Update = "UOM.UPDATE";
            public const string Delete = "UOM.DELETE";
        }

        // --- SETTINGS SCREENS (4 screens) ---

        /// <summary>
        /// User Settings screen permissions
        /// </summary>
        public static class UserSettings
        {
            public const string View = "USER_SETTINGS.VIEW";
            public const string Update = "USER_SETTINGS.UPDATE";
        }

        /// <summary>
        /// Application Settings screen permissions
        /// </summary>
        public static class ApplicationSettings
        {
            public const string View = "APP_SETTINGS.VIEW";
            public const string Update = "APP_SETTINGS.UPDATE";
        }

        /// <summary>
        /// Roles screen permissions
        /// </summary>
        public static class Roles
        {
            public const string View = "ROLES.VIEW";
            public const string Create = "ROLES.CREATE";
            public const string Read = "ROLES.READ";
            public const string Update = "ROLES.UPDATE";
            public const string Delete = "ROLES.DELETE";
        }

        /// <summary>
        /// Permissions screen permissions
        /// </summary>
        public static class Permissions
        {
            public const string View = "PERMISSIONS.VIEW";
            public const string Create = "PERMISSIONS.CREATE";
            public const string Read = "PERMISSIONS.READ";
            public const string Update = "PERMISSIONS.UPDATE";
            public const string Delete = "PERMISSIONS.DELETE";
        }


    }
}
