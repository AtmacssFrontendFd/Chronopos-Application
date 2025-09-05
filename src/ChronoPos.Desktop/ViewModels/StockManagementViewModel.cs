using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Stock Management page
/// </summary>
public partial class StockManagementViewModel : ObservableObject
{
    #region Fields

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
    /// Collection of stock management modules
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<StockModuleInfo> _modules = new();

    /// <summary>
    /// Current theme name
    /// </summary>
    [ObservableProperty]
    private string _currentTheme = "Light";

    /// <summary>
    /// Current zoom level
    /// </summary>
    [ObservableProperty]
    private int _currentZoom = 100;

    /// <summary>
    /// Current language
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

    /// <summary>
    /// Current font family
    /// </summary>
    [ObservableProperty]
    private string _currentFontFamily = "Segoe UI";

    /// <summary>
    /// Current font size
    /// </summary>
    [ObservableProperty]
    private double _currentFontSize = 14;

    #endregion

    #region Commands

    /// <summary>
    /// Command to navigate to a specific stock module
    /// </summary>
    [RelayCommand]
    private void NavigateToModule(string moduleType)
    {
        // TODO: Implement navigation to specific stock module
        System.Diagnostics.Debug.WriteLine($"Navigating to {moduleType} module...");
    }

    /// <summary>
    /// Command to refresh stock modules data
    /// </summary>
    [RelayCommand]
    private async Task RefreshModulesAsync()
    {
        await LoadModuleDataAsync();
    }

    /// <summary>
    /// Command to go back to management page
    /// </summary>
    public ICommand? GoBackCommand { get; set; }

    #endregion

    #region Constructor

    public StockManagementViewModel(
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
        CurrentZoom = (int)_zoomService.CurrentZoomLevel;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString() ?? "English";
        CurrentColorScheme = _colorSchemeService.CurrentPrimaryColor?.Name ?? "Blue";
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        CurrentFontFamily = "Segoe UI"; // Default font family
        CurrentFontSize = GetFontSizeValue(_fontService.CurrentFontSize);
    }

    /// <summary>
    /// Load stock module data
    /// </summary>
    private async Task LoadModuleDataAsync()
    {
        await Task.Delay(500); // Simulate loading

        var modules = new List<StockModuleInfo>
        {
            new StockModuleInfo
            {
                Title = await _databaseLocalizationService.GetTranslationAsync("stock.adjustment"),
                ModuleType = "StockAdjustment",
                ItemCount = 125,
                ItemCountLabel = "Items",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            },
            new StockModuleInfo
            {
                Title = await _databaseLocalizationService.GetTranslationAsync("stock.transfer"),
                ModuleType = "StockTransfer", 
                ItemCount = 32,
                ItemCountLabel = "Transfers",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            },
            new StockModuleInfo
            {
                Title = await _databaseLocalizationService.GetTranslationAsync("stock.goods_received"),
                ModuleType = "GoodsReceived",
                ItemCount = 67,
                ItemCountLabel = "Receipts",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            },
            new StockModuleInfo
            {
                Title = await _databaseLocalizationService.GetTranslationAsync("stock.goods_return"),
                ModuleType = "GoodsReturn",
                ItemCount = 15,
                ItemCountLabel = "Returns",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            }
        };

        Modules.Clear();
        foreach (var module in modules)
        {
            Modules.Add(module);
        }
    }

    /// <summary>
    /// Get the primary color brush from color scheme service
    /// </summary>
    private Brush GetPrimaryColorBrush()
    {
        return new SolidColorBrush(_colorSchemeService.CurrentPrimaryColor?.Color ?? Colors.DodgerBlue);
    }

    /// <summary>
    /// Get the button background brush based on current theme
    /// </summary>
    private Brush GetButtonBackgroundBrush()
    {
        return _themeService.CurrentTheme == Theme.Dark 
            ? new SolidColorBrush(Color.FromRgb(45, 45, 48))
            : new SolidColorBrush(Colors.White);
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
    }

    private async void OnDatabaseLanguageChanged(object? sender, string newLanguageCode)
    {
        // Reload module data with new language
        await LoadModuleDataAsync();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleanup event subscriptions
    /// </summary>
    ~StockManagementViewModel()
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

    #region Helper Methods

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

    #endregion
}

/// <summary>
/// Information about a stock management module
/// </summary>
public class StockModuleInfo : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _moduleType = string.Empty;
    private int _itemCount = 0;
    private string _itemCountLabel = string.Empty;
    private Brush _iconBackground = new SolidColorBrush(Colors.DodgerBlue);
    private Brush _buttonBackground = new SolidColorBrush(Colors.White);

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string ModuleType
    {
        get => _moduleType;
        set
        {
            if (_moduleType != value)
            {
                _moduleType = value;
                OnPropertyChanged();
            }
        }
    }

    public int ItemCount
    {
        get => _itemCount;
        set
        {
            if (_itemCount != value)
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }
    }

    public string ItemCountLabel
    {
        get => _itemCountLabel;
        set
        {
            if (_itemCountLabel != value)
            {
                _itemCountLabel = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush IconBackground
    {
        get => _iconBackground;
        set
        {
            if (_iconBackground != value)
            {
                _iconBackground = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush ButtonBackground
    {
        get => _buttonBackground;
        set
        {
            if (_buttonBackground != value)
            {
                _buttonBackground = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
