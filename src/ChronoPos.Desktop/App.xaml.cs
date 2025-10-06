using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.IO;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Services;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop;

/// <summary>
/// Main application class for ChronoPos Desktop POS System
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private static string _logFilePath = string.Empty;

    public App()
    {
        // Setup logging
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
        Directory.CreateDirectory(chronoPosPath);
        _logFilePath = Path.Combine(chronoPosPath, "app.log");
        
        LogMessage("=== Application Starting ===");
        
        try
        {
            LogMessage("Creating host...");
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    LogMessage("Configuring services...");
                    
                    // Get local app data path for database
                    var databasePath = Path.Combine(chronoPosPath, "chronopos.db");
                    LogMessage($"Database path: {databasePath}");

                    // Configure Entity Framework with SQLite and improved settings

                    services.AddDbContext<ChronoPosDbContext>(options =>
                    {
                        options.UseSqlite($"Data Source={databasePath}");
                        // Enable sensitive data logging in debug mode for better error messages
                        options.EnableSensitiveDataLogging(true);
                        options.EnableDetailedErrors(true);
                    }, ServiceLifetime.Scoped);
                    LogMessage("DbContext configured as Scoped for desktop application lifecycle");

                    // Register repositories and unit of work as Transient to match DbContext
                    services.AddTransient<IUnitOfWork, UnitOfWork>();
                    LogMessage("UnitOfWork registered as Transient");

                    // Register Product-related repositories
                    services.AddTransient<IProductRepository, ProductRepository>();
                    LogMessage("ProductRepository registered as Transient");
                    
                    services.AddTransient<IBrandRepository, BrandRepository>();
                    LogMessage("BrandRepository registered as Transient");
                    
                    services.AddTransient<ICustomerGroupRelationRepository, CustomerGroupRelationRepository>();
                    LogMessage("CustomerGroupRelationRepository registered as Transient");
                    
                    services.AddTransient<IStoreRepository, StoreRepository>();
                    LogMessage("StoreRepository registered as Transient");
                    
                    services.AddTransient<ISupplierRepository, SupplierRepository>();
                    LogMessage("SupplierRepository registered as Transient");
                    
                    services.AddTransient<IProductImageRepository, ProductImageRepository>();
                    LogMessage("ProductImageRepository registered as Transient");

                    services.AddTransient<IProductBarcodeRepository, ProductBarcodeRepository>();
                    LogMessage("ProductBarcodeRepository registered as Transient");

                    services.AddTransient<IDiscountRepository, DiscountRepository>();
                    LogMessage("DiscountRepository registered as Transient");

                    // Register Product Attribute repositories
                    services.AddTransient<IProductAttributeRepository, ProductAttributeRepository>();
                    LogMessage("ProductAttributeRepository registered as Transient");
                    
                    services.AddTransient<IProductAttributeValueRepository, ProductAttributeValueRepository>();
                    LogMessage("ProductAttributeValueRepository registered as Transient");

                    services.AddTransient<IProductCombinationItemRepository, ProductCombinationItemRepository>();
                    LogMessage("ProductCombinationItemRepository registered as Transient");

                    services.AddTransient<IProductGroupItemRepository, ProductGroupItemRepository>();
                    LogMessage("ProductGroupItemRepository registered as Transient");

                    // Register application services as Transient to ensure fresh DbContext instances
                    services.AddTransient<IProductService, ProductService>();
                    LogMessage("ProductService registered as Transient");
                    // Register DbContext interface for Application layer services
                    services.AddScoped<IChronoPosDbContext, ChronoPosDbContext>();
                    LogMessage("IChronoPosDbContext registered as Scoped");
                    
                    services.AddTransient<IBrandService, BrandService>();
                    LogMessage("BrandService registered as Transient");
                    
                    services.AddTransient<IStoreService, StoreService>();
                    LogMessage("StoreService registered as Transient");
                    
                    services.AddTransient<IProductImageService, ProductImageService>();
                    LogMessage("ProductImageService registered as Transient");

                    services.AddTransient<IProductBarcodeService, ProductBarcodeService>();
                    LogMessage("ProductBarcodeService registered as Transient");

                    // Register TaxType service
                    services.AddTransient<ITaxTypeService, TaxTypeService>();
                    LogMessage("TaxTypeService registered as Transient");
                    
                    // Register Customer service
                    services.AddTransient<ICustomerService, CustomerService>();
                    LogMessage("CustomerService registered as Transient");
                    
                    // Register CustomerGroup service
                    services.AddTransient<ICustomerGroupService, CustomerGroupService>();
                    LogMessage("CustomerGroupService registered as Transient");
                    
                    // Register CustomerGroupRelation service
                    services.AddTransient<ICustomerGroupRelationService, CustomerGroupRelationService>();
                    LogMessage("CustomerGroupRelationService registered as Transient");
                    
                    // Register ProductGroup service
                    services.AddTransient<IProductGroupService, ProductGroupService>();
                    LogMessage("ProductGroupService registered as Transient");
                    
                    // Register Supplier service
                    services.AddTransient<ISupplierService, SupplierService>();
                    LogMessage("SupplierService registered as Transient");
                    
                    // Register SellingPriceType service
                    services.AddTransient<ISellingPriceTypeService, SellingPriceTypeService>();
                    LogMessage("SellingPriceTypeService registered as Transient");
                    
                    // Register PaymentType service
                    services.AddTransient<IPaymentTypeService, PaymentTypeService>();
                    LogMessage("PaymentTypeService registered as Transient");
                    
                    // Register logging service
                    services.AddSingleton<ILoggingService, ApplicationLoggingService>();
                    LogMessage("ApplicationLoggingService registered as Singleton");
                    
                    services.AddTransient<IStockAdjustmentService, StockAdjustmentService>();
                    LogMessage("StockAdjustmentService registered as Transient");
                    
                    // Register stock service
                    services.AddTransient<IStockService, Infrastructure.Services.StockService>();
                    LogMessage("StockService registered as Transient");
                    
                    // Register global search service
                    services.AddTransient<IGlobalSearchService, Application.Services.GlobalSearchService>();
                    LogMessage("GlobalSearchService registered as Transient");

                    // Register discount service
                    services.AddTransient<IDiscountService, DiscountService>();
                    LogMessage("DiscountService registered as Transient");

                    // Register UOM service and repository
                    services.AddTransient<IUomRepository, UomRepository>();
                    LogMessage("UomRepository registered as Transient");
                    services.AddTransient<IUomService, UomService>();
                    LogMessage("UomService registered as Transient");

                    // Register ProductUnit service and repository
                    services.AddTransient<IProductUnitRepository, ProductUnitRepository>();
                    LogMessage("ProductUnitRepository registered as Transient");
                    services.AddTransient<IProductUnitService, ProductUnitService>();
                    LogMessage("ProductUnitService registered as Transient");

                    // Register ProductBatch service and repository
                    services.AddTransient<IProductBatchRepository, ProductBatchRepository>();
                    LogMessage("ProductBatchRepository registered as Transient");
                    services.AddTransient<IProductBatchService, ProductBatchService>();
                    LogMessage("ProductBatchService registered as Transient");

                    // Register GoodsReceived service and repository
                    services.AddTransient<IGoodsReceivedRepository, GoodsReceivedRepository>();
                    LogMessage("GoodsReceivedRepository registered as Transient");
                    services.AddTransient<IGoodsReceivedService, GoodsReceivedService>();
                    LogMessage("GoodsReceivedService registered as Transient");

                    // Register GoodsReceivedItem service and repository
                    services.AddTransient<IGoodsReceivedItemRepository, GoodsReceivedItemRepository>();
                    LogMessage("GoodsReceivedItemRepository registered as Transient");
                    services.AddTransient<IGoodsReceivedItemService, GoodsReceivedItemService>();
                    LogMessage("GoodsReceivedItemService registered as Transient");

                    // Register StockTransfer service and repository
                    services.AddTransient<IStockTransferRepository, StockTransferRepository>();
                    LogMessage("StockTransferRepository registered as Transient");
                    services.AddTransient<IStockTransferService, StockTransferService>();
                    LogMessage("StockTransferService registered as Transient");

                    // Register StockTransferItem service and repository
                    services.AddTransient<IStockTransferItemRepository, StockTransferItemRepository>();
                    LogMessage("StockTransferItemRepository registered as Transient");
                    services.AddTransient<IStockTransferItemService, StockTransferItemService>();
                    LogMessage("StockTransferItemService registered as Transient");

                    // Register GoodsReturn service and repository
                    services.AddTransient<IGoodsReturnRepository, GoodsReturnRepository>();
                    LogMessage("GoodsReturnRepository registered as Transient");
                    services.AddTransient<IGoodsReturnService, GoodsReturnService>();
                    LogMessage("GoodsReturnService registered as Transient");

                    // Register GoodsReturnItem service and repository
                    services.AddTransient<IGoodsReturnItemRepository, GoodsReturnItemRepository>();
                    LogMessage("GoodsReturnItemRepository registered as Transient");
                    services.AddTransient<IGoodsReturnItemService, GoodsReturnItemService>();
                    LogMessage("GoodsReturnItemService registered as Transient");

                    // Register GoodsReplace service and repository
                    services.AddTransient<IGoodsReplaceRepository, GoodsReplaceRepository>();
                    LogMessage("GoodsReplaceRepository registered as Transient");
                    services.AddTransient<IGoodsReplaceService, GoodsReplaceService>();
                    LogMessage("GoodsReplaceService registered as Transient");

                    // Register GoodsReplaceItem service and repository
                    services.AddTransient<IGoodsReplaceItemRepository, GoodsReplaceItemRepository>();
                    LogMessage("GoodsReplaceItemRepository registered as Transient");
                    services.AddTransient<IGoodsReplaceItemService, GoodsReplaceItemService>();
                    LogMessage("GoodsReplaceItemService registered as Transient");

                    // Register ShopLocation service and repository
                    services.AddTransient<IShopLocationRepository, ShopLocationRepository>();
                    LogMessage("ShopLocationRepository registered as Transient");
                    services.AddTransient<IShopLocationService, ShopLocationService>();
                    LogMessage("ShopLocationService registered as Transient");

                    // Register SKU Generation service
                    services.AddTransient<ISkuGenerationService, SkuGenerationService>();
                    LogMessage("SkuGenerationService registered as Transient");

                    // Register Product Attribute service
                    services.AddTransient<IProductAttributeService, ProductAttributeService>();
                    LogMessage("ProductAttributeService registered as Transient");
                    
                    services.AddTransient<IProductCombinationItemService, ProductCombinationItemService>();
                    LogMessage("ProductCombinationItemService registered as Transient");
                    
                    services.AddTransient<IProductGroupItemService, ProductGroupItemService>();
                    LogMessage("ProductGroupItemService registered as Transient");
                    
                    // Register theme service
                    services.AddSingleton<IThemeService, ThemeService>();
                    LogMessage("ThemeService registered");

                    // Register font service
                    services.AddSingleton<IFontService, FontService>();
                    LogMessage("FontService registered");

                    // Register localization service
                    services.AddSingleton<ILocalizationService, LocalizationService>();
                    LogMessage("LocalizationService registered");

                    // Register database localization service as Singleton for event persistence
                    services.AddSingleton<IDatabaseLocalizationService, DatabaseLocalizationService>();
                    LogMessage("DatabaseLocalizationService registered as Singleton");

                    // Register language seeding service
                    services.AddTransient<ILanguageSeedingService, LanguageSeedingService>();
                    LogMessage("LanguageSeedingService registered as Transient");

                    // Register color scheme service
                    services.AddSingleton<IColorSchemeService, ColorSchemeService>();
                    LogMessage("ColorSchemeService registered");

                    // Register layout direction service
                    services.AddSingleton<ILayoutDirectionService, LayoutDirectionService>();
                    LogMessage("LayoutDirectionService registered");

                    // Register zoom service
                    services.AddSingleton<IZoomService, ZoomService>();
                    LogMessage("ZoomService registered");

                    // Register icon service
                    services.AddSingleton<IIconService, IconService>();
                    LogMessage("IconService registered");

                    // Register MainWindowViewModel as Singleton for stable event subscriptions
                    services.AddSingleton<MainWindowViewModel>();
                    LogMessage("MainWindowViewModel registered as Singleton");
                    services.AddTransient<ProductsViewModel>();
                    LogMessage("ProductsViewModel registered as Transient");
                    services.AddTransient<ProductManagementViewModel>();
                    LogMessage("ProductManagementViewModel registered as Transient");
                    services.AddTransient<StockManagementViewModel>();
                    LogMessage("StockManagementViewModel registered as Transient");
                    services.AddTransient<AddStockTransferViewModel>();
                    LogMessage("AddStockTransferViewModel registered as Transient");
                    services.AddTransient<AddGoodsReturnViewModel>();
                    LogMessage("AddGoodsReturnViewModel registered as Transient");
                    services.AddTransient<AddGoodsReplaceViewModel>();
                    LogMessage("AddGoodsReplaceViewModel registered as Transient");
                    services.AddTransient<AddGrnViewModel>();
                    LogMessage("AddGrnViewModel registered as Transient");
                    services.AddTransient<AddOptionsViewModel>();
                    LogMessage("AddOptionsViewModel registered as Transient");
                    services.AddTransient<AddProductViewModel>();
                    LogMessage("AddProductViewModel registered as Transient");
                    services.AddTransient<SalesViewModel>();
                    LogMessage("SalesViewModel registered as Transient");
                    services.AddTransient<CustomersViewModel>();
                    LogMessage("CustomersViewModel registered as Transient");
                    services.AddTransient<SuppliersViewModel>();
                    LogMessage("SuppliersViewModel registered as Transient");
                    services.AddTransient<SupplierSidePanelViewModel>();
                    LogMessage("SupplierSidePanelViewModel registered as Transient");
                    services.AddTransient<SettingsViewModel>();
                    LogMessage("SettingsViewModel registered as Transient");
                    services.AddTransient<DiscountViewModel>();
                    LogMessage("DiscountViewModel registered as Transient");
                    services.AddTransient<ProductAttributeViewModel>();
                    LogMessage("ProductAttributeViewModel registered as Transient");
                    services.AddTransient<ProductAttributeSidePanelViewModel>();
                    LogMessage("ProductAttributeSidePanelViewModel registered as Transient");
                    services.AddTransient<UomViewModel>();
                    LogMessage("UomViewModel registered as Transient");
                    services.AddTransient<UomSidePanelViewModel>();
                    LogMessage("UomSidePanelViewModel registered as Transient");
                    services.AddTransient<ProductCombinationViewModel>();
                    LogMessage("ProductCombinationViewModel registered as Transient");
                    services.AddTransient<ProductCombinationSidePanelViewModel>();
                    LogMessage("ProductCombinationSidePanelViewModel registered as Transient");
                    services.AddTransient<CategorySidePanelViewModel>();
                    LogMessage("CategorySidePanelViewModel registered as Transient");
                    services.AddTransient<CustomerGroupSidePanelViewModel>();
                    LogMessage("CustomerGroupSidePanelViewModel registered as Transient");
        
                    // Register Views - MainWindow as Singleton to match ViewModel
                    services.AddSingleton<MainWindow>();
                    LogMessage("MainWindow registered as Singleton");
                    
                    LogMessage("All services configured successfully");
                })
                .Build();
            LogMessage("Host created successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Error creating host: {ex.Message}");
            LogMessage($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static void LogMessage(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        Console.WriteLine(logEntry);
        try
        {
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        LogMessage("OnStartup called");
        try
        {
            LogMessage("Starting host...");
            await _host.StartAsync();
            LogMessage("Host started successfully");

            // Initialize theme service
            try 
            {
                LogMessage("Initializing theme service...");
                var themeService = _host.Services.GetRequiredService<IThemeService>();
                LogMessage("Theme service retrieved");
                themeService.LoadThemeFromSettings();
                LogMessage("Theme service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Theme service error: {ex.Message}");
                LogMessage($"Theme service stack trace: {ex.StackTrace}");
            }

            // Initialize font service
            try 
            {
                LogMessage("Initializing font service...");
                var fontService = _host.Services.GetRequiredService<IFontService>();
                LogMessage("Font service retrieved");
                fontService.LoadFontFromSettings();
                LogMessage("Font service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Font service error: {ex.Message}");
                LogMessage($"Font service stack trace: {ex.StackTrace}");
            }

            // Initialize icon service
            try 
            {
                LogMessage("Initializing icon service...");
                var iconService = _host.Services.GetRequiredService<IIconService>();
                LogMessage("Icon service retrieved");
                iconService.RegisterIconResources();
                LogMessage("Icon service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Icon service error: {ex.Message}");
                LogMessage($"Icon service stack trace: {ex.StackTrace}");
            }

            // Initialize localization service
            try 
            {
                LogMessage("Initializing localization service...");
                var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
                LogMessage("Localization service retrieved");
                localizationService.LoadLanguageFromSettings();
                LogMessage("Localization service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Localization service error: {ex.Message}");
                LogMessage($"Localization service stack trace: {ex.StackTrace}");
            }

            // Initialize color scheme service
            try 
            {
                LogMessage("Initializing color scheme service...");
                var colorSchemeService = _host.Services.GetRequiredService<IColorSchemeService>();
                LogMessage("Color scheme service retrieved");
                colorSchemeService.LoadColorsFromSettings();
                LogMessage("Color scheme service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Color scheme service error: {ex.Message}");
                LogMessage($"Color scheme service stack trace: {ex.StackTrace}");
            }

            // Initialize layout direction service
            try 
            {
                LogMessage("Initializing layout direction service...");
                var layoutDirectionService = _host.Services.GetRequiredService<ILayoutDirectionService>();
                LogMessage("Layout direction service retrieved");
                layoutDirectionService.LoadDirectionFromSettings();
                LogMessage("Layout direction service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Layout direction service error: {ex.Message}");
                LogMessage($"Layout direction service stack trace: {ex.StackTrace}");
            }

            // Initialize zoom service
            try 
            {
                LogMessage("Initializing zoom service...");
                var zoomService = _host.Services.GetRequiredService<IZoomService>();
                LogMessage("Zoom service retrieved");
                zoomService.LoadZoomFromSettings();
                LogMessage("Zoom service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Zoom service error: {ex.Message}");
                LogMessage($"Zoom service stack trace: {ex.StackTrace}");
                // Don't fail the app if zoom service fails
            }

            // Initialize database on startup
            LogMessage("Initializing database...");
            await InitializeDatabaseAsync();
            LogMessage("Database initialized successfully");

            // Initialize and seed all language translations
            LogMessage("Seeding language translations...");
            await SeedLanguageTranslationsAsync();
            LogMessage("Language translations seeded successfully");

            LogMessage("Getting MainWindow...");
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            LogMessage("MainWindow retrieved, showing...");
            mainWindow.Show();
            LogMessage("MainWindow shown successfully");

            base.OnStartup(e);
            LogMessage("OnStartup completed successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Application startup failed: {ex.Message}");
            LogMessage($"Startup stack trace: {ex.StackTrace}");
            MessageBox.Show($"Application startup failed: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                          "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown(1);
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
            
            // Initialize database if it doesn't exist, but preserve existing data
            LogMessage("Ensuring database exists...");
            
            // Create database and apply all configuration including seed data
            var created = await dbContext.Database.EnsureCreatedAsync();
            
            if (created)
            {
                LogMessage("New database created with seed data");
            }
            else
            {
                LogMessage("Database already exists");
                
                // Check if we have customers, if not, ensure seed data is applied
                var customerCount = await dbContext.Customers.CountAsync();
                LogMessage($"Found {customerCount} customers in database");
                
                if (customerCount == 0)
                {
                    LogMessage("No customers found, database may need recreation for seed data");
                }
            }
            
            LogMessage("Database initialized successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Database initialization failed: {ex.Message}");
            // Log error but don't crash the application
            MessageBox.Show($"Database initialization failed: {ex.Message}\nThe application will continue without database functionality.", 
                          "Database Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private async Task SeedLanguageTranslationsAsync()
    {
        try
        {
            LogMessage("Starting comprehensive language translation seeding...");
            
            using var scope = _host.Services.CreateScope();
            var languageSeedingService = scope.ServiceProvider.GetRequiredService<ILanguageSeedingService>();
            
            // Seed all translations for all screens
            await languageSeedingService.SeedAllTranslationsAsync();
            
            LogMessage("Language translation seeding completed successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Language seeding failed: {ex.Message}");
            LogMessage($"Language seeding stack trace: {ex.StackTrace}");
            // Don't crash the application, just log the error
            Console.WriteLine($"Warning: Language seeding failed: {ex.Message}");
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
