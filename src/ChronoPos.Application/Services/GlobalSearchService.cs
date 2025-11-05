using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Implementation of global search service
/// </summary>
public class GlobalSearchService : IGlobalSearchService
{
    private readonly IChronoPosDbContext _context;

    public GlobalSearchService(IChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<GlobalSearchResponseDto> SearchAsync(GlobalSearchFilterDto filter)
    {
        var response = new GlobalSearchResponseDto
        {
            Query = filter.Query,
            Results = new List<GlobalSearchResultDto>(),
            ResultsByModule = new Dictionary<string, int>()
        };

        if (string.IsNullOrWhiteSpace(filter.Query))
            return response;

        var tasks = new List<Task<List<GlobalSearchResultDto>>>();

        // For quick searches, give each category enough results
        // For full searches, divide evenly
        int perModuleLimit = filter.MaxResults > 10 
            ? filter.MaxResults / 8  // Full search: divide evenly
            : filter.MaxResults;      // Quick search: let each module return up to max, then sort and limit

        // Search in different modules based on filter
        if (filter.IncludeProducts)
            tasks.Add(SearchProductsAsync(filter.Query, perModuleLimit));

        if (filter.IncludeCustomers)
            tasks.Add(SearchCustomersAsync(filter.Query, perModuleLimit));

        if (filter.IncludeSales)
            tasks.Add(SearchSalesAsync(filter.Query, perModuleLimit));

        if (filter.IncludeStock)
            tasks.Add(SearchStockAsync(filter.Query, perModuleLimit));

        if (filter.IncludeBrands)
            tasks.Add(SearchBrandsAsync(filter.Query, perModuleLimit));

        if (filter.IncludeCategories)
            tasks.Add(SearchCategoriesAsync(filter.Query, perModuleLimit));

        // Add Suppliers and Transactions
        tasks.Add(SearchSuppliersAsync(filter.Query, perModuleLimit));
        tasks.Add(SearchTransactionsAsync(filter.Query, perModuleLimit));
        
        // Add Pages/Screens and Features
        if (filter.IncludePages)
            tasks.Add(Task.FromResult(SearchPagesAsync(filter.Query, perModuleLimit)));
        
        if (filter.IncludeFeatures)
            tasks.Add(Task.FromResult(SearchFeaturesAsync(filter.Query, perModuleLimit)));

        var results = await Task.WhenAll(tasks);

        // Combine and sort results
        var allResults = results.SelectMany(r => r).ToList();
        
        // Sort by relevance (simple scoring based on exact matches, then partial matches)
        var sortedResults = allResults
            .OrderByDescending(r => CalculateRelevanceScore(r, filter.Query))
            .Take(filter.MaxResults)
            .ToList();

        response.Results = sortedResults;
        response.TotalResults = sortedResults.Count;
        response.HasMoreResults = allResults.Count > filter.MaxResults;

        // Group by module for summary
        response.ResultsByModule = sortedResults
            .GroupBy(r => r.Module)
            .ToDictionary(g => g.Key, g => g.Count());

        return response;
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string query, int maxSuggestions = 10)
    {
        var suggestions = new HashSet<string>();

        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return suggestions.ToList();

        try
        {
            // Product name suggestions
            var productSuggestions = await _context.Products
                .Where(p => p.Name.Contains(query))
                .Select(p => p.Name)
                .Take(maxSuggestions / 3)
                .ToListAsync();
            
            foreach (var suggestion in productSuggestions)
                suggestions.Add(suggestion);

            // Brand name suggestions
            var brandSuggestions = await _context.Brands
                .Where(b => b.Name.Contains(query))
                .Select(b => b.Name)
                .Take(maxSuggestions / 3)
                .ToListAsync();
            
            foreach (var suggestion in brandSuggestions)
                suggestions.Add(suggestion);

            // Category name suggestions
            var categorySuggestions = await _context.Categories
                .Where(c => c.Name.Contains(query))
                .Select(c => c.Name)
                .Take(maxSuggestions / 3)
                .ToListAsync();
            
            foreach (var suggestion in categorySuggestions)
                suggestions.Add(suggestion);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting search suggestions: {ex.Message}");
        }

        return suggestions.Take(maxSuggestions).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> GetQuickSearchAsync(string query, int maxResults = 10)
    {
        var filter = new GlobalSearchFilterDto
        {
            Query = query,
            MaxResults = maxResults,
            IncludeProducts = true,
            IncludeCustomers = true,
            IncludeSales = false, // Exclude sales for quick search
            IncludeStock = false, // Exclude stock for quick search
            IncludeBrands = true,
            IncludeCategories = true,
            IncludePages = true, // Include pages for quick search
            IncludeFeatures = true // Include features for quick search
        };

        var response = await SearchAsync(filter);
        return response.Results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchProductsAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            // Convert query to lowercase for case-insensitive search
            var lowerQuery = query.ToLower();

            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductBarcodes)
                .Where(p => p.Name.ToLower().Contains(lowerQuery) || 
                           (p.Description != null && p.Description.ToLower().Contains(lowerQuery)) ||
                           (p.Code != null && p.Code.ToLower().Contains(lowerQuery)) ||
                           (p.Brand != null && p.Brand.Name.ToLower().Contains(lowerQuery)) ||
                           p.ProductBarcodes.Any(pb => pb.Barcode.ToLower().Contains(lowerQuery)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var product in products)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = product.Id,
                    Title = product.Name,
                    Description = $"Code: {product.Code} | {product.Brand?.Name ?? "No Brand"}",
                    Category = product.Category?.Name ?? "Uncategorized",
                    Module = "Products",
                    SearchType = "Product",
                    Data = product,
                    ImagePath = product.ProductImages?.FirstOrDefault()?.ImageUrl,
                    Price = (double?)product.Price,
                    Status = product.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching products: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchCustomersAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            // Convert query to lowercase for case-insensitive search
            var lowerQuery = query.ToLower();

            var customers = await _context.Customers
                .Where(c => c.CustomerFullName.ToLower().Contains(lowerQuery) || 
                           c.BusinessFullName.ToLower().Contains(lowerQuery) ||
                           c.OfficialEmail.ToLower().Contains(lowerQuery) ||
                           c.MobileNo.Contains(query))
                .Take(maxResults)
                .ToListAsync();

            foreach (var customer in customers)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = customer.Id,
                    Title = customer.DisplayName,
                    Description = $"Email: {customer.OfficialEmail} | Phone: {customer.MobileNo}",
                    Category = "Customer",
                    Module = "Customers",
                    SearchType = "Customer",
                    Data = customer,
                    Status = "Active"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchSalesAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.TransactionNumber.Contains(query) ||
                           (s.Customer != null && 
                            (s.Customer.CustomerFullName.Contains(query) || s.Customer.BusinessFullName.Contains(query))))
                .Take(maxResults)
                .ToListAsync();

            foreach (var sale in sales)
            {
                var customerName = sale.Customer != null 
                    ? sale.Customer.DisplayName 
                    : "Walk-in";
                    
                results.Add(new GlobalSearchResultDto
                {
                    Id = sale.Id,
                    Title = $"Transaction #{sale.TransactionNumber}",
                    Description = $"Customer: {customerName} | Date: {sale.SaleDate:MMM dd, yyyy}",
                    Category = "Transaction",
                    Module = "Sales",
                    SearchType = "Sale",
                    Data = sale,
                    Price = (double)sale.TotalAmount,
                    Status = sale.Status.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching sales: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchStockAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var stockAdjustments = await _context.StockAdjustments
                .Include(sa => sa.Reason)
                .Where(sa => sa.AdjustmentNo.Contains(query) ||
                            (sa.Reason != null && sa.Reason.Name.Contains(query)) ||
                            (sa.Remarks != null && sa.Remarks.Contains(query)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var adjustment in stockAdjustments)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = adjustment.AdjustmentId,
                    Title = $"Stock Adjustment - {adjustment.AdjustmentNo}",
                    Description = $"Reason: {adjustment.Reason?.Name ?? "Unknown"} | Status: {adjustment.Status}",
                    Category = "Stock",
                    Module = "Stock",
                    SearchType = "StockAdjustment",
                    Data = adjustment,
                    Status = $"{adjustment.AdjustmentDate:MMM dd, yyyy}"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching stock: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchBrandsAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            // Convert query to lowercase for case-insensitive search
            var lowerQuery = query.ToLower();

            var brands = await _context.Brands
                .Where(b => b.Name.ToLower().Contains(lowerQuery) || 
                           (b.Description != null && b.Description.ToLower().Contains(lowerQuery)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var brand in brands)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = brand.Id,
                    Title = brand.Name,
                    Description = brand.Description ?? "Brand",
                    Category = "Brand",
                    Module = "Brands",
                    SearchType = "Brand",
                    Data = brand,
                    Status = brand.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching brands: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchCategoriesAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            // Convert query to lowercase for case-insensitive search
            var lowerQuery = query.ToLower();

            var categories = await _context.Categories
                .Where(c => c.Name.ToLower().Contains(lowerQuery) || 
                           c.Description.ToLower().Contains(lowerQuery))
                .Take(maxResults)
                .ToListAsync();

            foreach (var category in categories)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = category.Id,
                    Title = category.Name,
                    Description = category.Description ?? "Category",
                    Category = "Category",
                    Module = "Categories",
                    SearchType = "Category",
                    Data = category,
                    Status = category.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching categories: {ex.Message}");
        }

        return results;
    }

    private static double CalculateRelevanceScore(GlobalSearchResultDto result, string query)
    {
        double score = 0;
        var lowerQuery = query.ToLowerInvariant();
        var lowerTitle = result.Title.ToLowerInvariant();
        var lowerDescription = result.Description.ToLowerInvariant();

        // Exact title match gets highest score
        if (lowerTitle == lowerQuery)
            score += 100;
        // Title starts with query
        else if (lowerTitle.StartsWith(lowerQuery))
            score += 80;
        // Title contains query
        else if (lowerTitle.Contains(lowerQuery))
            score += 60;

        // Description matches
        if (lowerDescription.Contains(lowerQuery))
            score += 20;

        // Boost popular modules
        switch (result.Module.ToLowerInvariant())
        {
            case "products":
                score += 10;
                break;
            case "customers":
                score += 8;
                break;
            case "sales":
            case "transactions":
                score += 6;
                break;
        }

        return score;
    }

    private async Task<List<GlobalSearchResultDto>> SearchSuppliersAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            // Convert query to lowercase for case-insensitive search
            var lowerQuery = query.ToLower();

            var suppliers = await _context.Suppliers
                .Where(s => s.CompanyName.ToLower().Contains(lowerQuery) || 
                           (s.KeyContactName != null && s.KeyContactName.ToLower().Contains(lowerQuery)) ||
                           (s.Email != null && s.Email.ToLower().Contains(lowerQuery)) ||
                           (s.Mobile != null && s.Mobile.Contains(query)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var supplier in suppliers)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = (int)supplier.SupplierId,
                    Title = supplier.CompanyName,
                    Description = $"Contact: {supplier.KeyContactName ?? "N/A"} | Email: {supplier.Email ?? "N/A"}",
                    Category = "Supplier",
                    Module = "Suppliers",
                    SearchType = "Supplier",
                    Data = supplier,
                    Status = supplier.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching suppliers: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchTransactionsAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var transactions = await _context.Transactions
                .Include(t => t.Customer)
                .Where(t => (t.InvoiceNumber != null && t.InvoiceNumber.Contains(query)) ||
                           (t.Customer != null && 
                            (t.Customer.CustomerFullName.Contains(query) || t.Customer.BusinessFullName.Contains(query))))
                .OrderByDescending(t => t.SellingTime)
                .Take(maxResults)
                .ToListAsync();

            foreach (var transaction in transactions)
            {
                var customerName = transaction.Customer != null 
                    ? transaction.Customer.DisplayName 
                    : "Walk-in";
                    
                results.Add(new GlobalSearchResultDto
                {
                    Id = transaction.Id,
                    Title = $"Transaction #{transaction.InvoiceNumber ?? transaction.Id.ToString()}",
                    Description = $"Customer: {customerName} | Date: {transaction.SellingTime:MMM dd, yyyy}",
                    Category = "Transaction",
                    Module = "Transactions",
                    SearchType = "Transaction",
                    Data = transaction,
                    Price = (double)transaction.TotalAmount,
                    Status = transaction.StatusDisplay
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching transactions: {ex.Message}");
        }

        return results;
    }
    
    private List<GlobalSearchResultDto> SearchPagesAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();
        var lowerQuery = query.ToLowerInvariant();

        // Define all available pages/screens in the application
        var pages = new List<(string Name, string Description, string Category, string Module, string[] Keywords)>
        {
            // Dashboard
            ("Dashboard", "Main dashboard with overview and statistics", "Navigation", "Dashboard", new[] { "dashboard", "home", "overview", "main", "statistics", "stats" }),
            
            // Sales & Transactions
            ("Sales Window", "Point of sale transaction screen", "Sales", "Transactions", new[] { "sales", "pos", "sell", "transaction", "checkout", "billing" }),
            ("Transactions", "View and manage all transactions", "Sales", "Transactions", new[] { "transactions", "sales", "invoices", "bills", "payments" }),
            ("Transaction History", "View transaction history and reports", "Sales", "Transactions", new[] { "history", "past sales", "transaction log", "sales history" }),
            
            // Back Office / Management
            ("Product Management", "Manage products, inventory, and pricing", "Management", "Products", new[] { "products", "items", "inventory", "stock", "catalog", "manage products" }),
            ("Customer Management", "Manage customer information and groups", "Management", "Customers", new[] { "customers", "clients", "contacts", "customer list", "manage customers" }),
            ("Supplier Management", "Manage supplier details and relationships", "Management", "Suppliers", new[] { "suppliers", "vendors", "provider", "supplier list", "manage suppliers" }),
            ("Category Management", "Organize products into categories", "Management", "Categories", new[] { "categories", "groups", "classification", "product categories" }),
            ("Brand Management", "Manage product brands", "Management", "Brands", new[] { "brands", "manufacturers", "labels", "brand list" }),
            ("Stock Management", "Inventory and stock control", "Management", "Stock", new[] { "stock", "inventory", "warehouse", "stock levels", "stock adjustment" }),
            ("Customer Groups", "Manage customer groups and segments", "Management", "Customers", new[] { "customer groups", "segments", "customer categories" }),
            ("Product Groups", "Manage product groupings", "Management", "Products", new[] { "product groups", "product sets", "bundles" }),
            
            // Settings
            ("Settings", "Application settings and configuration", "Settings", "Settings", new[] { "settings", "configuration", "preferences", "options", "setup" }),
            ("User Settings", "Manage user accounts and profiles", "Settings", "Users", new[] { "user settings", "users", "accounts", "profiles", "user management", "add user" }),
            ("Application Settings", "Configure application preferences", "Settings", "Configuration", new[] { "application settings", "app settings", "system settings", "preferences" }),
            ("Permissions", "Manage user permissions and access control", "Settings", "Security", new[] { "permissions", "access", "rights", "authorization", "access control", "umac" }),
            ("Roles", "Manage user roles and role permissions", "Settings", "Security", new[] { "roles", "user roles", "role management", "permissions groups" }),
            ("Tax Types", "Configure tax types and rates", "Settings", "Finance", new[] { "tax", "vat", "gst", "tax types", "tax rates", "taxation" }),
            ("Payment Types", "Manage payment methods", "Settings", "Finance", new[] { "payment", "payment types", "payment methods", "cash", "card", "upi" }),
            ("Units of Measurement", "Manage UOM for products", "Settings", "Products", new[] { "uom", "units", "measurement", "unit of measurement", "kg", "liter" }),
            ("Discounts", "Configure discount rules and offers", "Settings", "Finance", new[] { "discounts", "offers", "promotions", "sales", "coupon" }),
            ("Service Charges", "Configure service charges", "Settings", "Finance", new[] { "service charge", "service fee", "additional charges" }),
            
            // Stock Operations
            ("Stock Adjustment", "Adjust stock levels and quantities", "Stock", "Stock", new[] { "stock adjustment", "adjust stock", "inventory adjustment", "stock correction" }),
            ("Stock Transfer", "Transfer stock between locations", "Stock", "Stock", new[] { "stock transfer", "transfer stock", "move stock", "inter-store transfer" }),
            ("Goods Received", "Record received goods from suppliers", "Stock", "Stock", new[] { "goods received", "grn", "purchase receipt", "receiving" }),
            ("Goods Return", "Process goods return to suppliers", "Stock", "Stock", new[] { "goods return", "return to supplier", "purchase return" }),
            ("Goods Replace", "Replace defective goods", "Stock", "Stock", new[] { "goods replace", "replacement", "exchange goods" }),
            
            // Reservation & Restaurant
            ("Reservations", "Manage table reservations", "Restaurant", "Reservations", new[] { "reservations", "booking", "table booking", "reserve table" }),
            ("Restaurant Tables", "Manage restaurant table layout", "Restaurant", "Tables", new[] { "tables", "restaurant tables", "table management", "dining tables" }),
            ("Order Table", "Take orders for tables", "Restaurant", "Orders", new[] { "order", "table order", "dine-in", "kot", "kitchen order" }),
            
            // Product Features
            ("Add Product", "Add new product to inventory", "Products", "Products", new[] { "add product", "new product", "create product", "product entry" }),
            ("Product Attributes", "Manage product attributes and variations", "Products", "Products", new[] { "attributes", "variants", "variations", "product attributes", "size", "color" }),
            ("Product Modifiers", "Manage product add-ons and modifiers", "Products", "Products", new[] { "modifiers", "add-ons", "extras", "toppings", "customization" }),
            ("Product Combinations", "Manage product variant combinations", "Products", "Products", new[] { "combinations", "variants", "product combinations", "sku" }),
            ("Barcodes", "Manage product barcodes", "Products", "Products", new[] { "barcode", "ean", "upc", "qr code", "sku code" }),
            ("Product Batches", "Manage product batches and expiry", "Products", "Products", new[] { "batches", "batch number", "expiry", "lot number", "manufacturing date" }),
            
            // Reports
            ("Reports", "View sales and inventory reports", "Reports", "Reports", new[] { "reports", "analytics", "insights", "sales report", "inventory report" }),
            
            // Additional Features
            ("Import/Export", "Import or export data", "Tools", "Data", new[] { "import", "export", "data import", "data export", "csv", "excel" }),
            ("Backup", "Backup and restore data", "Tools", "System", new[] { "backup", "restore", "data backup", "database backup" }),
            ("Languages", "Configure application languages", "Settings", "Localization", new[] { "language", "languages", "localization", "translation", "multi-language" }),
            ("Themes", "Configure application theme and appearance", "Settings", "Appearance", new[] { "theme", "themes", "appearance", "dark mode", "light mode", "colors" }),
            ("Currencies", "Manage currencies and exchange rates", "Settings", "Finance", new[] { "currency", "currencies", "exchange rate", "multi-currency" }),
        };

        // Search through pages
        foreach (var page in pages)
        {
            // Check if query matches name, description, or any keyword
            bool matches = page.Name.ToLowerInvariant().Contains(lowerQuery) ||
                          page.Description.ToLowerInvariant().Contains(lowerQuery) ||
                          page.Keywords.Any(k => k.Contains(lowerQuery));

            if (matches)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = 0, // Pages don't have IDs
                    Title = page.Name,
                    Description = page.Description,
                    Category = page.Category,
                    Module = page.Module,
                    SearchType = "Page",
                    Data = page.Name, // Store page name for navigation
                    Status = "Navigate"
                });
            }

            if (results.Count >= maxResults)
                break;
        }

        return results;
    }
    
    private List<GlobalSearchResultDto> SearchFeaturesAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();
        var lowerQuery = query.ToLowerInvariant();

        // Define common features and actions
        var features = new List<(string Name, string Description, string Category, string Module, string[] Keywords)>
        {
            // Product Actions
            ("Add New Product", "Create a new product in inventory", "Action", "Products", new[] { "add", "new", "create", "product", "item" }),
            ("Edit Product", "Modify existing product details", "Action", "Products", new[] { "edit", "update", "modify", "product", "change" }),
            ("Delete Product", "Remove product from inventory", "Action", "Products", new[] { "delete", "remove", "product", "archive" }),
            ("Adjust Stock", "Adjust product stock levels", "Action", "Stock", new[] { "adjust", "stock", "quantity", "inventory" }),
            
            // Customer Actions
            ("Add Customer", "Register a new customer", "Action", "Customers", new[] { "add", "new", "register", "customer", "client" }),
            ("Edit Customer", "Update customer information", "Action", "Customers", new[] { "edit", "update", "customer", "profile" }),
            
            // Sales Actions
            ("New Sale", "Start a new sale transaction", "Action", "Sales", new[] { "new", "sale", "transaction", "sell", "checkout" }),
            ("Process Payment", "Process customer payment", "Action", "Sales", new[] { "payment", "pay", "checkout", "cash", "card" }),
            ("Print Invoice", "Print sales invoice", "Action", "Sales", new[] { "print", "invoice", "receipt", "bill" }),
            ("Refund", "Process customer refund", "Action", "Sales", new[] { "refund", "return", "money back", "cancel sale" }),
            ("Exchange", "Process product exchange", "Action", "Sales", new[] { "exchange", "swap", "replace" }),
            
            // Discount Actions
            ("Create Shop Discount", "Create store-wide discount promotion", "Action", "Finance", new[] { "shop", "store", "discount", "promotion", "sale", "offer" }),
            ("Create Product Discount", "Create discount for specific products", "Action", "Finance", new[] { "product", "discount", "item", "sale" }),
            ("Create Customer Discount", "Create discount for specific customers", "Action", "Finance", new[] { "customer", "discount", "loyalty", "vip" }),
            
            // User Management
            ("Add User", "Create new user account", "Action", "Users", new[] { "add", "new", "user", "account", "staff" }),
            ("Assign Role", "Assign role to user", "Action", "Users", new[] { "role", "assign", "permission", "access" }),
            ("Reset Password", "Reset user password", "Action", "Users", new[] { "password", "reset", "change password", "forgot" }),
            
            // System Actions
            ("Logout", "Sign out from application", "Action", "System", new[] { "logout", "sign out", "exit", "log out" }),
            ("Change Theme", "Switch application theme", "Action", "Settings", new[] { "theme", "dark", "light", "appearance" }),
            ("Change Language", "Switch application language", "Action", "Settings", new[] { "language", "translate", "locale" }),
            ("Export Data", "Export data to file", "Action", "Tools", new[] { "export", "download", "save", "csv", "excel" }),
            ("Import Data", "Import data from file", "Action", "Tools", new[] { "import", "upload", "load", "csv", "excel" }),
        };

        // Search through features
        foreach (var feature in features)
        {
            bool matches = feature.Name.ToLowerInvariant().Contains(lowerQuery) ||
                          feature.Description.ToLowerInvariant().Contains(lowerQuery) ||
                          feature.Keywords.Any(k => k.Contains(lowerQuery));

            if (matches)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = 0,
                    Title = feature.Name,
                    Description = feature.Description,
                    Category = feature.Category,
                    Module = feature.Module,
                    SearchType = "Feature",
                    Data = feature.Name,
                    Status = "Action"
                });
            }

            if (results.Count >= maxResults)
                break;
        }

        return results;
    }
}