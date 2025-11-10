using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChronoPos.Infrastructure.Services;
using System.Windows;
using ChronoPos.Application.Logging;
using System.Collections.ObjectModel;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Main window view model for the ChronoPos Desktop application
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;
    private readonly IGlobalSearchService _globalSearchService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductBarcodeRepository _productBarcodeRepository;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly GlobalSearchBarViewModel _globalSearchBarViewModel;
    private System.Timers.Timer? _searchDelayTimer;

    /// <summary>
    /// Gets the GlobalSearchBarViewModel for the search bar
    /// </summary>
    public GlobalSearchBarViewModel GlobalSearchBarViewModel => _globalSearchBarViewModel;

    [ObservableProperty]
    private string currentPageTitle = "Dashboard";

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private string currentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");

    [ObservableProperty]
    private string currentUser = "Administrator";

    [ObservableProperty]
    private string statusMessage = "System Ready";

    [ObservableProperty]
    private string selectedPage = "Dashboard";

    // Navigation Button Text Properties (Translated)
    [ObservableProperty]
    private string dashboardButtonText = "Dashboard";

    [ObservableProperty]
    private string salesWindowButtonText = "Sales Window";

    [ObservableProperty]
    private string transactionButtonText = "Transaction";

    [ObservableProperty]
    private string backOfficeButtonText = "Back Office";

    [ObservableProperty]
    private string reservationButtonText = "Reservation";

    [ObservableProperty]
    private string orderTableButtonText = "Order Table";

    [ObservableProperty]
    private string reportsButtonText = "Reports";

    [ObservableProperty]
    private string settingsButtonText = "Settings";

    [ObservableProperty]
    private string logoutButtonText = "Logout";

    // Sidebar Width Property
    [ObservableProperty]
    private GridLength sidebarWidth = new GridLength(160);

    // Navigation Button Visibility Properties (Based on UMAC Permissions)
    [ObservableProperty]
    private bool isDashboardVisible = true;

    [ObservableProperty]
    private bool isTransactionsVisible = true;

    [ObservableProperty]
    private bool isTransactionVisible = true;

    [ObservableProperty]
    private bool isManagementVisible = true;

    [ObservableProperty]
    private bool isReservationVisible = true;

    [ObservableProperty]
    private bool isOrderTableVisible = true;

    [ObservableProperty]
    private bool isReportsVisible = true;

    [ObservableProperty]
    private bool isSettingsVisible = true;

    // Global Search Properties
    [ObservableProperty]
    private string globalSearchQuery = string.Empty;

    [ObservableProperty]
    private bool showGlobalSearchResults = false;

    [ObservableProperty]
    private bool hasGlobalSearchText = false;

    [ObservableProperty]
    private string globalSearchResultsHeader = "Search Results";

    [ObservableProperty]
    private bool hasMoreGlobalSearchResults = false;

    [ObservableProperty]
    private GlobalSearchResultDto? selectedGlobalSearchResult;

    [ObservableProperty]
    private ObservableCollection<GlobalSearchResultDto> globalSearchResults = new();

    // Language switching properties
    [ObservableProperty]
    private ObservableCollection<Language> availableLanguages = new();

    [ObservableProperty]
    private Language? selectedLanguage;

    [ObservableProperty]
    private string currentLanguageCode = "en";

    [ObservableProperty]
    private string currentLanguageDisplayName = "English";

    /// <summary>
    /// Gets the active currency symbol for display in UI
    /// </summary>
    public string CurrencySymbol => _activeCurrencyService?.CurrencySymbol ?? "$";

    // Commands
    public ICommand ClearGlobalSearchCommand { get; }
    public ICommand ShowAllGlobalSearchResultsCommand { get; }
    public ICommand ChangeLanguageCommand { get; }
    public ICommand ToggleLanguageCommand { get; }

    public MainWindowViewModel(
        IProductService productService, 
        IServiceProvider serviceProvider, 
        IProductBarcodeRepository productBarcodeRepository,
        GlobalSearchBarViewModel globalSearchBarViewModel)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _productBarcodeRepository = productBarcodeRepository ?? throw new ArgumentNullException(nameof(productBarcodeRepository));
        _globalSearchBarViewModel = globalSearchBarViewModel ?? throw new ArgumentNullException(nameof(globalSearchBarViewModel));
        _databaseLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
        _globalSearchService = serviceProvider.GetRequiredService<IGlobalSearchService>();
        _currentUserService = serviceProvider.GetRequiredService<ICurrentUserService>();
        _activeCurrencyService = serviceProvider.GetRequiredService<IActiveCurrencyService>();
        
        // Wire up GlobalSearchBar navigation events
        _globalSearchBarViewModel.NavigateToResult += OnNavigateToSearchResult;
        _globalSearchBarViewModel.ShowAllResults += OnShowAllSearchResults;
        
        // Load current logged-in user name
        var currentUserDto = _currentUserService.CurrentUser;
        if (currentUserDto != null)
        {
            CurrentUser = currentUserDto.FullName ?? currentUserDto.Email ?? "User";
            Console.WriteLine($"MainWindowViewModel: Logged-in user = {CurrentUser}");
        }
        else
        {
            CurrentUser = "Guest";
            Console.WriteLine("MainWindowViewModel: No user logged in, showing Guest");
        }
        
        // Initialize commands
        ClearGlobalSearchCommand = new RelayCommand(ClearGlobalSearch);
        ShowAllGlobalSearchResultsCommand = new RelayCommand(ShowAllGlobalSearchResults);
        ChangeLanguageCommand = new AsyncRelayCommand<Language>(ChangeLanguageAsync);
        ToggleLanguageCommand = new AsyncRelayCommand(ToggleLanguageAsync);
        
        // Initialize search delay timer
        _searchDelayTimer = new System.Timers.Timer(300); // 300ms delay
        _searchDelayTimer.Elapsed += OnSearchDelayElapsed;
        _searchDelayTimer.AutoReset = false;
        
        // Subscribe to language change events
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
        
        // Initialize navigation button visibility based on UMAC permissions
        InitializeNavigationVisibility();
        
        // Initialize translations and ensure keywords exist
        _ = InitializeTranslationsAsync();
        
        // Initialize languages
        _ = LoadLanguagesAsync();
        
        // Start timer to update date/time
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (s, e) => CurrentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");
        timer.Start();
        timer.Start();

    // Show dashboard by default
    _ = ShowDashboard();
    }

    /// <summary>
    /// Initialize navigation button visibility based on UMAC permissions
    /// Hides buttons for screens the user doesn't have any permission to access
    /// </summary>
    private void InitializeNavigationVisibility()
    {
        try
        {
            // Check if user has ANY permission for each screen (Create, Edit, Delete, Import, Export, View, Print)
            IsDashboardVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.DASHBOARD);
            IsTransactionsVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SALES_WINDOW);
            IsTransactionVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.TRANSACTION);
            IsManagementVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.BACK_OFFICE);
            IsReservationVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.RESERVATION);
            // IsOrderTableVisible = false; // ORDER_TABLE removed from ScreenNames - button removed from sidebar
            IsOrderTableVisible = false; // ORDER_TABLE functionality removed
            IsReportsVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.REPORTS);
            IsSettingsVisible = _currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SETTINGS);

            AppLogger.Log($"Navigation visibility initialized: Dashboard={IsDashboardVisible}, SalesWindow={IsTransactionsVisible}, Transaction={IsTransactionVisible}, BackOffice={IsManagementVisible}, Reservation={IsReservationVisible}, OrderTable={IsOrderTableVisible}, Reports={IsReportsVisible}, Settings={IsSettingsVisible}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error initializing navigation visibility: {ex.Message}");
            // Default to showing all buttons if error occurs
            IsDashboardVisible = true;
            IsTransactionsVisible = true;
            IsManagementVisible = true;
            IsReservationVisible = true;
            IsOrderTableVisible = true;
            IsReportsVisible = true;
            IsSettingsVisible = true;
        }
    }

    private async Task InitializeTranslationsAsync()
    {
        try
        {
            // Ensure navigation keywords exist in database
            await EnsureNavigationKeywordsExistAsync();
            
            // Load translations
            await LoadTranslationsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing translations: {ex.Message}");
            // Continue with default values if initialization fails
        }
    }

    private async Task EnsureNavigationKeywordsExistAsync()
    {
        var keywords = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "nav_dashboard",
                new Dictionary<string, string>
                {
                    { "en", "Dashboard" },
                    { "ur", "ڈیش بورڈ" }
                }
            },
            {
                "nav_transactions",
                new Dictionary<string, string>
                {
                    { "en", "Transactions" },
                    { "ur", "لین دین" }
                }
            },
            {
                "nav_management",
                new Dictionary<string, string>
                {
                    { "en", "Management" },
                    { "ur", "انتظام" }
                }
            },
            {
                "nav_reservation",
                new Dictionary<string, string>
                {
                    { "en", "Reservation" },
                    { "ur", "بکنگ" }
                }
            },
            {
                "nav_order_table",
                new Dictionary<string, string>
                {
                    { "en", "Order Table" },
                    { "ur", "آرڈر ٹیبل" }
                }
            },
            {
                "nav_reports",
                new Dictionary<string, string>
                {
                    { "en", "Reports" },
                    { "ur", "رپورٹس" }
                }
            },
            {
                "nav_settings",
                new Dictionary<string, string>
                {
                    { "en", "Settings" },
                    { "ur", "ترتیبات" }
                }
            },
            {
                "nav_logout",
                new Dictionary<string, string>
                {
                    { "en", "Logout" },
                    { "ur", "لاگ آؤٹ" }
                }
            },
            {
                "dashboard_welcome",
                new Dictionary<string, string>
                {
                    { "en", "Welcome to ChronoPos Point of Sale System" },
                    { "ur", "کرونوپوس پوائنٹ آف سیل سسٹم میں خوش آمدید" }
                }
            },
            {
                "dashboard_today_sales",
                new Dictionary<string, string>
                {
                    { "en", "Today's Sales" },
                    { "ur", "آج کی فروخت" }
                }
            },
            {
                "dashboard_total_products",
                new Dictionary<string, string>
                {
                    { "en", "Total Products" },
                    { "ur", "کل پروڈکٹس" }
                }
            },
            {
                "dashboard_total_customers",
                new Dictionary<string, string>
                {
                    { "en", "Total Customers" },
                    { "ur", "کل گاہک" }
                }
            },
            {
                "status_dashboard_loaded",
                new Dictionary<string, string>
                {
                    { "en", "Dashboard loaded" },
                    { "ur", "ڈیش بورڈ لوڈ ہوا" }
                }
            },
            {
                "status_transactions_loaded",
                new Dictionary<string, string>
                {
                    { "en", "Transactions interface loaded" },
                    { "ur", "لین دین انٹرفیس لوڈ ہوا" }
                }
            },
            {
                "status_loading_management",
                new Dictionary<string, string>
                {
                    { "en", "Loading management..." },
                    { "ur", "انتظام لوڈ کر رہا ہے..." }
                }
            },
            {
                "status_management_loaded",
                new Dictionary<string, string>
                {
                    { "en", "Management modules loaded successfully" },
                    { "ur", "انتظامی ماڈیولز کامیابی سے لوڈ ہوئے" }
                }
            },
            {
                "status_loading_settings",
                new Dictionary<string, string>
                {
                    { "en", "Loading settings..." },
                    { "ur", "ترتیبات لوڈ کر رہا ہے..." }
                }
            },
            {
                "status_settings_loaded",
                new Dictionary<string, string>
                {
                    { "en", "Settings loaded" },
                    { "ur", "ترتیبات لوڈ ہوئیں" }
                }
            },
            {
                "transactions_product_area",
                new Dictionary<string, string>
                {
                    { "en", "Product Selection Area\n(Products will be loaded here)" },
                    { "ur", "پروڈکٹ منتخب کرنے کا علاقہ\n(یہاں پروڈکٹس لوڈ ہوں گے)" }
                }
            },
            {
                "transactions_cart_area",
                new Dictionary<string, string>
                {
                    { "en", "Transaction Cart\n(Cart items will be shown here)" },
                    { "ur", "ٹرانزیکشن کارٹ\n(کارٹ آئٹمز یہاں دکھائے جائیں گے)" }
                }
            }
        };

        // Add keywords to database - get fresh service instance
        var dbService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
        foreach (var keyword in keywords)
        {
            // Add the keyword first
            await dbService.AddLanguageKeywordAsync(keyword.Key, $"Translation key for {keyword.Key}");
            
            // Add translations for each language
            foreach (var translation in keyword.Value)
            {
                await dbService.SaveTranslationAsync(keyword.Key, translation.Value, translation.Key);
            }
        }
    }

    private async void OnLanguageChanged(object? sender, string newLanguageCode)
    {
        // Reload translations when language changes
        await LoadTranslationsAsync();
    }

    private async Task LoadTranslationsAsync()
    {
        try
        {
            Console.WriteLine("🔄 [MainWindowViewModel] Starting LoadTranslationsAsync...");
            
            // First ensure database has the basic translations
            await EnsureBasicTranslationsExistAsync();

            Console.WriteLine("🔍 [MainWindowViewModel] Loading navigation button texts...");
            
            // Load navigation button texts with improved fallback - using correct keys from database
            DashboardButtonText = await GetTranslationWithFallback("nav.dashboard", "Dashboard");
            Console.WriteLine($"✅ [MainWindowViewModel] DashboardButtonText = '{DashboardButtonText}'");
            
            SalesWindowButtonText = await GetTranslationWithFallback("nav.sales", "Sales Window");
            Console.WriteLine($"✅ [MainWindowViewModel] SalesWindowButtonText = '{SalesWindowButtonText}'");
            
            TransactionButtonText = await GetTranslationWithFallback("nav.transaction", "Transaction");
            Console.WriteLine($"✅ [MainWindowViewModel] TransactionButtonText = '{TransactionButtonText}'");
            
            BackOfficeButtonText = await GetTranslationWithFallback("nav.management", "Back Office");
            Console.WriteLine($"✅ [MainWindowViewModel] BackOfficeButtonText = '{BackOfficeButtonText}'");
            
            ReservationButtonText = await GetTranslationWithFallback("nav_reservation", "Reservation");
            Console.WriteLine($"✅ [MainWindowViewModel] ReservationButtonText = '{ReservationButtonText}'");
            
            OrderTableButtonText = await GetTranslationWithFallback("nav_order_table", "Order Table");
            Console.WriteLine($"✅ [MainWindowViewModel] OrderTableButtonText = '{OrderTableButtonText}'");
            
            ReportsButtonText = await GetTranslationWithFallback("nav_reports", "Reports");
            Console.WriteLine($"✅ [MainWindowViewModel] ReportsButtonText = '{ReportsButtonText}'");
            
            SettingsButtonText = await GetTranslationWithFallback("nav.settings", "Settings");
            Console.WriteLine($"✅ [MainWindowViewModel] SettingsButtonText = '{SettingsButtonText}'");
            
            LogoutButtonText = await GetTranslationWithFallback("nav.logout", "Logout");
            Console.WriteLine($"✅ [MainWindowViewModel] LogoutButtonText = '{LogoutButtonText}'");

            Console.WriteLine("🎯 [MainWindowViewModel] All navigation translations loaded successfully!");

            // Update page titles to translated versions
            await UpdateCurrentPageTitle();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [MainWindowViewModel] Error loading translations: {ex.Message}");
            Console.WriteLine($"❌ [MainWindowViewModel] Stack trace: {ex.StackTrace}");
            // Use hardcoded fallbacks if everything fails
            SetFallbackTexts();
        }
    }

    private async Task<string> GetTranslationWithFallback(string key, string fallback)
    {
        try
        {
            Console.WriteLine($"🔍 [MainWindowViewModel] Getting translation for key: '{key}'");
            
            var translation = await _databaseLocalizationService.GetTranslationAsync(key);
            Console.WriteLine($"📝 [MainWindowViewModel] Database returned: '{translation}' for key '{key}'");
            
            // If translation is empty, null, or same as key (indicating no translation found)
            if (string.IsNullOrEmpty(translation) || translation == key)
            {
                Console.WriteLine($"⚠️ [MainWindowViewModel] No valid translation found for '{key}', using fallback: '{fallback}'");
                return fallback;
            }
            
            Console.WriteLine($"✅ [MainWindowViewModel] Using database translation: '{translation}' for key '{key}'");
            return translation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [MainWindowViewModel] Error getting translation for '{key}': {ex.Message}");
            return fallback;
        }
    }

    private void SetFallbackTexts()
    {
        DashboardButtonText = "Dashboard";
        SalesWindowButtonText = "Sales Window";
        TransactionButtonText = "Transaction";
        BackOfficeButtonText = "Back Office";
        ReservationButtonText = "Reservation";
        OrderTableButtonText = "Order Table";
        ReportsButtonText = "Reports";
        SettingsButtonText = "Settings";
        LogoutButtonText = "Logout";
    }

    private async Task EnsureBasicTranslationsExistAsync()
    {
        try
        {
            Console.WriteLine("🔍 [MainWindowViewModel] Checking if database translations exist...");
            
            // Check if we have any translations, if not, initialize them
            var languages = await _databaseLocalizationService.GetAvailableLanguagesAsync();
            Console.WriteLine($"📊 [MainWindowViewModel] Found {languages.Count} languages in database");
            
            foreach (var lang in languages)
            {
                Console.WriteLine($"🌐 [MainWindowViewModel] Available language: {lang.LanguageName} ({lang.LanguageCode}) - RTL: {lang.IsRtl}");
            }
            
            if (!languages.Any())
            {
                Console.WriteLine("⚠️ [MainWindowViewModel] No languages found in database. Database might not be initialized!");
                return;
            }

            // Check current language
            var currentLang = await _databaseLocalizationService.GetCurrentLanguageAsync();
            Console.WriteLine($"🎯 [MainWindowViewModel] Current language: {currentLang?.LanguageName} ({currentLang?.LanguageCode})");

            // Check if we have basic navigation translations with correct keys
            var dashboardTranslation = await _databaseLocalizationService.GetTranslationAsync("nav.dashboard");
            Console.WriteLine($"🔍 [MainWindowViewModel] Testing nav.dashboard translation: '{dashboardTranslation}'");
            
            if (string.IsNullOrEmpty(dashboardTranslation) || dashboardTranslation == "nav.dashboard")
            {
                Console.WriteLine("⚠️ [MainWindowViewModel] Basic translations missing or not found with correct keys!");
            }
            else
            {
                Console.WriteLine("✅ [MainWindowViewModel] Database translations appear to be working correctly!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [MainWindowViewModel] Error checking translations: {ex.Message}");
            Console.WriteLine($"❌ [MainWindowViewModel] Stack trace: {ex.StackTrace}");
        }
    }

    private async Task InitializeDatabaseTranslations()
    {
        try
        {
            Console.WriteLine("Initializing database with basic translations...");
            
            // This should initialize languages in the database
            // For now, we'll assume English is available and add navigation translations
            await InitializeNavigationTranslations();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing database translations: {ex.Message}");
        }
    }

    private async Task InitializeNavigationTranslations()
    {
        try
        {
            var navigationTranslations = new Dictionary<string, Dictionary<string, string>>
            {
                ["nav_dashboard"] = new Dictionary<string, string>
                {
                    ["en"] = "Dashboard",
                    ["ur"] = "ڈیش بورڈ"
                },
                ["nav_transactions"] = new Dictionary<string, string>
                {
                    ["en"] = "Transactions",
                    ["ur"] = "لین دین"
                },
                ["nav_management"] = new Dictionary<string, string>
                {
                    ["en"] = "Management",
                    ["ur"] = "انتظام"
                },
                ["nav_reservation"] = new Dictionary<string, string>
                {
                    ["en"] = "Reservation",
                    ["ur"] = "بکنگ"
                },
                ["nav_order_table"] = new Dictionary<string, string>
                {
                    ["en"] = "Order Table",
                    ["ur"] = "آرڈر ٹیبل"
                },
                ["nav_reports"] = new Dictionary<string, string>
                {
                    ["en"] = "Reports",
                    ["ur"] = "رپورٹس"
                },
                ["nav_settings"] = new Dictionary<string, string>
                {
                    ["en"] = "Settings",
                    ["ur"] = "ترتیبات"
                },
                ["nav_logout"] = new Dictionary<string, string>
                {
                    ["en"] = "Logout",
                    ["ur"] = "لاگ آؤٹ"
                }
            };

            Console.WriteLine("Adding navigation translation keywords...");
            
            foreach (var keyword in navigationTranslations)
            {
                // Add the keyword first
                await _databaseLocalizationService.AddLanguageKeywordAsync(keyword.Key, $"Navigation text for {keyword.Key}");
                
                // Add translations for each language
                foreach (var translation in keyword.Value)
                {
                    await _databaseLocalizationService.SaveTranslationAsync(keyword.Key, translation.Value, translation.Key);
                    Console.WriteLine($"Added: {keyword.Key} -> {translation.Key}: {translation.Value}");
                }
            }

            // Also initialize dashboard translations
            await InitializeDashboardTranslations();

            Console.WriteLine("Navigation translations initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing navigation translations: {ex.Message}");
        }
    }

    // Add dashboard content translations too
    private async Task InitializeDashboardTranslations()
    {
        try
        {
            var dashboardTranslations = new Dictionary<string, Dictionary<string, string>>
            {
                ["dashboard_welcome"] = new Dictionary<string, string>
                {
                    ["en"] = "Welcome to ChronoPos",
                    ["ur"] = "ChronoPos میں خوش آمدید"
                },
                ["dashboard_today_sales"] = new Dictionary<string, string>
                {
                    ["en"] = "Today's Sales",
                    ["ur"] = "آج کی فروخت"
                },
                ["dashboard_total_products"] = new Dictionary<string, string>
                {
                    ["en"] = "Total Products",
                    ["ur"] = "کل مصنوعات"
                },
                ["dashboard_total_customers"] = new Dictionary<string, string>
                {
                    ["en"] = "Total Customers",
                    ["ur"] = "کل گاہک"
                }
            };

            Console.WriteLine("Adding dashboard translation keywords...");
            
            foreach (var keyword in dashboardTranslations)
            {
                // Add the keyword first
                await _databaseLocalizationService.AddLanguageKeywordAsync(keyword.Key, $"Dashboard text for {keyword.Key}");
                
                // Add translations for each language
                foreach (var translation in keyword.Value)
                {
                    await _databaseLocalizationService.SaveTranslationAsync(keyword.Key, translation.Value, translation.Key);
                    Console.WriteLine($"Added: {keyword.Key} -> {translation.Key}: {translation.Value}");
                }
            }

            Console.WriteLine("Dashboard translations initialized successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing dashboard translations: {ex.Message}");
        }
    }

    private async Task UpdateCurrentPageTitle()
    {
        // Update current page title based on selected page
        CurrentPageTitle = SelectedPage switch
        {
            "Dashboard" => await _databaseLocalizationService.GetTranslationAsync("nav_dashboard") ?? "Dashboard",
            "Transactions" => await _databaseLocalizationService.GetTranslationAsync("nav_transactions") ?? "Transactions",
            "Management" => await _databaseLocalizationService.GetTranslationAsync("nav_management") ?? "Management",
            "Reservation" => await _databaseLocalizationService.GetTranslationAsync("nav_reservation") ?? "Reservation",
            "OrderTable" => await _databaseLocalizationService.GetTranslationAsync("nav_order_table") ?? "Order Table",
            "Reports" => await _databaseLocalizationService.GetTranslationAsync("nav_reports") ?? "Reports",
            "Settings" => await _databaseLocalizationService.GetTranslationAsync("nav_settings") ?? "Settings",
            _ => CurrentPageTitle
        };
    }

    [RelayCommand]
    private async Task ShowDashboard()
    {
        // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.DASHBOARD))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Dashboard screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        SelectedPage = "Dashboard";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_dashboard") ?? "Dashboard";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_dashboard_loaded") ?? "Dashboard loaded";
        
        try
        {
            // Create the comprehensive dashboard view with ViewModel
            var dashboardViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
            
            // Wire up navigation actions
            dashboardViewModel.NavigateToProductsAction = () => ShowProductManagement();
            dashboardViewModel.NavigateToTransactionsAction = () => _ = ShowTransaction();
            dashboardViewModel.NavigateToCustomersAction = () => _ = ShowCustomerManagement();
            dashboardViewModel.NavigateToStockManagementAction = () => _ = ShowStockManagement();
            dashboardViewModel.NavigateToNewSaleAction = () => _ = ShowTransactions();
            dashboardViewModel.NavigateToReportsAction = () => ShowReports();
            
            var dashboardView = new DashboardView
            {
                DataContext = dashboardViewModel
            };
            
            CurrentView = dashboardView;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading dashboard: {ex.Message}");
            AppLogger.LogError(
                $"Failed to load dashboard: {ex.Message}", 
                ex, 
                "MainWindowViewModel.ShowDashboard");
            
            StatusMessage = "Error loading dashboard";
            new MessageDialog(
                "Error",
                "Failed to load the dashboard. Please try again.",
                MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private async Task ShowTransactions()
    {
        // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SALES_WINDOW))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Sales Window screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        SelectedPage = "Transactions";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_transactions") ?? "Add Sales";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_transactions_loaded") ?? "Sales interface loaded";
        
        try
        {
            // Create the AddSalesViewModel with all required services
            var addSalesViewModel = new AddSalesViewModel(
                _serviceProvider,
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<ICategoryService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ITransactionService>(),
                _serviceProvider.GetRequiredService<IRestaurantTableService>(),
                _serviceProvider.GetRequiredService<IReservationService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _serviceProvider.GetRequiredService<IShiftService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _serviceProvider.GetRequiredService<IRefundService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _serviceProvider.GetRequiredService<ITransactionServiceChargeRepository>(),
                _serviceProvider.GetRequiredService<ITransactionModifierRepository>(),
                _productBarcodeRepository,
                _serviceProvider.GetRequiredService<IActiveCurrencyService>(),
                _serviceProvider.GetRequiredService<IServiceChargeOptionService>(),
                _serviceProvider.GetRequiredService<ICompanySettingsService>(),
                _serviceProvider.GetRequiredService<InfrastructureServices.IDatabaseLocalizationService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                navigateToTransactionList: async () => await ShowTransaction(),
                navigateToRefundTransaction: async (transactionId) => await LoadTransactionForRefund(transactionId),
                navigateToExchangeTransaction: async (transactionId) => await LoadTransactionForExchange(transactionId)
            );

            // Create the AddSalesView and set its DataContext
            var addSalesView = new AddSalesView
            {
                DataContext = addSalesViewModel
            };

            CurrentView = addSalesView;
            
            AppLogger.Log("Add Sales screen loaded successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = "Error loading Add Sales screen";
            AppLogger.LogError("Error loading Add Sales screen", ex);
            new MessageDialog("Error", $"Error loading Add Sales screen: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private async Task ShowTransaction(string? selectedTab = null)
    {
        // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.TRANSACTION))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Transaction screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        SelectedPage = "Transaction";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_transaction") ?? "Transaction";
        StatusMessage = "Transaction interface loaded";
        
        // Create the TransactionViewModel with required services and navigation callbacks
        try
        {
            var transactionViewModel = new TransactionViewModel(
                _serviceProvider.GetRequiredService<ITransactionService>(),
                _serviceProvider.GetRequiredService<IRefundService>(),
                _serviceProvider.GetRequiredService<IExchangeService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _serviceProvider.GetRequiredService<IReservationService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _activeCurrencyService,
                _serviceProvider.GetRequiredService<IProductService>(),
                _databaseLocalizationService,
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                navigateToEditTransaction: async (transactionId) => await LoadTransactionForEdit(transactionId),
                navigateToPayBill: async (transactionId) => await LoadTransactionForPayment(transactionId),
                navigateToRefundTransaction: async (transactionId) => await LoadTransactionForRefund(transactionId),
                navigateToExchangeTransaction: async (transactionId) => await LoadTransactionForExchange(transactionId),
                navigateToAddSales: async () => await ShowTransactions() // Fixed: ShowTransactions (plural) opens Add Sales screen
            );

            // Create the TransactionView and set its DataContext
            var transactionView = new TransactionView
            {
                DataContext = transactionViewModel
            };

            CurrentView = transactionView;
            
            // If a specific tab is requested, switch to it
            if (!string.IsNullOrEmpty(selectedTab))
            {
                await Task.Delay(100); // Wait for view to render
                
                switch (selectedTab.ToLowerInvariant())
                {
                    case "sales":
                        transactionViewModel.SwitchToSalesCommand.Execute(null);
                        break;
                    case "refund":
                        transactionViewModel.SwitchToRefundCommand.Execute(null);
                        break;
                    case "exchange":
                        transactionViewModel.SwitchToExchangeCommand.Execute(null);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading transaction screen: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task LoadTransactionForEdit(int transactionId)
    {
        try
        {
            // Load the transaction
            var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
            var transaction = await transactionService.GetByIdAsync(transactionId);
            
            if (transaction == null)
            {
                new MessageDialog("Error", "Transaction not found!", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            // Create a fresh AddSalesViewModel
            var addSalesViewModel = new AddSalesViewModel(
                _serviceProvider,
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<ICategoryService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ITransactionService>(),
                _serviceProvider.GetRequiredService<IRestaurantTableService>(),
                _serviceProvider.GetRequiredService<IReservationService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _serviceProvider.GetRequiredService<IShiftService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _serviceProvider.GetRequiredService<IRefundService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _serviceProvider.GetRequiredService<ITransactionServiceChargeRepository>(),
                _serviceProvider.GetRequiredService<ITransactionModifierRepository>(),
                _productBarcodeRepository,
                _serviceProvider.GetRequiredService<IActiveCurrencyService>(),
                _serviceProvider.GetRequiredService<IServiceChargeOptionService>(),
                _serviceProvider.GetRequiredService<ICompanySettingsService>(),
                _serviceProvider.GetRequiredService<InfrastructureServices.IDatabaseLocalizationService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                navigateToTransactionList: async () => await ShowTransaction(),
                navigateToRefundTransaction: async (transactionId) => await LoadTransactionForRefund(transactionId),
                navigateToExchangeTransaction: async (transactionId) => await LoadTransactionForExchange(transactionId)
            );

            // Create the AddSalesView and set its DataContext
            var addSalesView = new AddSalesView
            {
                DataContext = addSalesViewModel
            };

            // Navigate to Add Sales screen first
            SelectedPage = "Transactions";
            CurrentPageTitle = "Edit Transaction";
            CurrentView = addSalesView;
            
            // Load the transaction data for editing
            await addSalesViewModel.LoadTransactionForEdit(transactionId);
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading transaction for edit: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task LoadTransactionForPayment(int transactionId)
    {
        try
        {
            // Load the transaction
            var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
            var transaction = await transactionService.GetByIdAsync(transactionId);
            
            if (transaction == null)
            {
                new MessageDialog("Error", "Transaction not found!", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            // Create a fresh AddSalesViewModel
            var addSalesViewModel = new AddSalesViewModel(
                _serviceProvider,
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<ICategoryService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ITransactionService>(),
                _serviceProvider.GetRequiredService<IRestaurantTableService>(),
                _serviceProvider.GetRequiredService<IReservationService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _serviceProvider.GetRequiredService<IShiftService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _serviceProvider.GetRequiredService<IRefundService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _serviceProvider.GetRequiredService<ITransactionServiceChargeRepository>(),
                _serviceProvider.GetRequiredService<ITransactionModifierRepository>(),
                _productBarcodeRepository,
                _serviceProvider.GetRequiredService<IActiveCurrencyService>(),
                _serviceProvider.GetRequiredService<IServiceChargeOptionService>(),
                _serviceProvider.GetRequiredService<ICompanySettingsService>(),
                _serviceProvider.GetRequiredService<InfrastructureServices.IDatabaseLocalizationService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                navigateToTransactionList: async () => await ShowTransaction(),
                navigateToRefundTransaction: async (transactionId) => await LoadTransactionForRefund(transactionId),
                navigateToExchangeTransaction: async (transactionId) => await LoadTransactionForExchange(transactionId)
            );

            // Create the AddSalesView and set its DataContext
            var addSalesView = new AddSalesView
            {
                DataContext = addSalesViewModel
            };

            // Navigate to Add Sales screen first
            SelectedPage = "Transactions";
            CurrentPageTitle = "Payment";
            CurrentView = addSalesView;
            
            // Load the transaction data for payment
            await addSalesViewModel.LoadTransactionForPayment(transactionId);
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading transaction for payment: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// Load transaction for refund processing
    /// </summary>
    public async Task LoadTransactionForRefund(int transactionId)
    {
        try
        {
            // Load the transaction
            var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
            var transaction = await transactionService.GetByIdAsync(transactionId);
            
            if (transaction == null)
            {
                new MessageDialog("Error", "Transaction not found!", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            if (transaction.Status.ToLower() != "settled")
            {
                new MessageDialog("Invalid Status", "Only settled transactions can be refunded.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Create a fresh AddSalesViewModel
            var addSalesViewModel = new AddSalesViewModel(
                _serviceProvider,
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<ICategoryService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ITransactionService>(),
                _serviceProvider.GetRequiredService<IRestaurantTableService>(),
                _serviceProvider.GetRequiredService<IReservationService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _serviceProvider.GetRequiredService<IShiftService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _serviceProvider.GetRequiredService<IRefundService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _serviceProvider.GetRequiredService<ITransactionServiceChargeRepository>(),
                _serviceProvider.GetRequiredService<ITransactionModifierRepository>(),
                _productBarcodeRepository,
                _serviceProvider.GetRequiredService<IActiveCurrencyService>(),
                _serviceProvider.GetRequiredService<IServiceChargeOptionService>(),
                _serviceProvider.GetRequiredService<ICompanySettingsService>(),
                _serviceProvider.GetRequiredService<InfrastructureServices.IDatabaseLocalizationService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                navigateToTransactionList: async () => await ShowTransaction(),
                navigateToRefundTransaction: async (transactionId) => await LoadTransactionForRefund(transactionId),
                navigateToExchangeTransaction: async (transactionId) => await LoadTransactionForExchange(transactionId)
            );

            // Create the AddSalesView and set its DataContext
            var addSalesView = new AddSalesView
            {
                DataContext = addSalesViewModel
            };

            // Refunds are now handled via popup in Transaction screen
            // Navigate to transaction list instead
            await ShowTransaction();
            
            new MessageDialog("Refund", 
                "Refunds are now processed from the Transaction screen.\n\n" +
                "Click the 'Refund' button on the transaction card to process a refund.", 
                MessageDialog.MessageType.Info).ShowDialog();
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    public async Task LoadTransactionForExchange(int transactionId)
    {
        try
        {
            // Load the transaction
            var transactionService = _serviceProvider.GetRequiredService<ITransactionService>();
            var transaction = await transactionService.GetByIdAsync(transactionId);
            
            if (transaction == null)
            {
                new MessageDialog("Error", "Transaction not found!", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            if (transaction.Status.ToLower() != "settled")
            {
                new MessageDialog("Invalid Status", "Only settled transactions can be exchanged.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Create a fresh ExchangeSalesViewModel
            var exchangeSalesViewModel = new ExchangeSalesViewModel(
                _serviceProvider.GetRequiredService<ITransactionService>(),
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<IExchangeService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _activeCurrencyService,
                onExchangeComplete: async () => await ShowTransaction(),
                onBack: async () => await ShowTransaction()
            );

            // Create the ExchangeSalesView and set its DataContext
            var exchangeSalesView = new ExchangeSalesView
            {
                DataContext = exchangeSalesViewModel
            };

            // Navigate to Exchange screen
            SelectedPage = "Transactions";
            CurrentPageTitle = "Exchange Transaction";
            CurrentView = exchangeSalesView;
            
            // Initialize the view model and then load transaction
            await exchangeSalesViewModel.InitializeAsync();
            await exchangeSalesViewModel.LoadTransaction(transactionId);
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading transaction for exchange: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private async Task ShowManagement()
    {
        // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.BACK_OFFICE))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Back Office screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        SelectedPage = "Management";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_management") ?? "Management";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_loading_management") ?? "Loading management...";
        
        try
        {
            // Create the ManagementViewModel with all required services
            var managementViewModel = new ManagementViewModel(
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>(),
                _currentUserService
            );

            // Set up navigation from management to specific modules
            managementViewModel.NavigateToModuleAction = (moduleType) =>
            {
                switch (moduleType)
                {
                    case "Product":
                        ShowProductManagement();
                        break;
                    case "Stock":
                        _ = ShowStockManagement();
                        break;
                    case "CustomerManagement":
                        _ = ShowCustomerManagement();
                        break;
                    case "SupplierManagement":
                        _ = ShowSupplierManagement();
                        break;
                    case "AddOptions":
                        _ = ShowAddOptions();
                        break;
                    default:
                        StatusMessage = $"Navigation to {moduleType} not implemented yet";
                        break;
                }
            };

            // Create the ManagementView and set its DataContext
            var managementView = new ManagementView
            {
                DataContext = managementViewModel
            };

            CurrentView = managementView;
            StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_management_loaded") ?? "Management modules loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading management: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            var managementTitle = await _databaseLocalizationService.GetTranslationAsync("nav_management") ?? "Management";
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = managementTitle, 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 10) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading management modules: {ex.Message}",
                FontStyle = System.Windows.FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    private async Task ShowCustomerManagement()
    {
        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Customer Management";
        StatusMessage = "Loading customer management...";
        
        try
        {
            // Create the CustomerManagementViewModel with all required services
            var customerManagementViewModel = new CustomerManagementViewModel(
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ICustomerGroupService>()
            );

            // Set up navigation from customer management to specific modules
            customerManagementViewModel.NavigateToModuleAction = (moduleType) =>
            {
                switch (moduleType)
                {
                    case "Customers":
                        _ = ShowCustomers();
                        break;
                    case "CustomerGroups":
                        _ = ShowCustomerGroups();
                        break;
                    default:
                        StatusMessage = $"Navigation to {moduleType} not implemented yet";
                        break;
                }
            };

            // Set up back navigation
            customerManagementViewModel.GoBackAction = () =>
            {
                ShowManagementCommand.Execute(null);
            };

            // Create the CustomerManagementView and set its DataContext
            var customerManagementView = new CustomerManagementView
            {
                DataContext = customerManagementViewModel
            };

            CurrentView = customerManagementView;
            StatusMessage = "Customer management loaded successfully";
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading customer management: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Customer Management", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 10) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading customer management: {ex.Message}",
                FontStyle = System.Windows.FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    private async Task ShowSupplierManagement()
    {
        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Supplier Management";
        StatusMessage = "Loading supplier management...";
        
        try
        {
            // Create the SupplierManagementViewModel with all required services
            var supplierManagementViewModel = new SupplierManagementViewModel(
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<ISupplierService>()
            );

            // Set up navigation from supplier management to specific modules
            supplierManagementViewModel.NavigateToModuleAction = (moduleType) =>
            {
                switch (moduleType)
                {
                    case "Suppliers":
                        _ = ShowSuppliers();
                        break;
                    default:
                        StatusMessage = $"Navigation to {moduleType} not implemented yet";
                        break;
                }
            };

            // Set up back navigation
            supplierManagementViewModel.GoBackAction = () =>
            {
                ShowManagementCommand.Execute(null);
            };

            // Create the SupplierManagementView and set its DataContext
            var supplierManagementView = new SupplierManagementView
            {
                DataContext = supplierManagementViewModel
            };

            CurrentView = supplierManagementView;
            StatusMessage = "Supplier management loaded successfully";
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading supplier management: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Supplier Management", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 10) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading supplier management: {ex.Message}",
                FontStyle = System.Windows.FontStyles.Italic,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    [RelayCommand]
    private void ShowProductManagement()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PRODUCT_MANAGEMENT))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Product Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Product Management";
        StatusMessage = "Loading product management...";
        
        try
        {
            // Create the ProductManagementViewModel with proper navigation delegates
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var discountService = _serviceProvider.GetRequiredService<IDiscountService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            var activeCurrencyService = _serviceProvider.GetRequiredService<IActiveCurrencyService>();
            var barcodeExportService = _serviceProvider.GetRequiredService<IBarcodeExportService>();
            
            var productManagementViewModel = new ProductManagementViewModel(
                productService,
                discountService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                _currentUserService,
                activeCurrencyService,
                barcodeExportService,
                navigateToAddProduct: ShowAddProduct,  // Pass the ShowAddProduct method as delegate
                navigateToEditProduct: async (product) => await ShowEditProduct(product),  // Pass the ShowEditProduct method as delegate
                navigateBack: () => _ = ShowManagement()  // Async wrapper for back navigation
            );
            
            // Create the view and set the ViewModel
            var productManagementView = new ProductManagementView();
            productManagementView.DataContext = productManagementViewModel;
            CurrentView = productManagementView;
            
            StatusMessage = "Product management loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading product management: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Product Management", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 20) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading product management: {ex.Message}",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    private void ShowProductAttributes()
    {
        ChronoPos.Desktop.Services.FileLogger.Log("🔧 ShowProductAttributes method started");
        
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PRODUCT_ATTRIBUTES))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Product Attributes Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Product Attributes";
        StatusMessage = "Loading product attributes...";
        
        try
        {
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Getting ProductAttributeService from DI container");
            // Create the ProductAttributeViewModel
            var productAttributeService = _serviceProvider.GetRequiredService<IProductAttributeService>();
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ProductAttributeService retrieved successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Creating ProductAttributeViewModel");
            var productAttributeViewModel = new ProductAttributeViewModel(
                productAttributeService,
                _currentUserService,
                _databaseLocalizationService,
                () => _ = ShowAddOptions() // Navigate back to Others
            );
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ProductAttributeViewModel created successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Creating ProductAttributeView");
            // Create the view and set the ViewModel
            var productAttributeView = new ProductAttributeView();
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ProductAttributeView created successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Setting DataContext");
            productAttributeView.DataContext = productAttributeViewModel;
            ChronoPos.Desktop.Services.FileLogger.Log("✅ DataContext set successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Setting CurrentView");
            CurrentView = productAttributeView;
            ChronoPos.Desktop.Services.FileLogger.Log("✅ CurrentView set successfully");
            
            StatusMessage = "Product attributes loaded successfully";
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ShowProductAttributes completed successfully");
        }
        catch (Exception ex)
        {
            ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in ShowProductAttributes: {ex.Message}");
            ChronoPos.Desktop.Services.FileLogger.Log($"❌ ShowProductAttributes stack trace: {ex.StackTrace}");
            StatusMessage = $"Error loading product attributes: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Product Attributes", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 20) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading product attributes: {ex.Message}",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    private async Task ShowProductAttributesWithAddPanel()
    {
        // First open the Product Attributes screen
        ShowProductAttributes();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the ProductAttributeViewModel from the CurrentView
        if (CurrentView is ProductAttributeView attributeView && 
            attributeView.DataContext is ProductAttributeViewModel attributeViewModel)
        {
            // Trigger the AddAttributeCommand
            if (attributeViewModel.AddAttributeCommand.CanExecute(null))
            {
                attributeViewModel.AddAttributeCommand.Execute(null);
            }
        }
    }

    private void ShowProductModifiers()
    {
        ChronoPos.Desktop.Services.FileLogger.Log("🔧 ShowProductModifiers method started");
        
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PRODUCT_MODIFIERS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Product Modifiers Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Product Modifiers";
        StatusMessage = "Loading product modifiers...";
        
        try
        {
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Getting ProductModifierService and related services from DI container");
            // Create the ProductModifierViewModel
            var productModifierService = _serviceProvider.GetRequiredService<IProductModifierService>();
            var productModifierGroupService = _serviceProvider.GetRequiredService<IProductModifierGroupService>();
            var productModifierGroupItemService = _serviceProvider.GetRequiredService<IProductModifierGroupItemService>();
            var taxTypeService = _serviceProvider.GetRequiredService<ITaxTypeService>();
            ChronoPos.Desktop.Services.FileLogger.Log("✅ Services retrieved successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Creating ProductModifierViewModel");
            var productModifierViewModel = new ProductModifierViewModel(
                productModifierService,
                productModifierGroupService,
                productModifierGroupItemService,
                _currentUserService,
                taxTypeService,
                _activeCurrencyService,
                _databaseLocalizationService,
                () => _ = ShowAddOptions() // Navigate back to Others
            );
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ProductModifierViewModel created successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Creating ProductModifierView");
            // Create the view and set the ViewModel
            var productModifierView = new ProductModifierView();
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ProductModifierView created successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Setting DataContext");
            productModifierView.DataContext = productModifierViewModel;
            ChronoPos.Desktop.Services.FileLogger.Log("✅ DataContext set successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Setting CurrentView");
            CurrentView = productModifierView;
            ChronoPos.Desktop.Services.FileLogger.Log("✅ CurrentView set successfully");
            
            StatusMessage = "Product modifiers loaded successfully";
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ShowProductModifiers completed successfully");
        }
        catch (Exception ex)
        {
            ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in ShowProductModifiers: {ex.Message}");
            ChronoPos.Desktop.Services.FileLogger.Log($"❌ ShowProductModifiers stack trace: {ex.StackTrace}");
            StatusMessage = $"Error loading product modifiers: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Product Modifiers", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 20) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading product modifiers: {ex.Message}",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    private async Task ShowProductModifiersWithAddPanel()
    {
        // First open the Product Modifiers screen
        ShowProductModifiers();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the ProductModifierViewModel from the CurrentView
        if (CurrentView is ProductModifierView modifierView && 
            modifierView.DataContext is ProductModifierViewModel modifierViewModel)
        {
            // Trigger the AddModifierGroupCommand
            if (modifierViewModel.AddModifierGroupCommand.CanExecute(null))
            {
                modifierViewModel.AddModifierGroupCommand.Execute(null);
            }
        }
    }

    private async Task ShowServiceCharges()
    {
        ChronoPos.Desktop.Services.FileLogger.Log("🔧 ShowServiceCharges method started");
        
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SERVICE_CHARGE))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Service Charge Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Settings" so sidebar stays highlighted
        CurrentPageTitle = "Service Charges";
        StatusMessage = "Loading service charges...";
        
        try
        {
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Getting ServiceChargeTypeService and ServiceChargeOptionService from DI container");
            // Create the ServiceChargeViewModel
            var serviceChargeTypeService = _serviceProvider.GetRequiredService<IServiceChargeTypeService>();
            var serviceChargeOptionService = _serviceProvider.GetRequiredService<IServiceChargeOptionService>();
            ChronoPos.Desktop.Services.FileLogger.Log("✅ Services retrieved successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Creating ServiceChargeViewModel");
            var serviceChargeViewModel = new ServiceChargeViewModel(
                serviceChargeTypeService,
                serviceChargeOptionService,
                _currentUserService,
                () => _ = ShowSettings() // Navigate back to Settings
            );
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ServiceChargeViewModel created successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Creating ServiceChargeView");
            // Create the view and set the ViewModel
            var serviceChargeView = new ServiceChargeView();
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ServiceChargeView created successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Setting DataContext");
            serviceChargeView.DataContext = serviceChargeViewModel;
            ChronoPos.Desktop.Services.FileLogger.Log("✅ DataContext set successfully");
            
            ChronoPos.Desktop.Services.FileLogger.Log("🔧 Setting CurrentView");
            CurrentView = serviceChargeView;
            ChronoPos.Desktop.Services.FileLogger.Log("✅ CurrentView set successfully");
            
            StatusMessage = "Service charges loaded successfully";
            ChronoPos.Desktop.Services.FileLogger.Log("✅ ShowServiceCharges completed successfully");
        }
        catch (Exception ex)
        {
            ChronoPos.Desktop.Services.FileLogger.Log($"❌ Error in ShowServiceCharges: {ex.Message}");
            ChronoPos.Desktop.Services.FileLogger.Log($"❌ ShowServiceCharges stack trace: {ex.StackTrace}");
            StatusMessage = $"Error loading service charges: {ex.Message}";
            
            new MessageDialog(
                "Service Charges Error",
                $"Failed to load service charges: {ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                MessageDialog.MessageType.Error).ShowDialog();
        }

        await Task.CompletedTask;
    }

    private async Task ShowProductCombinations()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PRODUCT_COMBINATIONS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Product Combinations Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        CurrentPageTitle = "Product Combinations";
        StatusMessage = "Loading product combinations...";
        
        try
        {
            // Get services from DI container
            var combinationService = _serviceProvider.GetRequiredService<IProductCombinationItemService>();
            var productUnitService = _serviceProvider.GetRequiredService<IProductUnitService>();
            var attributeService = _serviceProvider.GetRequiredService<IProductAttributeService>();
            
            // Create the ProductCombinationViewModel with back navigation
            var productCombinationViewModel = new ProductCombinationViewModel(
                combinationService,
                productUnitService,
                attributeService,
                _currentUserService,
                _activeCurrencyService,
                _databaseLocalizationService,
                () => _ = ShowAddOptions() // Navigate back to Others
            );
            
            // Create the view and set the ViewModel
            var productCombinationView = new ProductCombinationView
            {
                DataContext = productCombinationViewModel
            };
            
            CurrentView = productCombinationView;
            StatusMessage = "Product combinations loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading product combinations: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Product Combinations", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 20) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading product combinations: {ex.Message}",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    private async Task ShowProductCombinationsWithAddPanel()
    {
        // First open the Product Combinations screen
        await ShowProductCombinations();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the ProductCombinationViewModel from the CurrentView
        if (CurrentView is ProductCombinationView combinationView && 
            combinationView.DataContext is ProductCombinationViewModel combinationViewModel)
        {
            // Trigger the AddCombinationCommand (passing null to add for all product units)
            if (combinationViewModel.AddCombinationCommand.CanExecute(null))
            {
                combinationViewModel.AddCombinationCommand.Execute(null);
            }
        }
    }

    [RelayCommand]
    private void ShowAddProduct()
    {
        CurrentPageTitle = "Add Product";
        StatusMessage = "Loading add product form...";
        
        try
        {
            // Create the AddProductView and manually create ViewModel with navigation callback
            var addProductView = new AddProductView();
            
            // Get services from DI container
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            var brandService = _serviceProvider.GetRequiredService<IBrandService>();
            var productImageService = _serviceProvider.GetRequiredService<IProductImageService>();
            var taxTypeService = _serviceProvider.GetRequiredService<ITaxTypeService>();
            var discountService = _serviceProvider.GetRequiredService<IDiscountService>();
            var productUnitService = _serviceProvider.GetRequiredService<IProductUnitService>();
            var skuGenerationService = _serviceProvider.GetRequiredService<ISkuGenerationService>();
            var activeCurrencyService = _serviceProvider.GetRequiredService<IActiveCurrencyService>();
            
            // Create ViewModel with navigation callback
            var addProductViewModel = new AddProductViewModel(
                productService,
                brandService,
                productImageService,
                taxTypeService,
                discountService,
                productUnitService,
                skuGenerationService,
                _serviceProvider.GetRequiredService<IProductBatchService>(),
                activeCurrencyService,
                _serviceProvider.GetRequiredService<IProductModifierGroupService>(),
                _serviceProvider.GetRequiredService<IProductModifierLinkService>(),
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                navigateBack: () => ShowProductManagement() // Navigate back to product management
            );
            
            addProductView.DataContext = addProductViewModel;
            CurrentView = addProductView;
            
            StatusMessage = "Add product form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading add product form: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Add Product", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 20) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading add product form: {ex.Message}",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    [RelayCommand]
    private async Task ShowEditProduct(ProductDto product)
    {
        CurrentPageTitle = "Edit Product";
        StatusMessage = "Loading edit product form...";
        
        try
        {
            // Create the AddProductView and manually create ViewModel with navigation callback
            var addProductView = new AddProductView();
            
            // Get services from DI container
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            var brandService = _serviceProvider.GetRequiredService<IBrandService>();
            var productImageService = _serviceProvider.GetRequiredService<IProductImageService>();
            var taxTypeService = _serviceProvider.GetRequiredService<ITaxTypeService>();
            var discountService = _serviceProvider.GetRequiredService<IDiscountService>();
            var productUnitService = _serviceProvider.GetRequiredService<IProductUnitService>();
            var skuGenerationService = _serviceProvider.GetRequiredService<ISkuGenerationService>();
            var activeCurrencyService = _serviceProvider.GetRequiredService<IActiveCurrencyService>();
            
            // Create ViewModel with navigation callback
            var addProductViewModel = new AddProductViewModel(
                productService,
                brandService,
                productImageService,
                taxTypeService,
                discountService,
                productUnitService,
                skuGenerationService,
                _serviceProvider.GetRequiredService<IProductBatchService>(),
                activeCurrencyService,
                _serviceProvider.GetRequiredService<IProductModifierGroupService>(),
                _serviceProvider.GetRequiredService<IProductModifierLinkService>(),
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                navigateBack: () => ShowProductManagement() // Navigate back to product management
            );
            
            // Load the product data for editing
            await addProductViewModel.LoadProductForEdit(product);
            
            addProductView.DataContext = addProductViewModel;
            CurrentView = addProductView;
            
            StatusMessage = "Edit product form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading edit product form: {ex.Message}";
            
            // Fallback to simple error display
            var errorContent = new System.Windows.Controls.StackPanel();
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = "Edit Product", 
                FontSize = 16, 
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 0, 0, 20) 
            });
            errorContent.Children.Add(new System.Windows.Controls.TextBlock 
            { 
                Text = $"Error loading edit product form: {ex.Message}",
                FontSize = 12,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });
            
            CurrentView = errorContent;
        }
    }

    /// <summary>
    /// Opens the customer management screen with the customer edit side panel visible
    /// </summary>
    [RelayCommand]
    private async Task ShowEditCustomer(CustomerDto customer)
    {
        CurrentPageTitle = "Edit Customer";
        StatusMessage = $"Loading customer: {customer.DisplayName}...";
        
        try
        {
            // Navigate to Customers screen first
            await ShowCustomers();
            
            // Wait for the view to load
            await Task.Delay(100);
            
            // Now trigger the side panel to open with the customer
            if (CurrentView is CustomersView customersView && 
                customersView.DataContext is CustomersViewModel customersViewModel)
            {
                // Call the ShowEditCustomerSidePanelCommand to open the side panel
                customersViewModel.ShowEditCustomerSidePanelCommand?.Execute(customer);
                StatusMessage = $"Editing customer: {customer.DisplayName}";
            }
            else
            {
                StatusMessage = "Customer loaded - please select to edit";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading edit customer form: {ex.Message}";
            AppLogger.LogError($"Error loading edit customer form", ex);
            
            // Fallback to customer management
            await ShowCustomerManagement();
        }
    }

    /// <summary>
    /// Opens the supplier management screen with the supplier edit side panel visible
    /// </summary>
    [RelayCommand]
    private async Task ShowEditSupplier(SupplierDto supplier)
    {
        CurrentPageTitle = "Edit Supplier";
        StatusMessage = $"Loading supplier: {supplier.DisplayName}...";
        
        try
        {
            // Navigate to Suppliers screen first
            await ShowSuppliers();
            
            // Wait for the view to load
            await Task.Delay(100);
            
            // Now trigger the side panel to open with the supplier
            if (CurrentView is SuppliersView suppliersView && 
                suppliersView.DataContext is SuppliersViewModel suppliersViewModel)
            {
                // Call the EditSupplierCommand to open the side panel
                suppliersViewModel.EditSupplierCommand?.Execute(supplier);
                StatusMessage = $"Editing supplier: {supplier.DisplayName}";
            }
            else
            {
                StatusMessage = "Supplier loaded - please select to edit";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading edit supplier form: {ex.Message}";
            AppLogger.LogError($"Error loading edit supplier form", ex);
            
            // Fallback to supplier management
            await ShowSupplierManagement();
        }
    }

    [RelayCommand]
    private async Task ShowStockManagement(string? selectedSection = null)
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.STOCK_MANAGEMENT))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Stock Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Stock Management";
        StatusMessage = "Loading stock management...";
        
        try
        {
            // Get services from the service provider for theme integration
            var themeService = _serviceProvider.GetService<IThemeService>();
            var colorSchemeService = _serviceProvider.GetService<IColorSchemeService>();
            var localizationService = _serviceProvider.GetService<ILocalizationService>();
            var zoomService = _serviceProvider.GetService<IZoomService>();
            var layoutDirectionService = _serviceProvider.GetService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetService<IDatabaseLocalizationService>();

            // Create StockManagementViewModel manually with navigation callback
            var stockManagementViewModel = new StockManagementViewModel(
                themeService ?? throw new InvalidOperationException("ThemeService is required"),
                zoomService ?? throw new InvalidOperationException("ZoomService is required"),
                localizationService ?? throw new InvalidOperationException("LocalizationService is required"),
                colorSchemeService ?? throw new InvalidOperationException("ColorSchemeService is required"),
                layoutDirectionService ?? throw new InvalidOperationException("LayoutDirectionService is required"),
                fontService ?? throw new InvalidOperationException("FontService is required"),
                databaseLocalizationService ?? throw new InvalidOperationException("DatabaseLocalizationService is required"),
                _currentUserService,
                _activeCurrencyService,
                _serviceProvider.GetService<IProductService>(),
                _serviceProvider.GetService<IStockAdjustmentService>(),
                _serviceProvider.GetService<IProductBatchService>(),
                _serviceProvider.GetService<IGoodsReceivedService>(),
                _serviceProvider.GetService<ISupplierService>(),
                _serviceProvider.GetService<IStockTransferService>(),
                _serviceProvider.GetService<IGoodsReturnService>(), // Add GoodsReturnService
                _serviceProvider.GetService<IGoodsReplaceService>(), // Add GoodsReplaceService
                navigateToAddGrn: ShowAddGrn, // Pass navigation callback
                navigateToEditGrn: ShowEditGrn, // Pass edit navigation callback
                navigateToAddStockTransfer: ShowAddStockTransfer, // Pass stock transfer navigation callback
                navigateToEditStockTransfer: ShowEditStockTransfer, // Pass edit stock transfer navigation callback
                navigateToAddGoodsReturn: ShowAddGoodsReturn, // Pass goods return navigation callback
                navigateToEditGoodsReturn: ShowEditGoodsReturn, // Pass edit goods return navigation callback
                navigateToAddGoodsReplace: ShowAddGoodsReplace, // Pass goods replace navigation callback
                navigateToEditGoodsReplace: ShowEditGoodsReplace // Pass edit goods replace navigation callback
            );
            
            // Create the view and set the ViewModel from DI
            var stockManagementView = new StockManagementView();
            stockManagementView.DataContext = stockManagementViewModel;

            // Set up the back command to return to dashboard properly
            stockManagementViewModel.GoBackCommand = new RelayCommand(() =>
            {
                ShowManagementCommand.Execute(null);
            });

            // Set the DataContext to the proper ViewModel
            stockManagementView.DataContext = stockManagementViewModel;

            // If a specific section is requested, navigate to it
            if (!string.IsNullOrEmpty(selectedSection))
            {
                stockManagementViewModel.SelectModuleCommand.Execute(selectedSection);
            }

            CurrentView = stockManagementView;
            StatusMessage = "Stock management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading stock management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private void ShowAddGrn()
    {
        CurrentPageTitle = "Add Goods Received Note";
        StatusMessage = "Loading add GRN form...";
        
        try
        {
            // Create the AddGrnView and manually create ViewModel with navigation callback
            var addGrnView = new AddGrnView();
            
            // Get services from DI container
            var goodsReceivedService = _serviceProvider.GetRequiredService<IGoodsReceivedService>();
            var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback
            var addGrnViewModel = new AddGrnViewModel(
                goodsReceivedService,
                supplierService,
                storeService,
                productService,
                uomService,
                productBatchService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                _activeCurrencyService,
                navigateBack: () => _ = ShowStockManagement("GoodsReceived") // Navigate back to stock management Goods Received section
            );
            
            addGrnView.DataContext = addGrnViewModel;
            CurrentView = addGrnView;
            StatusMessage = "Add GRN form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading add GRN form: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }
    
    private async void ShowEditGrn(long grnId)
    {
        CurrentPageTitle = "Edit Goods Received Note";
        StatusMessage = "Loading GRN for editing...";
        
        try
        {
            AppLogger.LogInfo($"Opening GRN for editing", $"GRN ID: {grnId}", "navigation");
            
            // Create the AddGrnView (same view used for add/edit)
            var addGrnView = new AddGrnView();
            
            // Get services from DI container
            var goodsReceivedService = _serviceProvider.GetRequiredService<IGoodsReceivedService>();
            var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback
            var addGrnViewModel = new AddGrnViewModel(
                goodsReceivedService,
                supplierService,
                storeService,
                productService,
                uomService,
                productBatchService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                _activeCurrencyService,
                navigateBack: () => _ = ShowStockManagement("GoodsReceived") // Navigate back to stock management Goods Received section
            );
            
            // Load the GRN data for editing
            await addGrnViewModel.LoadForEditAsync(grnId);
            
            addGrnView.DataContext = addGrnViewModel;
            CurrentView = addGrnView;
            StatusMessage = "GRN loaded for editing successfully";
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to load GRN for editing", ex, $"GRN ID: {grnId}", "navigation");
            StatusMessage = $"Error loading GRN for editing: {ex.Message}";
            
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error loading GRN {grnId}: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }
    
    private void ShowAddStockTransfer()
    {
        CurrentPageTitle = "Add Stock Transfer";
        StatusMessage = "Loading add stock transfer form...";
        
        try
        {
            // Create the AddStockTransferView
            var addStockTransferView = new AddStockTransferView();
            
            // Get services from DI container
            var stockTransferService = _serviceProvider.GetRequiredService<IStockTransferService>();
            var stockTransferItemService = _serviceProvider.GetRequiredService<IStockTransferItemService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var stockService = _serviceProvider.GetRequiredService<IStockService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback
            var addStockTransferViewModel = new AddStockTransferViewModel(
                stockTransferService,
                stockTransferItemService,
                storeService,
                productService,
                uomService,
                productBatchService,
                stockService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                navigateBack: () => _ = ShowStockManagement("StockTransfer") // Navigate back to stock management Stock Transfer section
            );
            
            addStockTransferView.DataContext = addStockTransferViewModel;
            CurrentView = addStockTransferView;
            StatusMessage = "Add stock transfer form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading add stock transfer form: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private void ShowEditStockTransfer(int transferId)
    {
        CurrentPageTitle = "Edit Stock Transfer";
        StatusMessage = "Loading edit stock transfer form...";
        
        try
        {
            // Create the AddStockTransferView (same view, different mode)
            var addStockTransferView = new AddStockTransferView();
            
            // Get services from DI container
            var stockTransferService = _serviceProvider.GetRequiredService<IStockTransferService>();
            var stockTransferItemService = _serviceProvider.GetRequiredService<IStockTransferItemService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var stockService = _serviceProvider.GetRequiredService<IStockService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback and transfer ID for editing
            var addStockTransferViewModel = new AddStockTransferViewModel(
                stockTransferService,
                stockTransferItemService,
                storeService,
                productService,
                uomService,
                productBatchService,
                stockService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                navigateBack: () => _ = ShowStockManagement("StockTransfer"), // Navigate back to stock management Stock Transfer section
                transferId: transferId // Pass the transfer ID for editing
            );
            
            addStockTransferView.DataContext = addStockTransferViewModel;
            CurrentView = addStockTransferView;
            StatusMessage = "Edit stock transfer form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading edit stock transfer form: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private void ShowAddGoodsReturn()
    {
        CurrentPageTitle = "Add Goods Return";
        StatusMessage = "Loading add goods return form...";
        
        try
        {
            // Create the AddGoodsReturnView
            var addGoodsReturnView = new AddGoodsReturnView();
            
            // Get services from DI container
            var goodsReturnService = _serviceProvider.GetRequiredService<IGoodsReturnService>();
            var goodsReturnItemService = _serviceProvider.GetRequiredService<IGoodsReturnItemService>();
            var goodsReceivedService = _serviceProvider.GetRequiredService<IGoodsReceivedService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback
            var addGoodsReturnViewModel = new AddGoodsReturnViewModel(
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                goodsReturnService,
                goodsReturnItemService,
                goodsReceivedService,
                storeService,
                supplierService,
                productService,
                uomService,
                productBatchService,
                navigateBack: () => _ = ShowStockManagement("GoodsReturn") // Navigate back to stock management Goods Return section
            );
            
            addGoodsReturnView.DataContext = addGoodsReturnViewModel;
            CurrentView = addGoodsReturnView;
            StatusMessage = "Add goods return form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load add goods return form";
            new MessageDialog("Error", $"Error loading add goods return form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private void ShowEditGoodsReturn(int returnId)
    {
        CurrentPageTitle = "Edit Goods Return";
        StatusMessage = "Loading goods return for editing...";
        
        try
        {
            // Create the AddGoodsReturnView (same view used for both add and edit)
            var addGoodsReturnView = new AddGoodsReturnView();
            
            // Get services from DI container
            var goodsReturnService = _serviceProvider.GetRequiredService<IGoodsReturnService>();
            var goodsReturnItemService = _serviceProvider.GetRequiredService<IGoodsReturnItemService>();
            var goodsReceivedService = _serviceProvider.GetRequiredService<IGoodsReceivedService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback and return ID for editing
            var addGoodsReturnViewModel = new AddGoodsReturnViewModel(
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                goodsReturnService,
                goodsReturnItemService,
                goodsReceivedService,
                storeService,
                supplierService,
                productService,
                uomService,
                productBatchService,
                navigateBack: () => _ = ShowStockManagement("GoodsReturn"), // Navigate back to stock management Goods Return section
                returnId: returnId // This puts it in edit mode
            );
            
            addGoodsReturnView.DataContext = addGoodsReturnViewModel;
            CurrentView = addGoodsReturnView;
            StatusMessage = "Goods return loaded for editing successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load goods return for editing";
            new MessageDialog("Error", $"Error loading goods return for editing: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private void ShowAddGoodsReplace()
    {
        CurrentPageTitle = "Add Goods Replace";
        StatusMessage = "Loading add goods replace form...";
        
        try
        {
            // Create the AddGoodsReplaceView
            var addGoodsReplaceView = new AddGoodsReplaceView();
            
            // Get services from DI container
            var goodsReplaceService = _serviceProvider.GetRequiredService<IGoodsReplaceService>();
            var goodsReplaceItemService = _serviceProvider.GetRequiredService<IGoodsReplaceItemService>();
            var goodsReturnService = _serviceProvider.GetRequiredService<IGoodsReturnService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var stockService = _serviceProvider.GetRequiredService<IStockService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback
            var addGoodsReplaceViewModel = new AddGoodsReplaceViewModel(
                goodsReplaceService,
                goodsReplaceItemService,
                goodsReturnService,
                storeService,
                productService,
                uomService,
                productBatchService,
                stockService,
                _activeCurrencyService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                navigateBack: () => _ = ShowStockManagement("GoodsReplace") // Navigate back to stock management Goods Replace section
            );
            
            addGoodsReplaceView.DataContext = addGoodsReplaceViewModel;
            CurrentView = addGoodsReplaceView;
            StatusMessage = "Add goods replace form loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load add goods replace form";
            new MessageDialog("Error", $"Error loading add goods replace form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private void ShowEditGoodsReplace(int replaceId)
    {
        CurrentPageTitle = "Edit Goods Replace";
        StatusMessage = "Loading goods replace for editing...";
        
        try
        {
            // Create the AddGoodsReplaceView (same view used for both add and edit)
            var addGoodsReplaceView = new AddGoodsReplaceView();
            
            // Get services from DI container
            var goodsReplaceService = _serviceProvider.GetRequiredService<IGoodsReplaceService>();
            var goodsReplaceItemService = _serviceProvider.GetRequiredService<IGoodsReplaceItemService>();
            var goodsReturnService = _serviceProvider.GetRequiredService<IGoodsReturnService>();
            var storeService = _serviceProvider.GetRequiredService<IStoreService>();
            var productService = _serviceProvider.GetRequiredService<IProductService>();
            var uomService = _serviceProvider.GetRequiredService<IUomService>();
            var productBatchService = _serviceProvider.GetRequiredService<IProductBatchService>();
            var stockService = _serviceProvider.GetRequiredService<IStockService>();
            var themeService = _serviceProvider.GetRequiredService<IThemeService>();
            var zoomService = _serviceProvider.GetRequiredService<IZoomService>();
            var localizationService = _serviceProvider.GetRequiredService<ILocalizationService>();
            var colorSchemeService = _serviceProvider.GetRequiredService<IColorSchemeService>();
            var layoutDirectionService = _serviceProvider.GetRequiredService<ILayoutDirectionService>();
            var fontService = _serviceProvider.GetRequiredService<IFontService>();
            var databaseLocalizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            
            // Create ViewModel with navigation callback and replace ID for editing
            var addGoodsReplaceViewModel = new AddGoodsReplaceViewModel(
                goodsReplaceService,
                goodsReplaceItemService,
                goodsReturnService,
                storeService,
                productService,
                uomService,
                productBatchService,
                stockService,
                _activeCurrencyService,
                themeService,
                zoomService,
                localizationService,
                colorSchemeService,
                layoutDirectionService,
                fontService,
                databaseLocalizationService,
                navigateBack: () => _ = ShowStockManagement("GoodsReplace"), // Navigate back to stock management Goods Replace section
                replaceId: replaceId // This puts it in edit mode
            );
            
            addGoodsReplaceView.DataContext = addGoodsReplaceViewModel;
            CurrentView = addGoodsReplaceView;
            StatusMessage = "Goods replace loaded for editing successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = "Failed to load goods replace for editing";
            new MessageDialog("Error", $"Error loading goods replace for editing: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }
    
    /// <summary>
    /// Helper method to convert FontSize enum to numeric value
    /// </summary>
    private static double GetFontSizeValue(FontSize fontSize)
    {
        return fontSize switch
        {
            FontSize.VerySmall => 10,
            FontSize.Small => 12,
            FontSize.Medium => 14,
            FontSize.Large => 16,
            _ => 14
        };
    }

    [RelayCommand]
    private async Task ShowAddOptions()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.ADD_OPTIONS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Others.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Settings" so sidebar stays highlighted
        CurrentPageTitle = "Others";
        StatusMessage = "Loading others...";
        
        try
        {
            // Create the AddOptionsViewModel with all required services
            var addOptionsViewModel = new AddOptionsViewModel(
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>(),
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ICustomerGroupService>(),
                _serviceProvider.GetRequiredService<ISupplierService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<ICategoryService>(),
                _serviceProvider.GetRequiredService<IProductAttributeService>(),
                _serviceProvider.GetRequiredService<IProductCombinationItemService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<IProductGroupService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _serviceProvider.GetRequiredService<IBrandService>(),
                _serviceProvider.GetRequiredService<IUomService>(),
                _serviceProvider.GetRequiredService<IStoreService>(),
                _serviceProvider.GetRequiredService<ICurrencyService>(),
                _serviceProvider.GetRequiredService<IProductModifierService>()
            );

            // Set up navigation from Others to specific modules
            addOptionsViewModel.NavigateToModuleAction = (moduleType) =>
            {
                switch (moduleType)
                {
                    case "Brand":
                        _ = ShowBrand();
                        break;
                    case "Category":
                        _ = ShowCategory();
                        break;
                    case "Discounts":
                        _ = ShowDiscounts();
                        break;
                    case "UOM":
                        _ = ShowUom();
                        break;
                    case "ProductAttributes":
                        ShowProductAttributes();
                        break;
                    case "ProductModifiers":
                        ShowProductModifiers();
                        break;
                    case "ProductCombinations":
                        _ = ShowProductCombinations();
                        break;
                    case "ProductGrouping":
                    case "ProductGroups":
                        _ = ShowProductGroups();
                        break;
                    case "PriceTypes":
                        _ = ShowPriceTypes();
                        break;
                    case "PaymentTypes":
                        _ = ShowPaymentTypes();
                        break;
                    case "TaxRates":
                        _ = ShowTaxTypes();
                        break;
                    case "Shop":
                        _ = ShowStore();
                        break;
                    case "Currency":
                        _ = ShowCurrency();
                        break;
                    default:
                        StatusMessage = $"Navigation to {moduleType} module not implemented yet";
                        break;
                }
            };

            // Set up back navigation to return to Settings
            addOptionsViewModel.GoBackAction = () =>
            {
                ShowSettingsCommand.Execute(null);
            };

            // Create the AddOptionsView and set its DataContext
            var addOptionsView = new AddOptionsView
            {
                DataContext = addOptionsViewModel
            };

            CurrentView = addOptionsView;
            StatusMessage = "Others loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading others: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    [RelayCommand]
    private async Task ShowDiscounts()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.DISCOUNTS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Discount Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Discount Management";
        StatusMessage = "Loading discount management...";
        
        try
        {
            // Create the DiscountViewModel with all required services
            var discountViewModel = new DiscountViewModel(
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<IStoreService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _activeCurrencyService,
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<ChronoPos.Infrastructure.Services.IDatabaseLocalizationService>(),
                navigateToAddDiscount: () => { /* TODO: Implement add discount navigation */ },
                navigateToEditDiscount: (discount) => { /* TODO: Implement edit discount navigation */ },
                navigateBack: () =>
                {
                    ShowAddOptionsCommand.Execute(null);
                }
            );

            // Create the DiscountView and set its DataContext
            var discountView = new DiscountView
            {
                DataContext = discountViewModel
            };

            CurrentView = discountView;
            StatusMessage = "Discount management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading discount management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowDiscountsWithAddPanel()
    {
        await ShowDiscounts();
        
        // Wait for view to be created
        await Task.Delay(100);
        
        if (CurrentView is DiscountView discountView && 
            discountView.DataContext is DiscountViewModel discountViewModel)
        {
            if (discountViewModel.AddDiscountCommand.CanExecute(null))
            {
                discountViewModel.AddDiscountCommand.Execute(null);
            }
        }
    }

    [RelayCommand]
    private async Task ShowUom()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.UOM))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access UOM Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Unit of Measurement Management";
        StatusMessage = "Loading UOM management...";
        
        try
        {
            // Create the UomViewModel with all required services
            var uomViewModel = new UomViewModel(
                _serviceProvider.GetRequiredService<IUomService>(),
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<ChronoPos.Infrastructure.Services.IDatabaseLocalizationService>(),
                _currentUserService,
                navigateToAddUom: () => { /* TODO: Implement add UOM navigation */ },
                navigateToEditUom: (uom) => { /* TODO: Implement edit UOM navigation */ },
                navigateBack: () =>
                {
                    ShowAddOptionsCommand.Execute(null);
                }
            );

            // Create the UomView and set its DataContext
            var uomView = new UomView
            {
                DataContext = uomViewModel
            };

            CurrentView = uomView;
            StatusMessage = "UOM management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading UOM management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowUomWithAddPanel()
    {
        await ShowUom();
        
        // Wait for view to be created
        await Task.Delay(100);
        
        if (CurrentView is UomView uomView && 
            uomView.DataContext is UomViewModel uomViewModel)
        {
            if (uomViewModel.AddUomCommand.CanExecute(null))
            {
                uomViewModel.AddUomCommand.Execute(null);
            }
        }
    }

    [RelayCommand]
    private async Task ShowPriceTypes()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PRICE_TYPES))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Price Types Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Price Types Management";
        StatusMessage = "Loading price types management...";
        
        try
        {
            // Create the PriceTypesViewModel with all required services
            var priceTypesViewModel = new PriceTypesViewModel(
                _serviceProvider.GetRequiredService<ISellingPriceTypeService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<ChronoPos.Infrastructure.Services.IDatabaseLocalizationService>()
            );

            // Set up back navigation to return to Others
            priceTypesViewModel.GoBackAction = () =>
            {
                ShowAddOptionsCommand.Execute(null);
            };

            // Create the PriceTypesView and set its DataContext
            var priceTypesView = new PriceTypesView
            {
                DataContext = priceTypesViewModel
            };

            CurrentView = priceTypesView;
            StatusMessage = "Price types management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading price types management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    [RelayCommand]
    private async Task ShowBrand()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.BRAND))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Brand Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Brand Management";
        StatusMessage = "Loading brand management...";
        
        try
        {
            AppLogger.Log("ShowBrand: Starting to create BrandViewModel...");
            
            // Create the BrandViewModel with all required services and navigation callback
            var brandViewModel = new BrandViewModel(
                _serviceProvider.GetRequiredService<IBrandService>(),
                _currentUserService,
                _databaseLocalizationService,
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            AppLogger.Log("ShowBrand: BrandViewModel created successfully");
            AppLogger.Log("ShowBrand: About to create BrandView...");
            
            // Create the BrandView and set its DataContext
            var brandView = new BrandView
            {
                DataContext = brandViewModel
            };

            AppLogger.Log("ShowBrand: BrandView created successfully");
            
            CurrentView = brandView;
            StatusMessage = "Brand management loaded successfully";
            AppLogger.Log("ShowBrand: CurrentView set successfully");
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            AppLogger.Log($"ShowBrand: ERROR - {ex.Message}");
            AppLogger.Log($"ShowBrand: Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                AppLogger.Log($"ShowBrand: Inner exception: {ex.InnerException.Message}");
            }
            
            StatusMessage = $"Error loading brand management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    /// <summary>
    /// Opens Brand Management with specific brand selected for editing
    /// </summary>
    private async Task ShowEditBrand(BrandDto brandDto)
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.BRAND))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Brand Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        CurrentPageTitle = "Brand Management";
        StatusMessage = $"Loading brand: {brandDto.Name}...";
        
        try
        {
            // Create the BrandViewModel with all required services and navigation callback
            var brandViewModel = new BrandViewModel(
                _serviceProvider.GetRequiredService<IBrandService>(),
                _currentUserService,
                _databaseLocalizationService,
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            // Create the BrandView and set its DataContext
            var brandView = new BrandView
            {
                DataContext = brandViewModel
            };

            CurrentView = brandView;
            
            // Wait for the view to load
            await Task.Delay(100);
            
            // Trigger the edit panel with the brand
            brandViewModel.EditBrandCommand.Execute(brandDto);
            
            StatusMessage = $"Editing brand: {brandDto.Name}";
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading brand for edit", ex);
            StatusMessage = $"Error loading brand: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowCurrency()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.CURRENCY))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Currency Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Currency Management";
        StatusMessage = "Loading currency management...";
        
        try
        {
            // Create the CurrencyViewModel with all required services and navigation callback
            var currencyViewModel = new CurrencyViewModel(
                _serviceProvider.GetRequiredService<ICurrencyService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<IActiveCurrencyService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<InfrastructureServices.IDatabaseLocalizationService>(),
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            // Create the CurrencyView and set its DataContext
            var currencyView = new CurrencyView
            {
                DataContext = currencyViewModel
            };

            CurrentView = currencyView;
            StatusMessage = "Currency management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading currency management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowCurrencyWithAddPanel()
    {
        // First open the Currency screen
        await ShowCurrency();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the CurrencyViewModel from the CurrentView
        if (CurrentView is CurrencyView currencyView && 
            currencyView.DataContext is CurrencyViewModel currencyViewModel)
        {
            // Trigger the AddCurrencyCommand
            if (currencyViewModel.AddCurrencyCommand.CanExecute(null))
            {
                currencyViewModel.AddCurrencyCommand.Execute(null);
            }
        }
    }

    private async Task ShowPriceTypesWithAddPanel()
    {
        // First open the Price Types screen
        await ShowPriceTypes();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the PriceTypesViewModel from the CurrentView
        if (CurrentView is PriceTypesView priceTypesView && 
            priceTypesView.DataContext is PriceTypesViewModel priceTypesViewModel)
        {
            // Trigger the AddPriceTypeCommand
            if (priceTypesViewModel.AddPriceTypeCommand.CanExecute(null))
            {
                priceTypesViewModel.AddPriceTypeCommand.Execute(null);
            }
        }
    }

    private async Task ShowPaymentTypes()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PAYMENT_TYPES))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Payment Types Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Payment Types Management";
        StatusMessage = "Loading payment types management...";
        
        try
        {
            // Create the PaymentTypesViewModel with all required services
            var paymentTypesViewModel = new PaymentTypesViewModel(
                _serviceProvider.GetRequiredService<IPaymentTypeService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<ChronoPos.Infrastructure.Services.IDatabaseLocalizationService>()
            );

            // Set up back navigation to return to Others
            paymentTypesViewModel.GoBackAction = () =>
            {
                ShowAddOptionsCommand.Execute(null);
            };

            // Create the PaymentTypesView and set its DataContext
            var paymentTypesView = new PaymentTypesView
            {
                DataContext = paymentTypesViewModel
            };

            CurrentView = paymentTypesView;
            StatusMessage = "Payment types management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading brand management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowPaymentTypesWithAddPanel()
    {
        // First open the Payment Types screen
        await ShowPaymentTypes();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the PaymentTypesViewModel from the CurrentView
        if (CurrentView is PaymentTypesView paymentView && 
            paymentView.DataContext is PaymentTypesViewModel paymentViewModel)
        {
            // Trigger the AddNewCommand
            if (paymentViewModel.AddNewCommand.CanExecute(null))
            {
                paymentViewModel.AddNewCommand.Execute(null);
            }
        }
    }

    private async Task ShowCategory()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.CATEGORY))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Category Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Category Management";
        StatusMessage = "Loading category management...";
        
        try
        {
            // Create the CategoryViewModel with all required services and navigation callback
            var categoryViewModel = new CategoryViewModel(
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider,
                _serviceProvider.GetRequiredService<ILogger<CategoryViewModel>>(),
                _currentUserService,
                _databaseLocalizationService,
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            // Create the CategoryView and set its DataContext
            var categoryView = new CategoryView
            {
                DataContext = categoryViewModel
            };

            CurrentView = categoryView;
            StatusMessage = "Category management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading category management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    /// <summary>
    /// Opens Category Management with specific category selected for editing
    /// </summary>
    private async Task ShowEditCategory(CategoryDto categoryDto)
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.CATEGORY))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Category Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        CurrentPageTitle = "Category Management";
        StatusMessage = $"Loading category: {categoryDto.Name}...";
        
        try
        {
            // Create the CategoryViewModel with all required services and navigation callback
            var categoryViewModel = new CategoryViewModel(
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider,
                _serviceProvider.GetRequiredService<ILogger<CategoryViewModel>>(),
                _currentUserService,
                _databaseLocalizationService,
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            // Create the CategoryView and set its DataContext
            var categoryView = new CategoryView
            {
                DataContext = categoryViewModel
            };

            CurrentView = categoryView;
            
            // Wait for the view to load
            await Task.Delay(100);
            
            // Create a CategoryHierarchyItem wrapper and trigger the edit panel
            var categoryHierarchyItem = new CategoryHierarchyItem
            {
                Category = categoryDto
            };
            categoryViewModel.EditCategoryCommand.Execute(categoryHierarchyItem);
            
            StatusMessage = $"Editing category: {categoryDto.Name}";
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading category for edit", ex);
            StatusMessage = $"Error loading category: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowStore()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SHOP))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Store Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Store Management";
        StatusMessage = "Loading store management...";
        
        try
        {
            // Create the StoreViewModel with all required services and navigation callback
            var storeViewModel = new StoreViewModel(
                _serviceProvider.GetRequiredService<IStoreService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<InfrastructureServices.IDatabaseLocalizationService>(),
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            // Create the StoreView and set its DataContext
            var storeView = new StoreView
            {
                DataContext = storeViewModel
            };

            CurrentView = storeView;
            StatusMessage = "Store management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading store management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowStoreWithAddPanel()
    {
        await ShowStore();
        
        // Wait for view to be created
        await Task.Delay(100);
        
        if (CurrentView is StoreView storeView && 
            storeView.DataContext is StoreViewModel storeViewModel)
        {
            if (storeViewModel.AddStoreCommand.CanExecute(null))
            {
                storeViewModel.AddStoreCommand.Execute(null);
            }
        }
    }

    [RelayCommand]
    private async Task ShowTaxTypes()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.TAX_RATES))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Tax Rates Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Tax Rates Management";
        StatusMessage = "Loading tax rates management...";
        
        try
        {
            // Create the TaxTypesViewModel with all required services
            var taxTypesViewModel = new TaxTypesViewModel(
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<IThemeService>(),
                _serviceProvider.GetRequiredService<IZoomService>(),
                _serviceProvider.GetRequiredService<ILocalizationService>(),
                _serviceProvider.GetRequiredService<IColorSchemeService>(),
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _serviceProvider.GetRequiredService<IFontService>(),
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>()
            );

            // Set up back navigation to return to Others
            taxTypesViewModel.GoBackAction = () =>
            {
                ShowAddOptionsCommand.Execute(null);
            };

            // Create the TaxTypesView and set its DataContext
            var taxTypesView = new TaxTypesView
            {
                DataContext = taxTypesViewModel
            };

            CurrentView = taxTypesView;
            StatusMessage = "Tax rates management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading tax rates management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowTaxTypesWithAddPanel()
    {
        // First open the Tax Types screen
        await ShowTaxTypes();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the TaxTypesViewModel from the CurrentView
        if (CurrentView is TaxTypesView taxView && 
            taxView.DataContext is TaxTypesViewModel taxViewModel)
        {
            // Trigger the AddNewCommand
            if (taxViewModel.AddNewCommand.CanExecute(null))
            {
                taxViewModel.AddNewCommand.Execute(null);
            }
        }
    }

    private async Task ShowCustomers()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.CUSTOMERS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Customer Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }
        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Customer Management";
        StatusMessage = "Loading customer management...";
        
        try
        {
            // Create the CustomersViewModel with all required services
            var customersViewModel = new CustomersViewModel(
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ICustomerGroupService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<IDiscountService>()
            );

            // Set up back navigation to return to Customer Management
            customersViewModel.GoBackAction = () =>
            {
                _ = ShowCustomerManagement();
            };

            // Set up customer transactions navigation
            customersViewModel.ShowCustomerTransactionsAction = ShowCustomerTransactions;

            // Create the CustomersView and set its DataContext
            var customersView = new CustomersView
            {
                DataContext = customersViewModel
            };

            CurrentView = customersView;
            StatusMessage = "Customer management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading customer management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowCustomersWithAddPanel()
    {
        // First open the Customers screen
        await ShowCustomers();
        
        // Then trigger the add customer sidepanel
        // Wait a moment for the view to be rendered
        await Task.Delay(100);
        
        // Try to get the CustomersViewModel from the CurrentView
        if (CurrentView is CustomersView customersView && 
            customersView.DataContext is CustomersViewModel customersViewModel)
        {
            // Trigger the ShowAddCustomerSidePanelCommand
            if (customersViewModel.ShowAddCustomerSidePanelCommand.CanExecute(null))
            {
                customersViewModel.ShowAddCustomerSidePanelCommand.Execute(null);
            }
        }
    }

    public void ShowCustomerTransactions(int customerId, string customerName)
    {
        try
        {
            CurrentPageTitle = "Customer Transactions";
            StatusMessage = "Loading customer transactions...";

            // Create the CustomerTransactionsViewModel
            var transactionRepository = _serviceProvider.GetRequiredService<ITransactionRepository>();
            var transactionsViewModel = new CustomerTransactionsViewModel(
                transactionRepository,
                customerId,
                customerName,
                () => _ = ShowCustomers()
            );

            // Create the view and set its DataContext
            var transactionsView = new CustomerTransactionsView
            {
                DataContext = transactionsViewModel
            };

            CurrentView = transactionsView;
            StatusMessage = "Customer transactions loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading customer transactions: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    /// <summary>
    /// Navigate to a specific view
    /// </summary>
    /// <param name="view">The view to navigate to</param>
    public void NavigateToView(object view)
    {
        CurrentView = view;
    }

    private async Task ShowCustomerGroups()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.CUSTOMER_GROUPS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Customer Groups Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Customer Groups Management";
        StatusMessage = "Loading customer groups...";
        
        try
        {
            // Create the CustomerGroupsViewModel with all required services
            var customerGroupsViewModel = new CustomerGroupsViewModel(
                _serviceProvider.GetRequiredService<ICustomerGroupService>(),
                _serviceProvider.GetRequiredService<ICustomerGroupRelationService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ISellingPriceTypeService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _currentUserService
            );

            // Set up back navigation to return to Customer Management
            customerGroupsViewModel.GoBackAction = () =>
            {
                _ = ShowCustomerManagement();
            };

            // Create the CustomerGroupsView and set its DataContext
            var customerGroupsView = new CustomerGroupsView
            {
                DataContext = customerGroupsViewModel
            };

            CurrentView = customerGroupsView;
            StatusMessage = "Customer groups loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading customer groups: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowCustomerGroupsWithAddPanel()
    {
        await ShowCustomerGroups();
        
        // Wait for view to be created
        await Task.Delay(100);
        
        if (CurrentView is CustomerGroupsView customerGroupsView && 
            customerGroupsView.DataContext is CustomerGroupsViewModel customerGroupsViewModel)
        {
            if (customerGroupsViewModel.AddCustomerGroupCommand.CanExecute(null))
            {
                customerGroupsViewModel.AddCustomerGroupCommand.Execute(null);
            }
        }
    }

    private async Task ShowProductGroups()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PRODUCT_GROUPING))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Product Groups Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Product Groups Management";
        StatusMessage = "Loading product groups...";
        
        try
        {
            // Create the ProductGroupsViewModel with all required services
            var productGroupsViewModel = new ProductGroupsViewModel(
                _serviceProvider.GetRequiredService<IProductGroupService>(),
                _serviceProvider.GetRequiredService<IProductGroupItemService>(),
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<ITaxTypeService>(),
                _serviceProvider.GetRequiredService<ISellingPriceTypeService>(),
                _serviceProvider.GetRequiredService<IProductService>(),
                _serviceProvider.GetRequiredService<IProductUnitService>(),
                _currentUserService,
                _serviceProvider.GetRequiredService<ILayoutDirectionService>(),
                _databaseLocalizationService
            );

            // Set up back navigation to return to Others
            productGroupsViewModel.GoBackAction = () =>
            {
                ShowAddOptionsCommand.Execute(null);
            };

            // Create the ProductGroupsView and set its DataContext
            var productGroupsView = new ProductGroupsView
            {
                DataContext = productGroupsViewModel
            };

            CurrentView = productGroupsView;
            StatusMessage = "Product groups loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading product groups: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowProductGroupsWithAddPanel()
    {
        // First open the Product Groups screen
        await ShowProductGroups();
        
        // Wait for the view to render
        await Task.Delay(100);
        
        // Try to get the ProductGroupsViewModel from the CurrentView
        if (CurrentView is ProductGroupsView groupsView && 
            groupsView.DataContext is ProductGroupsViewModel groupsViewModel)
        {
            // Trigger the AddProductGroupCommand
            if (groupsViewModel.AddProductGroupCommand.CanExecute(null))
            {
                groupsViewModel.AddProductGroupCommand.Execute(null);
            }
        }
    }

    private async Task ShowSuppliers()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SUPPLIERS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Supplier Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Supplier Management";
        StatusMessage = "Loading supplier management...";
        
        try
        {
            // Create the SuppliersViewModel with all required services
            var suppliersViewModel = new SuppliersViewModel(
                _serviceProvider.GetRequiredService<ISupplierService>(),
                _currentUserService
            );

            // Set up back navigation to return to Supplier Management
            suppliersViewModel.GoBackAction = () =>
            {
                _ = ShowSupplierManagement();
            };

            // Create the SuppliersView and set its DataContext
            var suppliersView = new SuppliersView
            {
                DataContext = suppliersViewModel
            };

            CurrentView = suppliersView;
            StatusMessage = "Supplier management loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading supplier management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            };
            CurrentView = errorContent;
        }
    }

    private async Task ShowSuppliersWithAddPanel()
    {
        await ShowSuppliers();
        
        // Wait for view to be created
        await Task.Delay(100);
        
        if (CurrentView is SuppliersView suppliersView && 
            suppliersView.DataContext is SuppliersViewModel suppliersViewModel)
        {
            if (suppliersViewModel.AddSupplierCommand.CanExecute(null))
            {
                suppliersViewModel.AddSupplierCommand.Execute(null);
            }
        }
    }

    [RelayCommand]
    private async Task ShowReservation()
    {
        AppLogger.LogInfo("===== ShowReservation command invoked =====", filename: "reservation");
        
        // Check if already on Reservation page to avoid unnecessary recreation
        if (SelectedPage == "Reservation" && CurrentView is ReservationView)
        {
            AppLogger.LogInfo("Already on Reservation page, skipping recreation", filename: "reservation");
            return;
        }
        
        // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
        AppLogger.LogInfo("Checking reservation screen permissions...", filename: "reservation");
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.RESERVATION))
        {
            AppLogger.LogWarning("User does not have permission to access Reservation screen", filename: "reservation");
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Reservation screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }
        AppLogger.LogInfo("Permission check passed", filename: "reservation");

        SelectedPage = "Reservation";
        AppLogger.LogInfo($"SelectedPage set to: {SelectedPage}", filename: "reservation");
        
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_reservation") ?? "Reservation";
        AppLogger.LogInfo($"CurrentPageTitle set to: {CurrentPageTitle}", filename: "reservation");
        
        StatusMessage = "Loading reservation management...";
        AppLogger.LogInfo("StatusMessage set to: Loading reservation management...", filename: "reservation");
        
        try
        {
            AppLogger.LogInfo("Creating ReservationTimelineViewModel...", filename: "reservation");
            
            // Create the ReservationTimelineViewModel with all required services
            var reservationTimelineViewModel = new ReservationTimelineViewModel(
                _serviceProvider.GetRequiredService<IReservationService>(),
                _serviceProvider.GetRequiredService<IRestaurantTableService>(),
                _serviceProvider.GetRequiredService<ICustomerService>(),
                _serviceProvider.GetRequiredService<ICurrentUserService>(),
                _serviceProvider.GetRequiredService<IPaymentTypeService>()
            );
            AppLogger.LogInfo("ReservationTimelineViewModel created successfully", filename: "reservation");

            // Create the ReservationView and set DataContext (following same pattern as ManagementView)
            AppLogger.LogInfo("Creating ReservationView...", filename: "reservation");
            var reservationView = new ReservationView
            {
                DataContext = reservationTimelineViewModel
            };
            AppLogger.LogInfo($"ReservationView created successfully. View type: {reservationView.GetType().FullName}", filename: "reservation");
            AppLogger.LogInfo($"DataContext set to ViewModel type: {reservationView.DataContext?.GetType().FullName ?? "null"}", filename: "reservation");

            AppLogger.LogInfo($"Setting CurrentView to ReservationView. Previous CurrentView: {CurrentView?.GetType().FullName ?? "null"}", filename: "reservation");
            CurrentView = reservationView;
            AppLogger.LogInfo($"CurrentView updated. New CurrentView type: {CurrentView?.GetType().FullName ?? "null"}", filename: "reservation");
            
            StatusMessage = "Reservation management loaded successfully";
            AppLogger.LogInfo("===== ShowReservation completed successfully =====", filename: "reservation");
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            AppLogger.LogError("ShowReservation failed", ex, filename: "reservation");
            StatusMessage = $"Error loading reservation management: {ex.Message}";
            var errorContent = new System.Windows.Controls.TextBlock
            {
                Text = $"Error: {ex.Message}\n\nPlease ensure all required services are registered.",
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.Red,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                Margin = new System.Windows.Thickness(20)
            };
            CurrentView = errorContent;
            AppLogger.LogInfo("Error view displayed to user", filename: "reservation");
        }
    }

    // COMMENTED OUT: OrderTable removed from sidebar - screen constant removed from ScreenNames
    // [RelayCommand]
    // private void ShowOrderTable()
    // {
    //     // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
    //     // Note: ORDER_TABLE constant has been removed from ScreenNames.cs
    //     // if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.ORDER_TABLE))
    //     // {
    //     //     MessageBox.Show(
    //     //         "You don't have permission to access the Order Table screen.",
    //     //         "Access Denied",
    //     //         MessageBoxButton.OK,
    //     //         MessageBoxImage.Warning);
    //     //     return;
    //     // }
    //
    //     SelectedPage = "OrderTable";
    //     CurrentPageTitle = "Order Table";
    //     StatusMessage = "Order table loaded";
    //     
    //     var orderTableContent = new System.Windows.Controls.TextBlock
    //     {
    //         Text = "Order Table\n(Order history and management will be shown here)",
    //         HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
    //         VerticalAlignment = System.Windows.VerticalAlignment.Center,
    //         FontSize = 16
    //     };
    //     CurrentView = orderTableContent;
    // }

    [RelayCommand]
    private void ShowReports()
    {
        // Check permission using UMAC - Allow if user has ANY permission (Create, Edit, Delete, Import, Export, View, Print)
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.REPORTS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Reports screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        SelectedPage = "Reports";
        CurrentPageTitle = "Reports";
        StatusMessage = "Reports interface loaded";
        
        try
        {
            // Create the Reports Hub view with ViewModel
            var reportsHubViewModel = _serviceProvider.GetRequiredService<ReportsHubViewModel>();
            var reportsHubView = new ReportsHubView
            {
                DataContext = reportsHubViewModel
            };
            CurrentView = reportsHubView;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reports hub: {ex.Message}");
            AppLogger.LogError(
                $"Failed to load reports hub: {ex.Message}", 
                ex, 
                "MainWindowViewModel.ShowReports");
            
            StatusMessage = "Error loading reports";
            new MessageDialog(
                "Error",
                "Failed to load the reports. Please try again.",
                MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.SETTINGS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access the Settings screen.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        SelectedPage = "Settings";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_settings") ?? "Settings";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_loading_settings") ?? "Loading settings...";
        
        try
        {
            Console.WriteLine("ShowSettings: Starting to load settings");
            
            // Create SettingsViewModel
            Console.WriteLine("ShowSettings: Getting SettingsViewModel from service provider");
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            Console.WriteLine("ShowSettings: SettingsViewModel retrieved successfully");
            
            // Set up navigation from settings to specific modules
            settingsViewModel.NavigateToSettingsModuleAction = (moduleType) =>
            {
                AppLogger.Log($"MainWindowViewModel: NavigateToSettingsModuleAction invoked with moduleType={moduleType}");
                switch (moduleType)
                {
                    case "UserSettings":
                        AppLogger.Log("MainWindowViewModel: Navigating to UserSettings");
                        _ = ShowUserSettings();
                        break;
                    case "ApplicationSettings":
                        AppLogger.Log("MainWindowViewModel: Navigating to ApplicationSettings");
                        _ = ShowApplicationSettings();
                        break;
                    case "AddOptions":
                        AppLogger.Log("MainWindowViewModel: Navigating to AddOptions (Others)");
                        _ = ShowAddOptions();
                        break;
                    case "Services":
                        AppLogger.Log("MainWindowViewModel: Navigating to Services");
                        _ = ShowServiceCharges();
                        break;
                    case "Permissions":
                        AppLogger.Log("MainWindowViewModel: Navigating to Permissions");
                        _ = ShowPermissions();
                        break;
                    case "Roles":
                        AppLogger.Log("MainWindowViewModel: Navigating to Roles");
                        _ = ShowRoles();
                        break;
                    default:
                        AppLogger.Log($"MainWindowViewModel: Unknown module type: {moduleType}");
                        StatusMessage = $"Navigation to {moduleType} not implemented yet";
                        break;
                }
            };
            
            // Create and configure the settings view
            Console.WriteLine("ShowSettings: Creating SettingsView");
            var settingsView = new SettingsView();
            Console.WriteLine("ShowSettings: SettingsView created successfully");
            
            Console.WriteLine("ShowSettings: Setting DataContext");
            settingsView.DataContext = settingsViewModel;
            Console.WriteLine("ShowSettings: DataContext set successfully");
            
            Console.WriteLine("ShowSettings: Setting CurrentView");
            CurrentView = settingsView;
            Console.WriteLine("ShowSettings: CurrentView set successfully");
            
            StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_settings_loaded") ?? "Settings loaded";
            Console.WriteLine("ShowSettings: Settings loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ShowSettings: Error occurred - {ex.Message}");
            Console.WriteLine($"ShowSettings: Stack trace - {ex.StackTrace}");
            StatusMessage = $"Error loading settings: {ex.Message}";
            new MessageDialog("Settings Error", $"Failed to load settings: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task ShowUserSettings()
    {
        AppLogger.Log("=== ShowUserSettings STARTED ===");
        
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.CLIENT_SETTINGS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access User Settings.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }
        
        // Don't change SelectedPage - keep it as "Settings" so sidebar stays highlighted
        CurrentPageTitle = "User Settings";
        StatusMessage = "Loading user settings...";
        
        try
        {
            AppLogger.Log("MainWindowViewModel: Step 1 - About to create UserSettingsViewModel");
            
            // Create the UserSettingsView with its ViewModel
            var userSettingsViewModel = new UserSettingsViewModel(_serviceProvider)
            {
                NavigateBackAction = async () => await ShowSettings()
            };
            
            AppLogger.Log("MainWindowViewModel: Step 2 - UserSettingsViewModel created successfully");
            AppLogger.Log("MainWindowViewModel: Step 3 - About to create UserSettingsView");
            
            var userSettingsView = new UserSettingsView
            {
                DataContext = userSettingsViewModel
            };
            
            AppLogger.Log("MainWindowViewModel: Step 4 - UserSettingsView created successfully");
            AppLogger.Log("MainWindowViewModel: Step 5 - Setting CurrentView");
            
            CurrentView = userSettingsView;
            
            AppLogger.Log("MainWindowViewModel: Step 6 - CurrentView set successfully");
            
            StatusMessage = "User settings loaded";
            
            AppLogger.Log("=== ShowUserSettings COMPLETED SUCCESSFULLY ===");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading user settings: {ex.Message}";
            AppLogger.Log($"!!! ShowUserSettings ERROR !!!: {ex.Message}");
            AppLogger.Log($"!!! ShowUserSettings STACK TRACE !!!: {ex.StackTrace}");
            AppLogger.Log($"!!! ShowUserSettings INNER EXCEPTION !!!: {ex.InnerException?.Message ?? "None"}");
            Console.WriteLine($"ShowUserSettings error: {ex.Message}");
            Console.WriteLine($"ShowUserSettings stack trace: {ex.StackTrace}");
            
            new MessageDialog(
                "User Settings Error",
                $"Error loading User Settings:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task ShowApplicationSettings()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.GLOBAL_SETTINGS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Application Settings.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }
        
        // Don't change SelectedPage - keep it as "Settings" so sidebar stays highlighted
        CurrentPageTitle = "Application Settings";
        StatusMessage = "Loading application settings...";
        
        try
        {
            Console.WriteLine("ShowApplicationSettings: Getting SettingsViewModel");
            // Get the current SettingsViewModel which already has all the application settings
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            
            // Set the navigation back action to return to Settings module view
            settingsViewModel.NavigateBackAction = () => _ = ShowSettings();
            
            Console.WriteLine("ShowApplicationSettings: Creating ApplicationSettingsView");
            // Create the application settings view with all settings controls
            var appSettingsView = new ApplicationSettingsView();
            appSettingsView.DataContext = settingsViewModel;
            
            Console.WriteLine("ShowApplicationSettings: Setting CurrentView");
            CurrentView = appSettingsView;
            StatusMessage = "Application settings loaded";
            Console.WriteLine("ShowApplicationSettings: Application settings loaded successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading application settings: {ex.Message}";
            Console.WriteLine($"ShowApplicationSettings error: {ex.Message}");
            Console.WriteLine($"ShowApplicationSettings stack trace: {ex.StackTrace}");
        }
    }

    private async Task ShowPermissions()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.PERMISSIONS))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Permissions Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }
        
        // Don't change SelectedPage - keep it as "Settings" so sidebar stays highlighted
        CurrentPageTitle = "Permissions";
        StatusMessage = "Loading permissions...";
        
        try
        {
            Console.WriteLine("ShowPermissions: Creating PermissionViewModel");
            // Create the PermissionViewModel with all required services and navigation callback
            var permissionViewModel = new PermissionViewModel(
                _serviceProvider.GetRequiredService<IPermissionService>(),
                navigateBack: () => _ = ShowSettings()
            );

            Console.WriteLine("ShowPermissions: Creating PermissionView");
            // Create the permissions view
            var permissionView = new PermissionView
            {
                DataContext = permissionViewModel
            };
            
            Console.WriteLine("ShowPermissions: Setting CurrentView");
            CurrentView = permissionView;
            StatusMessage = "Permissions loaded";
            Console.WriteLine("ShowPermissions: Permissions loaded successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading permissions: {ex.Message}";
            Console.WriteLine($"ShowPermissions error: {ex.Message}");
            Console.WriteLine($"ShowPermissions stack trace: {ex.StackTrace}");
        }
        
        await Task.CompletedTask;
    }

    private async Task ShowRoles()
    {
        // Check permission using UMAC - Allow if user has ANY permission
        if (!_currentUserService.HasAnyScreenPermission(ChronoPos.Application.Constants.ScreenNames.ROLES))
        {
            new MessageDialog(
                "Access Denied",
                "You don't have permission to access Roles Management.",
                MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }
        
        // Don't change SelectedPage - keep it as "Settings" so sidebar stays highlighted
        CurrentPageTitle = "Roles";
        StatusMessage = "Loading roles...";
        
        try
        {
            AppLogger.Log("ShowRoles: Starting method execution");
            Console.WriteLine("ShowRoles: Creating RoleViewModel");
            
            // Get the services
            AppLogger.Log("ShowRoles: Getting IRoleService from ServiceProvider");
            var roleService = _serviceProvider.GetRequiredService<IRoleService>();
            AppLogger.Log("ShowRoles: IRoleService obtained successfully");
            
            AppLogger.Log("ShowRoles: Getting IPermissionService from ServiceProvider");
            var permissionService = _serviceProvider.GetRequiredService<IPermissionService>();
            AppLogger.Log("ShowRoles: IPermissionService obtained successfully");
            
            // Create the RoleViewModel with all required services and navigation callback
            AppLogger.Log("ShowRoles: Creating RoleViewModel instance");
            var roleViewModel = new RoleViewModel(
                roleService,
                permissionService,
                navigateBack: () => _ = ShowSettings()
            );
            AppLogger.Log("ShowRoles: RoleViewModel created successfully");

            Console.WriteLine("ShowRoles: Creating RoleView");
            // Create the roles view
            AppLogger.Log("ShowRoles: Creating RoleView instance");
            var roleView = new RoleView
            {
                DataContext = roleViewModel
            };
            AppLogger.Log("ShowRoles: RoleView created successfully");
            
            Console.WriteLine("ShowRoles: Setting CurrentView");
            AppLogger.Log("ShowRoles: Setting CurrentView property");
            CurrentView = roleView;
            AppLogger.Log("ShowRoles: CurrentView set successfully");
            
            StatusMessage = "Roles loaded";
            Console.WriteLine("ShowRoles: Roles loaded successfully");
            AppLogger.Log("ShowRoles: Method completed successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading roles: {ex.Message}";
            Console.WriteLine($"ShowRoles error: {ex.Message}");
            Console.WriteLine($"ShowRoles stack trace: {ex.StackTrace}");
            AppLogger.LogError("ShowRoles", ex);
        }
        
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void Logout()
    {
        var dialog = new ConfirmationDialog(
            "Confirm Logout",
            "Are you sure you want to logout?\n\nThe application will restart to ensure a clean session.",
            ConfirmationDialog.DialogType.Warning);
        
        var result = dialog.ShowDialog();
        
        if (result == true)
        {
            StatusMessage = "Logging out...";
            
            try
            {
                // Clear the current user session
                _currentUserService.ClearCurrentUser();
                
                // Get the executable path to restart the application
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                
                if (!string.IsNullOrEmpty(exePath))
                {
                    // Start a new instance of the application
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true,
                        WorkingDirectory = Environment.CurrentDirectory
                    });
                }
                
                // Shutdown the current application instance
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                new MessageDialog(
                    "Logout Error",
                    $"An error occurred during logout: {ex.Message}\n\nPlease close and restart the application manually.",
                    MessageDialog.MessageType.Error).ShowDialog();
                
                StatusMessage = "Logout failed";
            }
        }
    }

    private System.Windows.Controls.Border CreateStatsCard(string title, string value, string colorHex)
    {
        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
        
        var card = new System.Windows.Controls.Border
        {
            Width = 200,
            Height = 100,
            Background = new System.Windows.Media.SolidColorBrush(color),
            CornerRadius = new System.Windows.CornerRadius(5),
            Margin = new System.Windows.Thickness(0, 0, 15, 15)
        };
        
        var stackPanel = new System.Windows.Controls.StackPanel
        {
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = value,
            FontSize = 24,
            FontWeight = System.Windows.FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        });
        
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = title,
            FontSize = 12,
            Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        });
        
        card.Child = stackPanel;
        return card;
    }

    #region Global Search Methods

    partial void OnGlobalSearchQueryChanged(string value)
    {
        HasGlobalSearchText = !string.IsNullOrWhiteSpace(value);
        
        // Reset and restart the search delay timer
        _searchDelayTimer?.Stop();
        
        if (HasGlobalSearchText)
        {
            _searchDelayTimer?.Start();
        }
        else
        {
            ClearGlobalSearchResults();
        }
    }

    private async void OnSearchDelayElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (HasGlobalSearchText)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => 
            {
                await PerformQuickGlobalSearchAsync();
            });
        }
    }

    private async Task PerformQuickGlobalSearchAsync()
    {
        try
        {
            // Limit quick search to 10 results for better UX
            var results = await _globalSearchService.GetQuickSearchAsync(GlobalSearchQuery, 10);
            
            GlobalSearchResults.Clear();
            foreach (var result in results)
            {
                GlobalSearchResults.Add(result);
            }

            HasMoreGlobalSearchResults = results.Count >= 10;
            ShowGlobalSearchResults = GlobalSearchResults.Count > 0;
            
            // Update header with result count and module breakdown
            if (GlobalSearchResults.Count > 0)
            {
                var moduleGroups = GlobalSearchResults.GroupBy(r => r.Module).Take(3);
                var moduleText = string.Join(", ", moduleGroups.Select(g => $"{g.Count()} {g.Key}"));
                GlobalSearchResultsHeader = $"Found {GlobalSearchResults.Count} result{(GlobalSearchResults.Count != 1 ? "s" : "")} ({moduleText})";
            }
            else
            {
                GlobalSearchResultsHeader = "No results found";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Quick search error: {ex.Message}");
            ClearGlobalSearchResults();
        }
    }

    public void ClearGlobalSearch()
    {
        GlobalSearchQuery = string.Empty;
        ClearGlobalSearchResults();
    }

    private void ClearGlobalSearchResults()
    {
        GlobalSearchResults.Clear();
        ShowGlobalSearchResults = false;
        HasMoreGlobalSearchResults = false;
        SelectedGlobalSearchResult = null;
    }

    private async void ShowAllGlobalSearchResults()
    {
        if (HasGlobalSearchText)
        {
            try
            {
                // Perform a comprehensive search with more results
                var filter = new GlobalSearchFilterDto
                {
                    Query = GlobalSearchQuery,
                    MaxResults = 50,
                    IncludeProducts = true,
                    IncludeCustomers = true,
                    IncludeSales = true,
                    IncludeStock = true,
                    IncludeBrands = true,
                    IncludeCategories = true,
                    IncludePages = true,
                    IncludeFeatures = true
                };
                
                var response = await _globalSearchService.SearchAsync(filter);
                
                // Close the popup
                ShowGlobalSearchResults = false;
                
                // Show results in a categorized view (can be enhanced with a dedicated view later)
                StatusMessage = $"Found {response.TotalResults} results for '{GlobalSearchQuery}' - " +
                               $"{string.Join(", ", response.ResultsByModule.Select(kvp => $"{kvp.Value} {kvp.Key}"))}";
                
                // For now, navigate to the most relevant module
                if (response.Results.Any())
                {
                    var firstResult = response.Results.First();
                    NavigateToSearchResult(firstResult);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error performing full search: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Full search error: {ex.Message}");
            }
        }
    }

    public void SelectNextGlobalSearchResult()
    {
        if (GlobalSearchResults.Count == 0)
            return;

        var currentIndex = SelectedGlobalSearchResult != null ? GlobalSearchResults.IndexOf(SelectedGlobalSearchResult) : -1;
        var nextIndex = (currentIndex + 1) % GlobalSearchResults.Count;
        SelectedGlobalSearchResult = GlobalSearchResults[nextIndex];
    }

    public void SelectPreviousGlobalSearchResult()
    {
        if (GlobalSearchResults.Count == 0)
            return;

        var currentIndex = SelectedGlobalSearchResult != null ? GlobalSearchResults.IndexOf(SelectedGlobalSearchResult) : 0;
        var previousIndex = currentIndex <= 0 ? GlobalSearchResults.Count - 1 : currentIndex - 1;
        SelectedGlobalSearchResult = GlobalSearchResults[previousIndex];
    }

    public void OpenSelectedGlobalSearchResult()
    {
        if (SelectedGlobalSearchResult != null)
        {
            OpenGlobalSearchResult(SelectedGlobalSearchResult);
        }
        else if (GlobalSearchResults.Count > 0)
        {
            OpenGlobalSearchResult(GlobalSearchResults[0]);
        }
    }

    public void OpenGlobalSearchResult(object? result)
    {
        if (result is GlobalSearchResultDto searchResult)
        {
            NavigateToSearchResult(searchResult);
            ShowGlobalSearchResults = false;
        }
    }

    private async void NavigateToSearchResult(GlobalSearchResultDto result)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"NavigateToSearchResult called - Type: {result.SearchType}, Title: {result.Title}, Module: {result.Module}");
            
            // Clear search to hide results popup
            GlobalSearchQuery = string.Empty;
            
            var searchType = result.SearchType.ToLowerInvariant();
            var module = result.Module.ToLowerInvariant();
            
            // First check if it's a page or feature navigation
            if (searchType == "page")
            {
                System.Diagnostics.Debug.WriteLine($"Navigating to page: {result.Data}");
                NavigateToPage(result.Data?.ToString() ?? "");
                StatusMessage = $"Navigated to: {result.Title}";
                return;
            }
            
            if (searchType == "feature")
            {
                System.Diagnostics.Debug.WriteLine($"Executing feature: {result.Data}");
                ExecuteFeature(result.Data?.ToString() ?? "");
                StatusMessage = $"Executed: {result.Title}";
                return;
            }
            
            // Handle entity-specific navigation with edit screens
            switch (searchType)
            {
                case "product":
                    System.Diagnostics.Debug.WriteLine($"Opening product for edit: {result.Title}");
                    // Open the AddProduct screen with prefilled product data
                    try
                    {
                        ProductDto? productDto = null;
                        
                        if (result.Data is ProductDto dto)
                        {
                            productDto = dto;
                        }
                        else if (result.Data is Product product)
                        {
                            // Fetch complete product data from service using ID
                            productDto = await _productService.GetProductByIdAsync(product.Id);
                        }
                        
                        if (productDto != null)
                        {
                            await ShowEditProduct(productDto);
                            StatusMessage = $"Editing product: {result.Title}";
                        }
                        else
                        {
                            // Fallback to product management if data not found
                            ShowProductManagement();
                            StatusMessage = $"Product not found - navigated to product management";
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError($"Error loading product for edit", ex);
                        ShowProductManagement();
                        StatusMessage = $"Error loading product - navigated to product management";
                    }
                    break;
                
                case "customer":
                    System.Diagnostics.Debug.WriteLine($"Opening customer for edit: {result.Title}");
                    // Open the customer management screen with customer edit side panel
                    try
                    {
                        CustomerDto? customerDto = null;
                        
                        if (result.Data is CustomerDto dto)
                        {
                            customerDto = dto;
                        }
                        else if (result.Data is Customer customer)
                        {
                            // Fetch complete customer data from service using ID
                            var customerService = _serviceProvider.GetRequiredService<ICustomerService>();
                            customerDto = await customerService.GetByIdAsync(customer.Id);
                        }
                        
                        if (customerDto != null)
                        {
                            await ShowEditCustomer(customerDto);
                            StatusMessage = $"Editing customer: {result.Title}";
                        }
                        else
                        {
                            // Fallback to customer management if data not found
                            await ShowCustomerManagement();
                            StatusMessage = $"Customer not found - navigated to customer management";
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError($"Error loading customer for edit", ex);
                        await ShowCustomerManagement();
                        StatusMessage = $"Error loading customer - navigated to customer management";
                    }
                    break;
                
                case "supplier":
                    System.Diagnostics.Debug.WriteLine($"Opening supplier for edit: {result.Title}");
                    // Open the supplier management screen with supplier edit side panel
                    try
                    {
                        SupplierDto? supplierDto = null;
                        
                        if (result.Data is SupplierDto dto)
                        {
                            supplierDto = dto;
                        }
                        else if (result.Data is Supplier supplier)
                        {
                            // Fetch complete supplier data from service using ID
                            var supplierService = _serviceProvider.GetRequiredService<ISupplierService>();
                            supplierDto = await supplierService.GetByIdAsync(supplier.SupplierId);
                        }
                        
                        if (supplierDto != null)
                        {
                            await ShowEditSupplier(supplierDto);
                            StatusMessage = $"Editing supplier: {result.Title}";
                        }
                        else
                        {
                            // Fallback to supplier management if data not found
                            await ShowSupplierManagement();
                            StatusMessage = $"Supplier not found - navigated to supplier management";
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError($"Error loading supplier for edit", ex);
                        await ShowSupplierManagement();
                        StatusMessage = $"Error loading supplier - navigated to supplier management";
                    }
                    break;
                
                case "sale":
                case "transaction":
                    System.Diagnostics.Debug.WriteLine($"Navigating to transaction: {result.Title}");
                    // Navigate to transaction screen
                    await ShowTransaction();
                    StatusMessage = $"Navigated to transactions";
                    break;
                
                case "stockadjustment":
                    System.Diagnostics.Debug.WriteLine($"Navigating to stock adjustment: {result.Title}");
                    // Navigate to stock management
                    await ShowStockManagement("StockAdjustment");
                    StatusMessage = $"Navigated to stock adjustments";
                    break;
                
                case "brand":
                    System.Diagnostics.Debug.WriteLine($"Opening brand for edit: {result.Title}");
                    // Open the brand management screen with brand edit side panel
                    try
                    {
                        BrandDto? brandDto = null;
                        
                        if (result.Data is BrandDto dto)
                        {
                            brandDto = dto;
                        }
                        else if (result.Data is Brand brand)
                        {
                            // Fetch complete brand data from service using ID
                            var brandService = _serviceProvider.GetRequiredService<IBrandService>();
                            brandDto = await brandService.GetByIdAsync(brand.Id);
                        }
                        
                        if (brandDto != null)
                        {
                            await ShowEditBrand(brandDto);
                            StatusMessage = $"Editing brand: {result.Title}";
                        }
                        else
                        {
                            // Fallback to add options if data not found
                            await ShowAddOptions();
                            StatusMessage = $"Brand not found - navigated to add options";
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError($"Error loading brand for edit", ex);
                        await ShowAddOptions();
                        StatusMessage = $"Error loading brand - navigated to add options";
                    }
                    break;
                
                case "category":
                    System.Diagnostics.Debug.WriteLine($"Opening category for edit: {result.Title}");
                    // Open the category management screen with category edit side panel
                    try
                    {
                        CategoryDto? categoryDto = null;
                        
                        if (result.Data is CategoryDto cdto)
                        {
                            categoryDto = cdto;
                        }
                        else if (result.Data is Category category)
                        {
                            // Fetch complete category data from service using ID
                            var categoryService = _serviceProvider.GetRequiredService<ICategoryService>();
                            categoryDto = await categoryService.GetByIdAsync(category.Id);
                        }
                        
                        if (categoryDto != null)
                        {
                            await ShowEditCategory(categoryDto);
                            StatusMessage = $"Editing category: {result.Title}";
                        }
                        else
                        {
                            // Fallback to add options if data not found
                            await ShowAddOptions();
                            StatusMessage = $"Category not found - navigated to add options";
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError($"Error loading category for edit", ex);
                        await ShowAddOptions();
                        StatusMessage = $"Error loading category - navigated to add options";
                    }
                    break;
                
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown search type: {result.SearchType}, defaulting to module-based navigation");
                    // Fallback to module-based navigation
                    NavigateToModuleByName(module);
                    StatusMessage = $"Navigated to {module}";
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating: {ex.Message}");
            AppLogger.LogError($"Error navigating to search result", ex);
            StatusMessage = $"Error navigating to result: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Navigate to a module by its name (fallback method)
    /// </summary>
    private async void NavigateToModuleByName(string moduleName)
    {
        switch (moduleName)
        {
            case "products":
                ShowProductManagement();
                break;
            case "customers":
                await ShowCustomerManagement();
                break;
            case "suppliers":
                await ShowSupplierManagement();
                break;
            case "stock":
                await ShowStockManagement();
                break;
            case "sales":
            case "transactions":
                await ShowTransaction();
                break;
            case "brands":
                await ShowBrand();
                break;
            case "categories":
                await ShowCategory();
                break;
            case "productgroups":
            case "product groups":
                await ShowProductGroups();
                break;
            case "settings":
                await ShowSettings();
                break;
            case "reports":
                ShowReports();
                break;
            case "reservations":
            case "restaurant":
                await ShowReservation();
                break;
            case "dashboard":
                await ShowDashboard();
                break;
            default:
                System.Diagnostics.Debug.WriteLine($"Unknown module: {moduleName}");
                break;
        }
    }
    
    private void NavigateToPage(string pageName)
    {
        // Map page names to navigation actions
        switch (pageName.ToLowerInvariant())
        {
            case "dashboard":
                _ = ShowDashboard();
                break;
            case "sales window":
                _ = ShowTransactions();
                break;
            case "transactions":
            case "transaction history":
                _ = ShowTransaction();
                break;
            case "back office":
                _ = ShowManagement(); // General management landing page
                break;
            case "product management":
                ShowProductManagement(); // Direct to product management
                break;
            case "add product":
                ShowAddProduct(); // Direct to add product screen
                break;
            case "add customer":
                _ = ShowCustomersWithAddPanel(); // Open customers screen with add sidepanel
                break;
            case "add product attribute":
            case "add attribute":
                _ = ShowProductAttributesWithAddPanel(); // Open product attributes with add sidepanel
                break;
            case "add product modifier":
            case "add modifier":
                _ = ShowProductModifiersWithAddPanel(); // Open product modifiers with add sidepanel
                break;
            case "add product combination":
            case "add combination":
                _ = ShowProductCombinationsWithAddPanel(); // Open product combinations with add sidepanel
                break;
            case "add product group":
            case "add group":
                _ = ShowProductGroupsWithAddPanel(); // Open product groups with add sidepanel
                break;
            case "add price type":
                _ = ShowPriceTypesWithAddPanel(); // Open price types with add sidepanel
                break;
            case "add payment type":
                _ = ShowPaymentTypesWithAddPanel(); // Open payment types with add sidepanel
                break;
            case "add tax rate":
            case "add tax type":
                _ = ShowTaxTypesWithAddPanel(); // Open tax types with add sidepanel
                break;
            case "add currency":
                _ = ShowCurrencyWithAddPanel(); // Open currency with add sidepanel
                break;
            case "add discount":
                _ = ShowDiscountsWithAddPanel(); // Open discounts with add sidepanel
                break;
            case "add shop":
            case "add store":
                _ = ShowStoreWithAddPanel(); // Open store with add sidepanel
                break;
            case "add uom":
            case "add unit of measurement":
                _ = ShowUomWithAddPanel(); // Open UOM with add sidepanel
                break;
            case "add supplier":
                _ = ShowSuppliersWithAddPanel(); // Open suppliers with add sidepanel
                break;
            case "add customer group":
                _ = ShowCustomerGroupsWithAddPanel(); // Open customer groups with add sidepanel
                break;
            case "product attributes":
                ShowProductAttributes(); // Dedicated screen for attributes
                break;
            case "product modifiers":
                ShowProductModifiers(); // Dedicated screen for modifiers
                break;
            case "product combinations":
                _ = ShowProductCombinations(); // Dedicated screen for combinations
                break;
            case "barcodes":
            case "product batches":
                ShowProductManagement(); // Barcodes/batches are in product management
                break;
            case "customer management":
                _ = ShowCustomerManagement(); // Customer Management landing screen
                break;
            case "customer":
            case "customers":
                _ = ShowCustomers(); // Customer list screen
                break;
            case "customer groups":
                _ = ShowCustomerGroups(); // Customer Groups screen
                break;
            case "supplier management":
                _ = ShowSupplierManagement(); // Supplier Management landing screen
                break;
            case "supplier":
            case "suppliers":
                _ = ShowSuppliers(); // Supplier list screen
                break;
            case "category management":
                _ = ShowCategory(); // Direct to category management
                break;
            case "brand management":
                _ = ShowBrand(); // Direct to brand management
                break;
            case "product groups":
                _ = ShowProductGroups(); // Direct to product groups management
                break;
            case "stock management":
                _ = ShowStockManagement(); // Direct to stock management (default section)
                break;
            case "stock adjustment":
                _ = ShowStockManagement("StockAdjustment"); // Stock management with Stock Adjustment section
                break;
            case "stock transfer":
                _ = ShowStockManagement("StockTransfer"); // Stock management with Stock Transfer section
                break;
            case "goods received":
                _ = ShowStockManagement("GoodsReceived"); // Stock management with Goods Received section
                break;
            case "goods return":
                _ = ShowStockManagement("GoodsReturn"); // Stock management with Goods Return section
                break;
            case "goods replace":
                _ = ShowStockManagement("GoodsReplace"); // Stock management with Goods Replace section
                break;
            case "settings":
                _ = ShowSettings();
                break;
            case "user settings":
                _ = ShowUserSettings();
                break;
            case "application settings":
                _ = ShowApplicationSettings();
                break;
            case "permissions":
                _ = ShowPermissions();
                break;
            case "roles":
                _ = ShowRoles();
                break;
            case "tax types":
            case "tax rates":
                _ = ShowTaxTypes(); // Dedicated screen for tax types
                break;
            case "payment types":
                _ = ShowPaymentTypes(); // Dedicated screen for payment types
                break;
            case "units of measurement":
                _ = ShowUom(); // Dedicated screen for UOM
                break;
            case "discounts":
                _ = ShowDiscounts(); // Dedicated screen for discounts
                break;
            case "currencies":
                _ = ShowCurrency(); // Dedicated screen for currencies
                break;
            case "price types":
                _ = ShowPriceTypes(); // Dedicated screen for price types
                break;
            case "shop management":
            case "store management":
                _ = ShowStore(); // Dedicated screen for shop/store
                break;
            case "service charge":
            case "service charges":
                _ = ShowServiceCharges(); // Dedicated screen for service charges
                break;
            case "themes":
            case "languages":
                _ = ShowSettings(); // Navigate to settings for these
                break;
            case "exchange transaction":
                _ = ShowTransaction("Exchange"); // Transaction screen with Exchange tab
                break;
            case "sales":
                _ = ShowTransaction("Sales"); // Transaction screen with Sales tab
                break;
            case "refund":
                _ = ShowTransaction("Refund"); // Transaction screen with Refund tab
                break;
            case "exchange":
                _ = ShowTransaction("Exchange"); // Transaction screen with Exchange tab
                break;
            case "reservations":
            case "restaurant tables":
                _ = ShowReservation();
                break;
            case "reports":
                ShowReports();
                break;
            default:
                StatusMessage = $"Navigation for '{pageName}' not configured yet";
                break;
        }
    }
    
    private void ExecuteFeature(string featureName)
    {
        // Map feature names to actions
        switch (featureName.ToLowerInvariant())
        {
            case "add new product":
            case "add product":
                ShowAddProduct(); // Go directly to add product screen
                break;
            case "add customer":
                _ = ShowCustomersWithAddPanel(); // Open customers screen with add sidepanel
                break;
            case "add supplier":
                _ = ShowSupplierManagement(); // Go directly to supplier management
                StatusMessage = "Click the Add Supplier button in Supplier Management";
                break;
            case "new sale":
                _ = ShowTransactions();
                break;
            case "add user":
                _ = ShowUserSettings();
                break;
            case "logout":
                Logout();
                break;
            case "change theme":
            case "change language":
                _ = ShowSettings();
                break;
            default:
                StatusMessage = $"Feature '{featureName}' not configured yet";
                break;
        }
    }

    #region GlobalSearchBar Event Handlers

    /// <summary>
    /// Handle navigation to search result from GlobalSearchBarViewModel
    /// </summary>
    private void OnNavigateToSearchResult(GlobalSearchResultDto result)
    {
        NavigateToSearchResult(result);
    }

    /// <summary>
    /// Handle "Show All Results" from GlobalSearchBarViewModel
    /// </summary>
    private void OnShowAllSearchResults(string query)
    {
        // You can implement a full search results page here
        // For now, just show a message
        StatusMessage = $"Showing all results for: {query}";
        
        // TODO: Navigate to a dedicated search results page/view
        // For now, this is a placeholder
        System.Diagnostics.Debug.WriteLine($"Show all search results for: {query}");
    }

    #endregion

    #region Language Switching Methods

    private async Task LoadLanguagesAsync()
    {
        try
        {
            var languages = await _databaseLocalizationService.GetAvailableLanguagesAsync();
            
            AvailableLanguages.Clear();
            foreach (var lang in languages)
            {
                AvailableLanguages.Add(lang);
            }

            // Set current language
            var currentLang = await _databaseLocalizationService.GetCurrentLanguageAsync();
            SelectedLanguage = currentLang ?? AvailableLanguages.FirstOrDefault();
            
            if (SelectedLanguage != null)
            {
                CurrentLanguageCode = SelectedLanguage.LanguageCode;
                CurrentLanguageDisplayName = SelectedLanguage.LanguageName;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading languages: {ex.Message}");
            
            // Fallback to default languages
            AvailableLanguages.Clear();
            AvailableLanguages.Add(new Language 
            { 
                LanguageCode = "en", 
                LanguageName = "English", 
                IsRtl = false 
            });
            AvailableLanguages.Add(new Language 
            { 
                LanguageCode = "ur", 
                LanguageName = "اردو", 
                IsRtl = true 
            });
            
            SelectedLanguage = AvailableLanguages.First();
            CurrentLanguageCode = "en";
            CurrentLanguageDisplayName = "English";
        }
    }

    private async Task ChangeLanguageAsync(Language? language)
    {
        if (language == null) return;

        try
        {
            await _databaseLocalizationService.SetCurrentLanguageAsync(language.LanguageCode);
            SelectedLanguage = language;
            CurrentLanguageCode = language.LanguageCode;
            CurrentLanguageDisplayName = language.LanguageName;
            
            StatusMessage = $"Language changed to {language.LanguageName}";
            
            // Update layout direction based on language
            var layoutDirectionService = _serviceProvider.GetService<ILayoutDirectionService>();
            if (layoutDirectionService != null)
            {
                if (language.IsRtl)
                {
                    layoutDirectionService.ChangeDirection(LayoutDirection.RightToLeft);
                }
                else
                {
                    layoutDirectionService.ChangeDirection(LayoutDirection.LeftToRight);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error changing language: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error changing language: {ex.Message}");
        }
    }

    private async Task ToggleLanguageAsync()
    {
        try
        {
            if (AvailableLanguages.Count < 2) return;

            // Find current language index
            var currentIndex = AvailableLanguages.IndexOf(SelectedLanguage ?? AvailableLanguages.First());
            
            // Toggle to next language (cycle through available languages)
            var nextIndex = (currentIndex + 1) % AvailableLanguages.Count;
            var nextLanguage = AvailableLanguages[nextIndex];
            
            await ChangeLanguageAsync(nextLanguage);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error toggling language: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"Error toggling language: {ex.Message}");
        }
    }

    #endregion

    #endregion
}


