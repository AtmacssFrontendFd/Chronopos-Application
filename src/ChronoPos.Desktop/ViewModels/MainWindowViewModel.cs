using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Infrastructure.Services;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Main window view model for the ChronoPos Desktop application
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;

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

    public MainWindowViewModel(IProductService productService, IServiceProvider serviceProvider)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _databaseLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
        
        // Subscribe to language change events
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
        
        // Initialize translations and ensure keywords exist
        _ = InitializeTranslationsAsync();
        
        // Start timer to update date/time
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += (s, e) => CurrentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy - HH:mm:ss");
        timer.Start();

        // Show dashboard by default
        ShowDashboard();
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

        // Add keywords to database
        foreach (var keyword in keywords)
        {
            // Add the keyword first
            await _databaseLocalizationService.AddLanguageKeywordAsync(keyword.Key, $"Translation key for {keyword.Key}");
            
            // Add translations for each language
            foreach (var translation in keyword.Value)
            {
                await _databaseLocalizationService.SaveTranslationAsync(keyword.Key, translation.Value, translation.Key);
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
            
            ReservationButtonText = await GetTranslationWithFallback("nav.customers", "Reservation");
            Console.WriteLine($"✅ [MainWindowViewModel] ReservationButtonText = '{ReservationButtonText}'");
            
            OrderTableButtonText = await GetTranslationWithFallback("nav.customers", "Order Table");
            Console.WriteLine($"✅ [MainWindowViewModel] OrderTableButtonText = '{OrderTableButtonText}'");
            
            ReportsButtonText = await GetTranslationWithFallback("nav.sales", "Reports");
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
                    case "Stock":
                        ShowStockManagement();
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

            // Create the view
            var stockManagementView = new StockManagementView();
            
            // Set up the back command and theme integration
            if (stockManagementView.DataContext is Views.StockManagementSimpleViewModel viewModel)
            {
                // Set up the back command to return to management properly
                viewModel.GoBackCommand = new RelayCommand(() =>
                {
                    ShowManagementCommand.Execute(null);
                });

                // Set up navigation from stock management to specific modules
                viewModel.NavigateToModuleAction = (moduleType) =>
                {
                    switch (moduleType)
                    {
                        case "StockAdjustment":
                            ShowStockAdjustment();
                            break;
                        default:
                            StatusMessage = $"Navigation to {moduleType} not implemented yet";
                            break;
                    }
                };

                // Pass the actual service instances for dynamic updates
                viewModel.SetThemeServices(themeService, colorSchemeService, localizationService, 
                                         zoomService, layoutDirectionService, fontService, databaseLocalizationService);

                // Integrate with theme services if available
                if (themeService != null)
                {
                    viewModel.CurrentTheme = themeService.CurrentTheme.ToString();
                }
                if (colorSchemeService != null)
                {
                    viewModel.CurrentColorScheme = colorSchemeService.CurrentPrimaryColor?.Name ?? "Blue";
                }
                if (localizationService != null)
                {
                    viewModel.CurrentLanguage = localizationService.CurrentLanguage.ToString();
                }
                if (zoomService != null)
                {
                    viewModel.CurrentZoom = (int)zoomService.CurrentZoomLevel;
                }
                if (layoutDirectionService != null)
                {
                    viewModel.CurrentFlowDirection = layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
                        ? System.Windows.FlowDirection.RightToLeft : System.Windows.FlowDirection.LeftToRight;
                }
                if (fontService != null)
                {
                    viewModel.CurrentFontFamily = "Segoe UI"; // Use default font family
                    viewModel.CurrentFontSize = GetFontSizeValue(fontService.CurrentFontSize);
                }
                
                // Initialize modules after all theme properties are set
                viewModel.InitializeModules();
            }

            CurrentView = stockManagementView;
            StatusMessage = "Stock management loaded successfully";
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
    /// Show stock adjustment view
    /// </summary>
    private void ShowStockAdjustment()
    {
        CurrentPageTitle = "Stock Adjustment";
        StatusMessage = "Loading stock adjustment...";
        
        try
        {
            // Create the stock adjustment view
            var stockAdjustmentView = new StockAdjustmentView();
            
            // Set the data context using the service provider
            stockAdjustmentView.SetDataContext(_serviceProvider);
            
            // Set up the back command to return to stock management
            if (stockAdjustmentView.DataContext is StockAdjustmentViewModel viewModel)
            {
                viewModel.GoBackCommand = new RelayCommand(() =>
                {
                    ShowStockManagement();
                });
            }
            
            CurrentView = stockAdjustmentView;
            StatusMessage = "Stock adjustment loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading stock adjustment: {ex.Message}";
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
            ShowDashboard();
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
}
