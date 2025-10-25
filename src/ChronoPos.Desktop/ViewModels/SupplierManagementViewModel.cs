using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Supplier Management page
/// Provides access to Supplier module
/// </summary>
public partial class SupplierManagementViewModel : ObservableObject
{
    #region Private Fields

    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISupplierService _supplierService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of supplier management modules
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ManagementModuleInfo> _modules = new();

    /// <summary>
    /// Current page title with localization support
    /// </summary>
    [ObservableProperty]
    private string _pageTitle = "Supplier Management";

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
    /// Back navigation action (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    /// <summary>
    /// Command to navigate to a specific supplier module
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
    /// Command to go back to previous screen
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        if (GoBackAction != null)
        {
            GoBackAction();
        }
        else
        {
            // Fallback for debug
            MessageBox.Show("Going back to Management", "Navigation", 
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

    public SupplierManagementViewModel(
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService,
        ICurrentUserService currentUserService,
        ISupplierService supplierService)
    {
        _themeService = themeService;
        _zoomService = zoomService;
        _localizationService = localizationService;
        _colorSchemeService = colorSchemeService;
        _layoutDirectionService = layoutDirectionService;
        _fontService = fontService;
        _databaseLocalizationService = databaseLocalizationService;
        _currentUserService = currentUserService;
        _supplierService = supplierService;

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
        PageTitle = await _databaseLocalizationService.GetTranslationAsync("management.supplier") ?? "Supplier Management";
    }

    /// <summary>
    /// Load supplier management module data with localized content
    /// </summary>
    private async Task LoadModuleDataAsync()
    {
        // Clear existing modules
        Modules.Clear();

        // Get the primary color brush for all icons
        var primaryColorBrush = GetPrimaryColorBrush();
        var buttonBackgroundBrush = GetButtonBackgroundBrush();

        // Create module with localized content from database (1 module)
        var moduleData = new[]
        {
            new { Type = "Suppliers", TitleKey = "add_options.suppliers", CountLabel = "Suppliers", Count = await GetSupplierCountAsync() }
        };

        // Add module to collection
        for (int i = 0; i < moduleData.Length; i++)
        {
            var data = moduleData[i];
            
            Modules.Add(new ManagementModuleInfo
            {
                ModuleType = data.Type,
                Title = await _databaseLocalizationService.GetTranslationAsync(data.TitleKey) ?? data.Type,
                ItemCount = data.Count,
                ItemCountLabel = data.CountLabel,
                IconBackground = primaryColorBrush,
                ButtonBackground = buttonBackgroundBrush
            });
        }
        
        // Reorder modules based on current direction after loading
        ReorderModulesForDirection(_layoutDirectionService.CurrentDirection);
    }

    /// <summary>
    /// Get module colors based on current color scheme
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

    #endregion

    #region Data Loading Methods

    private async Task<int> GetSupplierCountAsync()
    {
        try
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return suppliers.Count();
        }
        catch
        {
            return 0;
        }
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
        _ = LoadModuleDataAsync();
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
        PageTitle = await _databaseLocalizationService.GetTranslationAsync("management.supplier") ?? "Supplier Management";
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
    ~SupplierManagementViewModel()
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
