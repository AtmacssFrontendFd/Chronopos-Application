using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
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
        
        // Add global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            LogMessage($"UNHANDLED EXCEPTION: {ex?.Message}");
            LogMessage($"UNHANDLED STACK TRACE: {ex?.StackTrace}");
        };
        
        DispatcherUnhandledException += (s, e) =>
        {
            LogMessage($"DISPATCHER EXCEPTION: {e.Exception.Message}");
            LogMessage($"DISPATCHER STACK TRACE: {e.Exception.StackTrace}");
            e.Handled = true; // Prevent app crash
        };
        
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
                    
                    services.AddTransient<ICurrencyRepository, CurrencyRepository>();
                    LogMessage("CurrencyRepository registered as Transient");
                    
                    // Register Permission repositories
                    services.AddTransient<IPermissionRepository, PermissionRepository>();
                    LogMessage("PermissionRepository registered as Transient");
                    
                    services.AddTransient<IRoleRepository, RoleRepository>();
                    LogMessage("RoleRepository registered as Transient");
                    
                    services.AddTransient<IRolePermissionRepository, RolePermissionRepository>();
                    LogMessage("RolePermissionRepository registered as Transient");
                    
                    services.AddTransient<IUserPermissionOverrideRepository, UserPermissionOverrideRepository>();
                    LogMessage("UserPermissionOverrideRepository registered as Transient");
                    
                    services.AddTransient<IUserRepository, UserRepository>();
                    LogMessage("UserRepository registered as Transient");
                    
                    services.AddTransient<IStoreRepository, StoreRepository>();
                    LogMessage("StoreRepository registered as Transient");
                    
                    services.AddTransient<IProductImageRepository, ProductImageRepository>();
                    LogMessage("ProductImageRepository registered as Transient");

                    services.AddTransient<IProductBarcodeRepository, ProductBarcodeRepository>();
                    LogMessage("ProductBarcodeRepository registered as Transient");
                    
                    services.AddTransient<IProductBatchRepository, ProductBatchRepository>();
                    LogMessage("ProductBatchRepository registered as Transient");

                    services.AddTransient<IDiscountRepository, DiscountRepository>();
                    LogMessage("DiscountRepository registered as Transient");
                    
                    // Register CustomerGroupRelation repository
                    services.AddTransient<ICustomerGroupRelationRepository, CustomerGroupRelationRepository>();
                    LogMessage("CustomerGroupRelationRepository registered as Transient");
                    
                    // Register StockTransfer repository
                    services.AddTransient<IStockTransferRepository, StockTransferRepository>();
                    LogMessage("StockTransferRepository registered as Transient");
                    
                    // Register StockTransferItem repository
                    services.AddTransient<IStockTransferItemRepository, StockTransferItemRepository>();
                    LogMessage("StockTransferItemRepository registered as Transient");
                    
                    // Register ShopLocation repository
                    services.AddTransient<IShopLocationRepository, ShopLocationRepository>();
                    LogMessage("ShopLocationRepository registered as Transient");
                    
                    // Register Supplier repository
                    services.AddTransient<ISupplierRepository, SupplierRepository>();
                    LogMessage("SupplierRepository registered as Transient");
                    
                    // Register GoodsReceived repositories
                    services.AddTransient<IGoodsReceivedRepository, GoodsReceivedRepository>();
                    LogMessage("GoodsReceivedRepository registered as Transient");
                    
                    services.AddTransient<IGoodsReceivedItemRepository, GoodsReceivedItemRepository>();
                    LogMessage("GoodsReceivedItemRepository registered as Transient");
                    
                    // Register GoodsReturn repositories
                    services.AddTransient<IGoodsReturnRepository, GoodsReturnRepository>();
                    LogMessage("GoodsReturnRepository registered as Transient");
                    
                    services.AddTransient<IGoodsReturnItemRepository, GoodsReturnItemRepository>();
                    LogMessage("GoodsReturnItemRepository registered as Transient");
                    
                    // Register GoodsReplace repository
                    services.AddTransient<IGoodsReplaceRepository, GoodsReplaceRepository>();
                    LogMessage("GoodsReplaceRepository registered as Transient");
                    
                    services.AddTransient<IGoodsReplaceItemRepository, GoodsReplaceItemRepository>();
                    LogMessage("GoodsReplaceItemRepository registered as Transient");

                    // Register Product Attribute repositories
                    services.AddTransient<IProductAttributeRepository, ProductAttributeRepository>();
                    LogMessage("ProductAttributeRepository registered as Transient");
                    
                    services.AddTransient<IProductAttributeValueRepository, ProductAttributeValueRepository>();
                    LogMessage("ProductAttributeValueRepository registered as Transient");

                    services.AddTransient<IProductCombinationItemRepository, ProductCombinationItemRepository>();
                    LogMessage("ProductCombinationItemRepository registered as Transient");

                    // Register application services as Transient to ensure fresh DbContext instances
                    services.AddTransient<IProductService, ProductService>();
                    LogMessage("ProductService registered as Transient");
                    // Register DbContext interface for Application layer services
                    services.AddScoped<IChronoPosDbContext, ChronoPosDbContext>();
                    LogMessage("IChronoPosDbContext registered as Scoped");
                    
                    services.AddTransient<IBrandService, BrandService>();
                    LogMessage("BrandService registered as Transient");
                    
                    services.AddTransient<ICurrencyService, CurrencyService>();
                    LogMessage("CurrencyService registered as Transient");
                    
                    // Register Permission services
                    services.AddTransient<IPermissionService, PermissionService>();
                    LogMessage("PermissionService registered as Transient");
                    
                    // Register Role service
                    services.AddTransient<IRoleService, RoleService>();
                    LogMessage("RoleService registered as Transient");
                    
                    // Register User service
                    services.AddTransient<IUserService, UserService>();
                    LogMessage("UserService registered as Transient");
                    
                    // Register UserPermissionOverride service
                    services.AddTransient<IUserPermissionOverrideService, UserPermissionOverrideService>();
                    LogMessage("UserPermissionOverrideService registered as Transient");
                    
                    // Register CurrentUser service (Singleton to maintain state across the app)
                    services.AddSingleton<ICurrentUserService, CurrentUserService>();
                    LogMessage("CurrentUserService registered as Singleton");
                    
                    services.AddTransient<IStoreService, StoreService>();
                    LogMessage("StoreService registered as Transient");
                    
                    services.AddTransient<IProductImageService, ProductImageService>();
                    LogMessage("ProductImageService registered as Transient");

                    services.AddTransient<IProductBarcodeService, ProductBarcodeService>();
                    LogMessage("ProductBarcodeService registered as Transient");
                    
                    services.AddTransient<IProductBatchService, Infrastructure.Services.ProductBatchService>();
                    LogMessage("ProductBatchService registered as Transient");

                    // Register TaxType service
                    services.AddTransient<ITaxTypeService, TaxTypeService>();
                    LogMessage("TaxTypeService registered as Transient");
                    
                    // Register Customer service
                    services.AddTransient<ICustomerService, CustomerService>();
                    LogMessage("CustomerService registered as Transient");
                    
                    // Register CustomerGroup services
                    services.AddTransient<ICustomerGroupService, CustomerGroupService>();
                    LogMessage("CustomerGroupService registered as Transient");
                    
                    services.AddTransient<ICustomerGroupRelationService, CustomerGroupRelationService>();
                    LogMessage("CustomerGroupRelationService registered as Transient");
                    
                    // Register ProductGroup service
                    services.AddTransient<IProductGroupService, ProductGroupService>();
                    LogMessage("ProductGroupService registered as Transient");
                    
                    // Register ProductGroupItem service
                    services.AddTransient<IProductGroupItemService, ProductGroupItemService>();
                    LogMessage("ProductGroupItemService registered as Transient");
                    
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
                    
                    // Register stock transfer service
                    services.AddTransient<IStockTransferService, Infrastructure.Services.StockTransferService>();
                    LogMessage("StockTransferService registered as Transient");
                    
                    // Register stock transfer item service
                    services.AddTransient<IStockTransferItemService, StockTransferItemService>();
                    LogMessage("StockTransferItemService registered as Transient");
                    
                    // Register goods received service
                    services.AddTransient<IGoodsReceivedService, GoodsReceivedService>();
                    LogMessage("GoodsReceivedService registered as Transient");
                    
                    // Register goods return service
                    services.AddTransient<IGoodsReturnService, GoodsReturnService>();
                    LogMessage("GoodsReturnService registered as Transient");
                    
                    // Register goods replace service
                    services.AddTransient<IGoodsReplaceService, GoodsReplaceService>();
                    LogMessage("GoodsReplaceService registered as Transient");
                    
                    // Register goods item services
                    services.AddTransient<IGoodsReturnItemService, GoodsReturnItemService>();
                    LogMessage("GoodsReturnItemService registered as Transient");
                    
                    services.AddTransient<IGoodsReplaceItemService, GoodsReplaceItemService>();
                    LogMessage("GoodsReplaceItemService registered as Transient");
                    
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

                    // Register SKU Generation service
                    services.AddTransient<ISkuGenerationService, SkuGenerationService>();
                    LogMessage("SkuGenerationService registered as Transient");

                    // Register Product Attribute service
                    services.AddTransient<IProductAttributeService, ProductAttributeService>();
                    LogMessage("ProductAttributeService registered as Transient");
                    
                    services.AddTransient<IProductCombinationItemService, ProductCombinationItemService>();
                    LogMessage("ProductCombinationItemService registered as Transient");
                    
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

                    // Register licensing services
                    services.AddSingleton<ILicensingService, LicensingService>();
                    LogMessage("LicensingService registered");

                    services.AddSingleton<IHostDiscoveryService, HostDiscoveryService>();
                    LogMessage("HostDiscoveryService registered");

                    services.AddSingleton<ICameraService, CameraService>();
                    LogMessage("CameraService registered");

                    // Register MainWindowViewModel as Singleton for stable event subscriptions
                    services.AddSingleton<MainWindowViewModel>();
                    LogMessage("MainWindowViewModel registered as Singleton");
                    services.AddTransient<ProductsViewModel>();
                    LogMessage("ProductsViewModel registered as Transient");
                    services.AddTransient<ProductManagementViewModel>();
                    LogMessage("ProductManagementViewModel registered as Transient");
                    services.AddTransient<StockManagementViewModel>();
                    LogMessage("StockManagementViewModel registered as Transient");
                    services.AddTransient<AddOptionsViewModel>();
                    LogMessage("AddOptionsViewModel registered as Transient");
                    services.AddTransient<AddProductViewModel>();
                    LogMessage("AddProductViewModel registered as Transient");
                    services.AddTransient<SalesViewModel>();
                    LogMessage("SalesViewModel registered as Transient");
                    services.AddTransient<CustomersViewModel>();
                    LogMessage("CustomersViewModel registered as Transient");
                    services.AddTransient<CustomerSidePanelViewModel>();
                    LogMessage("CustomerSidePanelViewModel registered as Transient");
                    services.AddTransient<CustomerGroupsViewModel>();
                    LogMessage("CustomerGroupsViewModel registered as Transient");
                    services.AddTransient<CustomerGroupSidePanelViewModel>();
                    LogMessage("CustomerGroupSidePanelViewModel registered as Transient");
                    services.AddTransient<ProductGroupsViewModel>();
                    LogMessage("ProductGroupsViewModel registered as Transient");
                    services.AddTransient<ProductGroupSidePanelViewModel>();
                    LogMessage("ProductGroupSidePanelViewModel registered as Transient");
                    services.AddTransient<SuppliersViewModel>();
                    LogMessage("SuppliersViewModel registered as Transient");
                    services.AddTransient<SupplierSidePanelViewModel>();
                    LogMessage("SupplierSidePanelViewModel registered as Transient");
                    services.AddTransient<SettingsViewModel>();
                    LogMessage("SettingsViewModel registered as Transient");
                    
                    // Register Permission ViewModels
                    services.AddTransient<PermissionViewModel>();
                    LogMessage("PermissionViewModel registered as Transient");
                    
                    services.AddTransient<PermissionSidePanelViewModel>();
                    LogMessage("PermissionSidePanelViewModel registered as Transient");
                    
                    // Register Role ViewModels
                    services.AddTransient<RoleViewModel>();
                    LogMessage("RoleViewModel registered as Transient");
                    
                    services.AddTransient<RoleSidePanelViewModel>();
                    LogMessage("RoleSidePanelViewModel registered as Transient");
                    
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

                    // Register onboarding view model
                    services.AddTransient<OnboardingViewModel>();
                    LogMessage("OnboardingViewModel registered as Transient");
        
                    // Register Views - MainWindow as Singleton to match ViewModel
                    services.AddSingleton<MainWindow>();
                    LogMessage("MainWindow registered as Singleton");

                    services.AddTransient<OnboardingWindow>();
                    LogMessage("OnboardingWindow registered as Transient");

                    services.AddTransient<CreateAdminWindow>();
                    LogMessage("CreateAdminWindow registered as Transient");

                    services.AddTransient<LoginWindow>();
                    LogMessage("LoginWindow registered as Transient");
                    
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

    protected override void OnStartup(StartupEventArgs e)
    {
        LogMessage("=== OnStartup called ===");
        
        // Call base.OnStartup FIRST (standard WPF pattern)
        base.OnStartup(e);
        LogMessage("base.OnStartup() completed");
        
        try
        {
            LogMessage("Starting host...");
            _host.Start();
            LogMessage("Host started successfully");

            // Initialize all services synchronously
            InitializeServices();
            
            // Initialize database synchronously
            LogMessage("Initializing database...");
            InitializeDatabase();
            LogMessage("Database initialized successfully");

            // Seed language translations synchronously
            LogMessage("Seeding language translations...");
            SeedLanguageTranslations();
            LogMessage("Language translations seeded successfully");

            // Now run the startup flow SYNCHRONOUSLY (we're already on UI thread)
            LogMessage("=== Starting Startup Flow ===");
            RunStartupFlow();
            
            LogMessage("=== OnStartup completed successfully ===");
        }
        catch (Exception ex)
        {
            LogMessage($"!!! FATAL ERROR in OnStartup: {ex.Message}");
            LogMessage($"!!! Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                LogMessage($"!!! Inner exception: {ex.InnerException.Message}");
                LogMessage($"!!! Inner stack trace: {ex.InnerException.StackTrace}");
            }
            
            MessageBox.Show(
                $"Application startup failed:\n\n{ex.Message}\n\nSee log file for details.", 
                "Startup Error", 
                MessageBoxButton.OK, 
                MessageBoxImage.Error);
            
            this.Shutdown(1);
        }
    }

    private void InitializeServices()
    {
        LogMessage(">>> Initializing all services...");
        
        // Initialize theme service
        try 
        {
            LogMessage("  - Initializing theme service...");
            var themeService = _host.Services.GetRequiredService<IThemeService>();
            themeService.LoadThemeFromSettings();
            LogMessage("  - Theme service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Theme service error: {ex.Message}");
        }

        // Initialize font service
        try 
        {
            LogMessage("  - Initializing font service...");
            var fontService = _host.Services.GetRequiredService<IFontService>();
            fontService.LoadFontFromSettings();
            LogMessage("  - Font service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Font service error: {ex.Message}");
        }

        // Initialize icon service
        try 
        {
            LogMessage("  - Initializing icon service...");
            var iconService = _host.Services.GetRequiredService<IIconService>();
            iconService.RegisterIconResources();
            LogMessage("  - Icon service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Icon service error: {ex.Message}");
        }

        // Initialize localization service
        try 
        {
            LogMessage("  - Initializing localization service...");
            var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
            localizationService.LoadLanguageFromSettings();
            LogMessage("  - Localization service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Localization service error: {ex.Message}");
        }

        // Initialize color scheme service
        try 
        {
            LogMessage("  - Initializing color scheme service...");
            var colorSchemeService = _host.Services.GetRequiredService<IColorSchemeService>();
            colorSchemeService.LoadColorsFromSettings();
            LogMessage("  - Color scheme service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Color scheme service error: {ex.Message}");
        }

        // Initialize layout direction service
        try 
        {
            LogMessage("  - Initializing layout direction service...");
            var layoutDirectionService = _host.Services.GetRequiredService<ILayoutDirectionService>();
            layoutDirectionService.LoadDirectionFromSettings();
            LogMessage("  - Layout direction service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Layout direction service error: {ex.Message}");
        }

        // Initialize zoom service
        try 
        {
            LogMessage("  - Initializing zoom service...");
            var zoomService = _host.Services.GetRequiredService<IZoomService>();
            zoomService.LoadZoomFromSettings();
            LogMessage("  - Zoom service initialized ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Zoom service error: {ex.Message}");
        }
        
        LogMessage(">>> All services initialized");
    }

    private void RunStartupFlow()
    {
        LogMessage(">>> Step 1: Checking license status...");
        
        var licensingService = _host.Services.GetRequiredService<ILicensingService>();
        
        // Step 1: License check
        if (!licensingService.IsLicenseValid())
        {
            LogMessage(">>> No valid license found, showing OnboardingWindow...");
            
            var onboardingWindow = _host.Services.GetRequiredService<OnboardingWindow>();
            LogMessage(">>> OnboardingWindow instance created");
            
            LogMessage(">>> Calling OnboardingWindow.ShowDialog()...");
            var onboardingResult = onboardingWindow.ShowDialog();
            LogMessage($">>> OnboardingWindow.ShowDialog() returned: {onboardingResult}");
            
            if (onboardingResult != true)
            {
                LogMessage(">>> Onboarding cancelled by user, shutting down...");
                this.Shutdown();
                return;
            }
            
            LogMessage(">>> Onboarding completed successfully ✓");
        }
        else
        {
            LogMessage(">>> Valid license found, skipping onboarding ✓");
        }

        // Step 2: Admin user check
        LogMessage(">>> Step 2: Checking for admin user...");
        
        if (!AdminUserExists())
        {
            LogMessage(">>> No admin user found, showing CreateAdminWindow...");
            
            var createAdminWindow = _host.Services.GetRequiredService<CreateAdminWindow>();
            LogMessage(">>> CreateAdminWindow instance created");
            
            LogMessage(">>> Calling CreateAdminWindow.ShowDialog()...");
            var adminResult = createAdminWindow.ShowDialog();
            LogMessage($">>> CreateAdminWindow.ShowDialog() returned: {adminResult}");
            LogMessage($">>> AdminCreated property: {createAdminWindow.AdminCreated}");
            
            if (adminResult != true || !createAdminWindow.AdminCreated)
            {
                LogMessage(">>> Admin creation cancelled or failed, shutting down...");
                this.Shutdown();
                return;
            }
            
            LogMessage(">>> Admin user created successfully ✓");
        }
        else
        {
            LogMessage(">>> Admin user already exists, skipping admin creation ✓");
        }

        // Step 3: Login
        LogMessage(">>> Step 3: Showing LoginWindow...");
        
        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        LogMessage(">>> LoginWindow instance created");
        
        LogMessage(">>> Calling LoginWindow.ShowDialog()...");
        var loginResult = loginWindow.ShowDialog();
        LogMessage($">>> LoginWindow.ShowDialog() returned: {loginResult}");
        
        if (loginResult != true)
        {
            LogMessage(">>> Login cancelled by user, shutting down...");
            this.Shutdown();
            return;
        }
        
        LogMessage($">>> Login successful! User ID: {loginWindow.LoggedInUserId} ✓");

        // Set the current user in the CurrentUserService
        var currentUserService = _host.Services.GetRequiredService<ICurrentUserService>();
        currentUserService.SetCurrentUserAsync(loginWindow.LoggedInUserId).GetAwaiter().GetResult();
        LogMessage($">>> Current user set in CurrentUserService");

        // Step 4: Show MainWindow
        LogMessage(">>> Step 4: Showing MainWindow...");
        
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        LogMessage(">>> MainWindow instance created");
        
        // Set as application main window
        this.MainWindow = mainWindow;
        LogMessage(">>> Set as Application.MainWindow");
        
        // Change shutdown mode to close when main window closes
        this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        LogMessage(">>> ShutdownMode changed to OnMainWindowClose");
        
        LogMessage(">>> Calling MainWindow.Show()...");
        mainWindow.Show();
        LogMessage(">>> MainWindow.Show() completed ✓");
        
        LogMessage(">>> === STARTUP FLOW COMPLETED SUCCESSFULLY ===");
    }

    private bool AdminUserExists()
    {
        try
        {
            LogMessage("  - Creating scope for admin user check...");
            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
            
            LogMessage("  - Checking database connection...");
            var canConnect = dbContext.Database.CanConnect();
            LogMessage($"  - Database connection status: {canConnect}");
            
            if (!canConnect)
            {
                LogMessage("  - Cannot connect to database, returning false");
                return false;
            }
            
            LogMessage("  - Counting non-deleted users...");
            var userCount = dbContext.Users.Count(u => !u.Deleted);
            LogMessage($"  - Found {userCount} non-deleted users");
            
            if (userCount > 0)
            {
                var users = dbContext.Users
                    .Where(u => !u.Deleted)
                    .Select(u => new { u.Id, u.Email, u.FullName })
                    .ToList();
                
                foreach (var user in users)
                {
                    LogMessage($"  - Existing user: ID={user.Id}, Email={user.Email}, Name={user.FullName}");
                }
            }
            
            return userCount > 0;
        }
        catch (Exception ex)
        {
            LogMessage($"  - Error checking for admin user: {ex.Message}");
            LogMessage($"  - Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
            
            LogMessage("  - Ensuring database exists...");
            dbContext.Database.EnsureCreated();
            LogMessage("  - Database ensured ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Database initialization error: {ex.Message}");
            MessageBox.Show(
                $"Database initialization failed: {ex.Message}\nThe application will continue without database functionality.", 
                "Database Warning", 
                MessageBoxButton.OK, 
                MessageBoxImage.Warning);
        }
    }

    private void SeedLanguageTranslations()
    {
        try
        {
            LogMessage("  - Starting language translation seeding...");
            
            using var scope = _host.Services.CreateScope();
            var seedingService = scope.ServiceProvider.GetRequiredService<ILanguageSeedingService>();
            
            // Run async seeding synchronously (we're in startup, it's ok)
            seedingService.SeedAllTranslationsAsync().GetAwaiter().GetResult();
            
            LogMessage("  - Language translations seeded ✓");
        }
        catch (Exception ex)
        {
            LogMessage($"  - Language seeding error: {ex.Message}");
            // Don't fail the app if seeding fails
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
            
            // Only create database if it doesn't exist - preserve existing data
            await dbContext.Database.EnsureCreatedAsync();
            
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
