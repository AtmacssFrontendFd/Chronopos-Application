using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Management page with all settings integration
/// Provides management modules with zoom-aware UI and multi-language support
/// </summary>
public partial class ManagementViewModel : ObservableObject
{
    #region Private Fields

    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of management modules
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ManagementModuleInfo> _modules = new();

    /// <summary>
    /// Current page title with localization support
    /// </summary>
    [ObservableProperty]
    private string _pageTitle = "Management";

    /// <summary>
    /// Current theme (Light/Dark)
    /// </summary>
    [ObservableProperty]
    private string _currentTheme = "Light";

    /// <summary>
    /// Current zoom level percentage
    /// </summary>
    [ObservableProperty]
    private double _currentZoom = 100.0;

    /// <summary>
    /// Current language for UI
    /// </summary>
    [ObservableProperty]
    private string _currentLanguage = "English";

    /// <summary>
    /// Current color scheme
    /// </summary>
    [ObservableProperty]
    private string _currentColorScheme = "Blue";

    /// <summary>
    /// Current flow direction for RTL/LTR support
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    #endregion

    #region Commands

    /// <summary>
    /// Navigation action for module navigation (set by parent)
    /// </summary>
    public Action<string>? NavigateToModuleAction { get; set; }

    /// <summary>
    /// Command to navigate to a specific management module
    /// </summary>
    [RelayCommand]
    private void NavigateToModule(string moduleType)
    {
        if (NavigateToModuleAction != null)
        {
            NavigateToModuleAction(moduleType);
        }
        else
        {
            // Fallback for debug
            MessageBox.Show($"Navigating to {moduleType} module", "Navigation", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Command to refresh module data
    /// </summary>
    [RelayCommand]
    private async Task RefreshModulesAsync()
    {
        await LoadModuleDataAsync();
    }

    #endregion

    #region Constructor

    public ManagementViewModel(
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _themeService = themeService;
        _zoomService = zoomService;
        _localizationService = localizationService;
        _colorSchemeService = colorSchemeService;
        _layoutDirectionService = layoutDirectionService;
        _fontService = fontService;
        _databaseLocalizationService = databaseLocalizationService;

        // Subscribe to settings changes
        _themeService.ThemeChanged += OnThemeChanged;
        _zoomService.ZoomChanged += OnZoomChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _colorSchemeService.PrimaryColorChanged += OnPrimaryColorChanged;
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;

        // Initialize with current settings
        InitializeSettings();
        
        // Load module data
        _ = LoadModuleDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize all settings with current values
    /// </summary>
    private void InitializeSettings()
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = _zoomService.CurrentZoomPercentage;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString();
        CurrentColorScheme = _colorSchemeService.CurrentPrimaryColor.Name;
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        // Update page title with database localization
        _ = UpdatePageTitleAsync();
    }

    /// <summary>
    /// Update page title with database localization
    /// </summary>
    private async Task UpdatePageTitleAsync()
    {
        PageTitle = await _databaseLocalizationService.GetTranslationAsync("nav.management");
    }

    /// <summary>
    /// Load management module data with localized content
    /// </summary>
    private async Task LoadModuleDataAsync()
    {
        // Clear existing modules
        Modules.Clear();

        // Get the primary color brush for all icons
        var primaryColorBrush = GetPrimaryColorBrush();
        var buttonBackgroundBrush = GetButtonBackgroundBrush();

        // Create modules with localized content from database (All 6 original modules)
        var moduleData = new[]
        {
            new { Type = "Stock", TitleKey = "management.stock", CountLabel = "Items", Count = await GetStockCountAsync() },
            new { Type = "Product", TitleKey = "management.products", CountLabel = "Products", Count = await GetProductCountAsync() },
            new { Type = "Supplier", TitleKey = "management.supplier", CountLabel = "Suppliers", Count = await GetSupplierCountAsync() },
            new { Type = "Customer", TitleKey = "management.customers", CountLabel = "Customers", Count = await GetCustomerCountAsync() },
            new { Type = "Payment", TitleKey = "management.payment", CountLabel = "Transactions", Count = await GetPaymentCountAsync() },
            new { Type = "Service", TitleKey = "management.service", CountLabel = "Services", Count = await GetServiceCountAsync() }
        };

        // Add modules to collection - All use the same primary color
        for (int i = 0; i < moduleData.Length; i++)
        {
            var data = moduleData[i];
            
            Modules.Add(new ManagementModuleInfo
            {
                ModuleType = data.Type,
                Title = await _databaseLocalizationService.GetTranslationAsync(data.TitleKey),
                ItemCount = data.Count,
                ItemCountLabel = data.CountLabel, // TODO: Make this dynamic too
                IconBackground = primaryColorBrush, // Same primary color for all icons
                ButtonBackground = buttonBackgroundBrush
            });
        }
        
        // Reorder modules based on current direction after loading
        ReorderModulesForDirection(_layoutDirectionService.CurrentDirection);
    }

    /// <summary>
    /// Get module colors based on current color scheme - All icons use the same primary color
    /// </summary>
    private Brush GetPrimaryColorBrush()
    {
        var primaryColor = _colorSchemeService.CurrentPrimaryColor;
        return new SolidColorBrush(primaryColor.Color);
    }

    /// <summary>
    /// Get button background brush based on current theme
    /// </summary>
    private Brush GetButtonBackgroundBrush()
    {
        return CurrentTheme == "Dark" 
            ? new SolidColorBrush(Color.FromRgb(45, 45, 45))   // Dark background
            : new SolidColorBrush(Color.FromRgb(255, 255, 255)); // Light background
    }

    /// <summary>
    /// Get localized string using the localization service
    /// </summary>
    private string GetLocalizedString(string key)
    {
        try
        {
            return _localizationService.GetString(key);
        }
        catch
        {
            // Return the key if localization fails
            return key;
        }
    }

    /// <summary>
    /// Update all localized strings
    /// </summary>
    private void UpdateLocalizedStrings()
    {
        PageTitle = GetLocalizedString("Management");
        
        // Refresh modules to update localized content
        if (Modules.Any())
        {
            _ = LoadModuleDataAsync();
        }
    }

    #endregion

    #region Data Loading Methods (Mock Implementation)

    private async Task<int> GetStockCountAsync()
    {
        // TODO: Replace with actual data service call
        await Task.Delay(10);
        return 150;
    }

    private async Task<int> GetProductCountAsync()
    {
        // TODO: Replace with actual data service call
        await Task.Delay(10);
        return 75;
    }

    private async Task<int> GetSupplierCountAsync()
    {
        // TODO: Replace with actual data service call
        await Task.Delay(10);
        return 25;
    }

    private async Task<int> GetCustomerCountAsync()
    {
        // TODO: Replace with actual data service call
        await Task.Delay(10);
        return 120;
    }

    private async Task<int> GetPaymentCountAsync()
    {
        // TODO: Replace with actual data service call
        await Task.Delay(10);
        return 340;
    }

    private async Task<int> GetServiceCountAsync()
    {
        // TODO: Replace with actual data service call
        await Task.Delay(10);
        return 15;
    }

    #endregion

    #region Event Handlers

    private void OnThemeChanged(Theme newTheme)
    {
        CurrentTheme = newTheme.ToString();
        
        // Update button backgrounds for all modules
        foreach (var module in Modules)
        {
            module.ButtonBackground = GetButtonBackgroundBrush();
        }
    }

    private void OnZoomChanged(ZoomLevel newZoom)
    {
        CurrentZoom = (int)newZoom;
        // UI will automatically respond to zoom changes through resource bindings
    }

    private void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        CurrentLanguage = newLanguage.ToString();
        UpdateLocalizedStrings();
    }

    private void OnPrimaryColorChanged(ColorOption newColor)
    {
        CurrentColorScheme = newColor.Name;
        
        // Update all module icons to use the same new primary color
        var newPrimaryColorBrush = new SolidColorBrush(newColor.Color);
        foreach (var module in Modules)
        {
            module.IconBackground = newPrimaryColorBrush;
        }
    }

    private void OnDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        // Reorder modules based on the new direction
        ReorderModulesForDirection(newDirection);
    }

    private async void OnDatabaseLanguageChanged(object? sender, string newLanguageCode)
    {
        // Reload module data with new language and update page title
        PageTitle = await _databaseLocalizationService.GetTranslationAsync("nav.management");
        await LoadModuleDataAsync();
    }

    /// <summary>
    /// Reorder modules based on layout direction
    /// </summary>
    private void ReorderModulesForDirection(LayoutDirection direction)
    {
        if (Modules.Count == 0) return;

        // Get current modules as list
        var modulesList = Modules.ToList();
        
        // Clear the collection
        Modules.Clear();
        
        // Add modules in the appropriate order
        if (direction == LayoutDirection.RightToLeft)
        {
            // Reverse order for RTL
            foreach (var module in modulesList.AsEnumerable().Reverse())
            {
                Modules.Add(module);
            }
        }
        else
        {
            // Normal order for LTR
            foreach (var module in modulesList)
            {
                Modules.Add(module);
            }
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleanup event subscriptions
    /// </summary>
    ~ManagementViewModel()
    {
        // Unsubscribe from events
        if (_themeService != null)
            _themeService.ThemeChanged -= OnThemeChanged;
        if (_zoomService != null)
            _zoomService.ZoomChanged -= OnZoomChanged;
        if (_localizationService != null)
            _localizationService.LanguageChanged -= OnLanguageChanged;
        if (_colorSchemeService != null)
            _colorSchemeService.PrimaryColorChanged -= OnPrimaryColorChanged;
        if (_layoutDirectionService != null)
            _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        if (_databaseLocalizationService != null)
            _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
    }

    #endregion
}

/// <summary>
/// Model class for management module information
/// </summary>
public partial class ManagementModuleInfo : ObservableObject
{
    [ObservableProperty]
    private string _moduleType = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private int _itemCount;

    [ObservableProperty]
    private string _itemCountLabel = string.Empty;

    [ObservableProperty]
    private Brush _iconBackground = new SolidColorBrush(Colors.Blue);

    [ObservableProperty]
    private Brush _buttonBackground = new SolidColorBrush(Colors.White);
}
