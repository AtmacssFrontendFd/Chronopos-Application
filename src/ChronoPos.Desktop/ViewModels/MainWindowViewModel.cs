using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChronoPos.Infrastructure.Services;
using System.Windows;
using System.Collections.ObjectModel;
using ChronoPos.Domain.Entities;

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
    private System.Timers.Timer? _searchDelayTimer;

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
    private string transactionsButtonText = "Transactions";

    [ObservableProperty]
    private string managementButtonText = "Management";

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

    // Commands
    public ICommand ClearGlobalSearchCommand { get; }
    public ICommand ShowAllGlobalSearchResultsCommand { get; }
    public ICommand ChangeLanguageCommand { get; }
    public ICommand ToggleLanguageCommand { get; }

    public MainWindowViewModel(IProductService productService, IServiceProvider serviceProvider)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _databaseLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
        _globalSearchService = serviceProvider.GetRequiredService<IGlobalSearchService>();
        
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
            
            TransactionsButtonText = await GetTranslationWithFallback("nav.sales", "Transactions");
            Console.WriteLine($"✅ [MainWindowViewModel] TransactionsButtonText = '{TransactionsButtonText}'");
            
            ManagementButtonText = await GetTranslationWithFallback("nav.management", "Management");
            Console.WriteLine($"✅ [MainWindowViewModel] ManagementButtonText = '{ManagementButtonText}'");
            
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
        TransactionsButtonText = "Transactions";
        ManagementButtonText = "Management";
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
        SelectedPage = "Dashboard";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_dashboard") ?? "Dashboard";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_dashboard_loaded") ?? "Dashboard loaded";
        
        // Create dashboard view content
        var dashboardContent = new System.Windows.Controls.StackPanel();
        var welcomeText = await _databaseLocalizationService.GetTranslationAsync("dashboard_welcome") ?? "Welcome to ChronoPos Point of Sale System";
        dashboardContent.Children.Add(new System.Windows.Controls.TextBlock 
        { 
            Text = welcomeText, 
            FontSize = 18, 
            Margin = new System.Windows.Thickness(0, 0, 0, 20) 
        });
        
        var statsPanel = new System.Windows.Controls.WrapPanel();
        
        // Quick stats cards with translated labels
        var todaySalesLabel = await _databaseLocalizationService.GetTranslationAsync("dashboard_today_sales") ?? "Today's Sales";
        var productsLabel = await _databaseLocalizationService.GetTranslationAsync("dashboard_total_products") ?? "Total Products";
        var customersLabel = await _databaseLocalizationService.GetTranslationAsync("dashboard_total_customers") ?? "Total Customers";
        
        var todaySalesCard = CreateStatsCard(todaySalesLabel, "$0.00", "#FF4CAF50");
        var productsCard = CreateStatsCard(productsLabel, "0", "#FF2196F3");
        var customersCard = CreateStatsCard(customersLabel, "0", "#FFFF9800");
        
        statsPanel.Children.Add(todaySalesCard);
        statsPanel.Children.Add(productsCard);
        statsPanel.Children.Add(customersCard);
        
        dashboardContent.Children.Add(statsPanel);
        CurrentView = dashboardContent;
    }

    [RelayCommand]
    private async Task ShowTransactions()
    {
        SelectedPage = "Transactions";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_transactions") ?? "Transactions";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_transactions_loaded") ?? "Transactions interface loaded";
        
        var transactionsContent = new System.Windows.Controls.Grid();
        transactionsContent.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(2, System.Windows.GridUnitType.Star) });
        transactionsContent.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
        
        // Product selection area
        var productAreaText = await _databaseLocalizationService.GetTranslationAsync("transactions_product_area") ?? "Product Selection Area\n(Products will be loaded here)";
        var productArea = new System.Windows.Controls.Border
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
            CornerRadius = new System.Windows.CornerRadius(5),
            Margin = new System.Windows.Thickness(0, 0, 10, 0),
            Child = new System.Windows.Controls.TextBlock
            {
                Text = productAreaText,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            }
        };
        System.Windows.Controls.Grid.SetColumn(productArea, 0);
        
        // Cart area
        var cartAreaText = await _databaseLocalizationService.GetTranslationAsync("transactions_cart_area") ?? "Transaction Cart\n(Cart items will be shown here)";
        var cartArea = new System.Windows.Controls.Border
        {
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue),
            CornerRadius = new System.Windows.CornerRadius(5),
            Child = new System.Windows.Controls.TextBlock
            {
                Text = cartAreaText,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                FontSize = 16
            }
        };
        System.Windows.Controls.Grid.SetColumn(cartArea, 1);
        
        transactionsContent.Children.Add(productArea);
        transactionsContent.Children.Add(cartArea);
        CurrentView = transactionsContent;
    }

    [RelayCommand]
    private async Task ShowManagement()
    {
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
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>()
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

    [RelayCommand]
    private void ShowProductManagement()
    {
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
            var productAttributeViewModel = new ProductAttributeViewModel(productAttributeService);
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

    private async Task ShowProductCombinations()
    {
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
                () => _ = ShowAddOptions() // Navigate back to Add Options
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
            
            // Create ViewModel with navigation callback
            var addProductViewModel = new AddProductViewModel(
                productService,
                brandService,
                productImageService,
                taxTypeService,
                discountService,
                productUnitService,
                skuGenerationService,
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
            
            // Create ViewModel with navigation callback
            var addProductViewModel = new AddProductViewModel(
                productService,
                brandService,
                productImageService,
                taxTypeService,
                discountService,
                productUnitService,
                skuGenerationService,
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

    [RelayCommand]
    private async Task ShowStockManagement()
    {
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

            // Get the StockManagementViewModel from DI container to ensure proper scoping
            var stockManagementViewModel = _serviceProvider.GetRequiredService<StockManagementViewModel>();
            
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
        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Add Options";
        StatusMessage = "Loading add options...";
        
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
                _serviceProvider.GetRequiredService<IDatabaseLocalizationService>()
            );

            // Set up navigation from add options to specific modules
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
                    case "ProductCombinations":
                        _ = ShowProductCombinations();
                        break;
                    default:
                        StatusMessage = $"Navigation to {moduleType} module not implemented yet";
                        break;
                }
            };

            // Set up back navigation to return to Management
            addOptionsViewModel.GoBackAction = () =>
            {
                ShowManagementCommand.Execute(null);
            };

            // Create the AddOptionsView and set its DataContext
            var addOptionsView = new AddOptionsView
            {
                DataContext = addOptionsViewModel
            };

            CurrentView = addOptionsView;
            StatusMessage = "Add options loaded successfully";
            await Task.CompletedTask; // satisfy analyzer
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading add options: {ex.Message}";
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
        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Discount Management";
        StatusMessage = "Loading discount management...";
        
        try
        {
            // Create the DiscountViewModel with all required services
            var discountViewModel = new DiscountViewModel(
                _serviceProvider.GetRequiredService<IDiscountService>(),
                _serviceProvider.GetRequiredService<IProductService>(),
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

    [RelayCommand]
    private async Task ShowUom()
    {
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

    [RelayCommand]
    private async Task ShowBrand()
    {
        // Don't change SelectedPage - keep it as "Management" so sidebar stays highlighted
        CurrentPageTitle = "Brand Management";
        StatusMessage = "Loading brand management...";
        
        try
        {
            // Create the BrandViewModel with all required services and navigation callback
            var brandViewModel = new BrandViewModel(
                _serviceProvider.GetRequiredService<IBrandService>(),
                navigateBack: () => ShowAddOptionsCommand.Execute(null)
            );

            // Create the BrandView and set its DataContext
            var brandView = new BrandView
            {
                DataContext = brandViewModel
            };

            CurrentView = brandView;
            StatusMessage = "Brand management loaded successfully";
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

    private async Task ShowCategory()
    {
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

    [RelayCommand]
    private void ShowReservation()
    {
        SelectedPage = "Reservation";
        CurrentPageTitle = "Reservation Management";
        StatusMessage = "Reservation interface loaded";
        
        var reservationContent = new System.Windows.Controls.TextBlock
        {
            Text = "Reservation Management\n(Customer reservation interface will be implemented here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = reservationContent;
    }

    [RelayCommand]
    private void ShowOrderTable()
    {
        SelectedPage = "OrderTable";
        CurrentPageTitle = "Order Table";
        StatusMessage = "Order table loaded";
        
        var orderTableContent = new System.Windows.Controls.TextBlock
        {
            Text = "Order Table\n(Order history and management will be shown here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = orderTableContent;
    }

    [RelayCommand]
    private void ShowReports()
    {
        SelectedPage = "Reports";
        CurrentPageTitle = "Reports";
        StatusMessage = "Reports interface loaded";
        
        var reportsContent = new System.Windows.Controls.TextBlock
        {
            Text = "Reports & Analytics\n(Business reports and analytics will be displayed here)",
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            FontSize = 16
        };
        CurrentView = reportsContent;
    }

    [RelayCommand]
    private async Task ShowSettings()
    {
        SelectedPage = "Settings";
        CurrentPageTitle = await _databaseLocalizationService.GetTranslationAsync("nav_settings") ?? "Settings";
        StatusMessage = await _databaseLocalizationService.GetTranslationAsync("status_loading_settings") ?? "Loading settings...";
        
        try
        {
            Console.WriteLine("ShowSettings: Starting to load settings");
            
            // Create and configure the settings view
            Console.WriteLine("ShowSettings: Creating SettingsView");
            var settingsView = new SettingsView();
            Console.WriteLine("ShowSettings: SettingsView created successfully");
            
            Console.WriteLine("ShowSettings: Getting SettingsViewModel from service provider");
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            Console.WriteLine("ShowSettings: SettingsViewModel retrieved successfully");
            
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
            System.Windows.MessageBox.Show($"Failed to load settings: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                "Settings Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Logout()
    {
        var result = System.Windows.MessageBox.Show(
            "Are you sure you want to logout?", 
            "Confirm Logout", 
            System.Windows.MessageBoxButton.YesNo, 
            System.Windows.MessageBoxImage.Question);
        
        if (result == System.Windows.MessageBoxResult.Yes)
        {
            StatusMessage = "Logging out...";
            
            // Here you can add any cleanup logic like:
            // - Clear user session
            // - Save any pending data
            // - Clear sensitive information
            
            // For now, we'll just show a confirmation and reset to dashboard
            System.Windows.MessageBox.Show(
                "You have been successfully logged out.", 
                "Logout Complete", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Information);
            
            // Reset to dashboard
            _ = ShowDashboard();
            CurrentUser = "Guest";
            StatusMessage = "Please login to continue";
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
            var results = await _globalSearchService.GetQuickSearchAsync(GlobalSearchQuery, 5);
            
            GlobalSearchResults.Clear();
            foreach (var result in results)
            {
                GlobalSearchResults.Add(result);
            }

            HasMoreGlobalSearchResults = results.Count >= 5;
            ShowGlobalSearchResults = GlobalSearchResults.Count > 0;
            
            // Update header with result count
            GlobalSearchResultsHeader = GlobalSearchResults.Count > 0 
                ? $"Found {GlobalSearchResults.Count} result{(GlobalSearchResults.Count != 1 ? "s" : "")}"
                : "No results found";
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

    private void ShowAllGlobalSearchResults()
    {
        if (HasGlobalSearchText)
        {
            // Navigate to a dedicated search results page
            StatusMessage = $"Showing all results for '{GlobalSearchQuery}'";
            ShowGlobalSearchResults = false;
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

    private void NavigateToSearchResult(GlobalSearchResultDto result)
    {
        try
        {
            switch (result.SearchType.ToLowerInvariant())
            {
                case "product":
                    // Navigate to product management and highlight the product
                    _ = ShowManagement();
                    StatusMessage = $"Navigated to product: {result.Title}";
                    break;
                
                case "customer":
                    // Navigate to customer management
                    StatusMessage = $"Navigated to customer: {result.Title}";
                    break;
                
                case "sale":
                    // Navigate to sales/transactions
                    _ = ShowTransactions();
                    StatusMessage = $"Navigated to sale: {result.Title}";
                    break;
                
                case "brand":
                case "category":
                    // Navigate to management
                    _ = ShowManagement();
                    StatusMessage = $"Navigated to {result.SearchType.ToLowerInvariant()}: {result.Title}";
                    break;
                
                default:
                    StatusMessage = $"Opening: {result.Title}";
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error navigating to result: {ex.Message}";
        }
    }

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
