using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for StockManagementView.xaml
/// </summary>
public partial class StockManagementView : UserControl
{
    public StockManagementView()
    {
        InitializeComponent();
        
        // Set a ViewModel that handles theme integration and commands
        DataContext = new StockManagementSimpleViewModel();
    }
}

public class StockManagementSimpleViewModel : System.ComponentModel.INotifyPropertyChanged
{
    private ObservableCollection<StockModuleInfo> _modules = new();
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;
    private string _currentTheme = "Light";
    private int _currentZoom = 100;
    private string _currentLanguage = "English";
    private string _currentColorScheme = "Blue";
    private string _currentFontFamily = "Segoe UI";
    private double _currentFontSize = 14;
    private string _backButtonText = "Back";
    private string _refreshButtonText = "Refresh";

    public ObservableCollection<StockModuleInfo> Modules 
    { 
        get => _modules; 
        set { _modules = value; OnPropertyChanged(); } 
    }
    
    public FlowDirection CurrentFlowDirection 
    { 
        get => _currentFlowDirection; 
        set { _currentFlowDirection = value; OnPropertyChanged(); } 
    }
    
    public string CurrentTheme 
    { 
        get => _currentTheme; 
        set { _currentTheme = value; OnPropertyChanged(); } 
    }
    
    public int CurrentZoom 
    { 
        get => _currentZoom; 
        set { _currentZoom = value; OnPropertyChanged(); } 
    }
    
    public string CurrentLanguage 
    { 
        get => _currentLanguage; 
        set { _currentLanguage = value; OnPropertyChanged(); } 
    }
    
    public string CurrentColorScheme 
    { 
        get => _currentColorScheme; 
        set { _currentColorScheme = value; OnPropertyChanged(); } 
    }
    
    public string BackButtonText
    {
        get => _backButtonText;
        set { _backButtonText = value; OnPropertyChanged(); }
    }

    public string RefreshButtonText
    {
        get => _refreshButtonText;
        set { _refreshButtonText = value; OnPropertyChanged(); }
    }
    
    public string CurrentFontFamily 
    { 
        get => _currentFontFamily; 
        set { _currentFontFamily = value; OnPropertyChanged(); } 
    }
    
    public double CurrentFontSize 
    { 
        get => _currentFontSize; 
        set { _currentFontSize = value; OnPropertyChanged(); } 
    }

    // Commands
    public ICommand GoBackCommand { get; set; }
    public ICommand RefreshModulesCommand { get; set; }
    public ICommand NavigateToModuleCommand { get; set; }

    // Service references for theme integration
    private IThemeService? _themeService;
    private IColorSchemeService? _colorSchemeService;
    private ILocalizationService? _localizationService;
    private IZoomService? _zoomService;
    private ILayoutDirectionService? _layoutDirectionService;
    private IFontService? _fontService;
    private IDatabaseLocalizationService? _databaseLocalizationService;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Set theme services for proper integration
    /// </summary>
    public void SetThemeServices(IThemeService? themeService, IColorSchemeService? colorSchemeService,
                                ILocalizationService? localizationService, IZoomService? zoomService,
                                ILayoutDirectionService? layoutDirectionService, IFontService? fontService,
                                IDatabaseLocalizationService? databaseLocalizationService = null)
    {
        _themeService = themeService;
        _colorSchemeService = colorSchemeService;
        _localizationService = localizationService;
        _zoomService = zoomService;
        _layoutDirectionService = layoutDirectionService;
        _fontService = fontService;
        _databaseLocalizationService = databaseLocalizationService;
        
        // Subscribe to direction changes
        if (_layoutDirectionService != null)
        {
            _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        }
        
        // Subscribe to database language changes
        if (_databaseLocalizationService != null)
        {
            _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;
        }
        
        // Update current flow direction
        if (_layoutDirectionService != null)
        {
            CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
                ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        
        // Now that theme services are set, load modules with proper theming
        LoadModules();
    }
    
    /// <summary>
    /// Initialize modules - call this after all theme properties are set
    /// </summary>
    public void InitializeModules()
    {
        LoadModules();
    }

    public StockManagementSimpleViewModel()
    { 
        // Initialize commands
        GoBackCommand = new RelayCommand(GoBack);
        RefreshModulesCommand = new RelayCommand(RefreshModules);
        NavigateToModuleCommand = new RelayCommand<string>(NavigateToModule);

        // Initialize empty collection - modules will be loaded after theme services are set
        Modules = new ObservableCollection<StockModuleInfo>();
    }

    private void GoBack()
    {
        // This will be handled by the MainWindow - for now just debug
        System.Diagnostics.Debug.WriteLine("Back button clicked");
        
        // This will be overridden by MainWindowViewModel
    }

    private void RefreshModules()
    {
        System.Diagnostics.Debug.WriteLine("Refresh button clicked");
        
        // Update color scheme and theme values
        UpdateThemeSettings();
        
        // Reload the modules with updated colors
        LoadModules();
        
        // Notify that properties have changed to update the UI
        OnPropertyChanged(nameof(Modules));
        OnPropertyChanged(nameof(CurrentTheme));
        OnPropertyChanged(nameof(CurrentColorScheme));
    }
    
    private void UpdateThemeSettings()
    {
        // Update theme settings if they were passed from MainWindow
        // This will be set by MainWindowViewModel when creating the view
    }

    private void NavigateToModule(string? moduleType)
    {
        System.Diagnostics.Debug.WriteLine($"Navigate to module: {moduleType}");
        // TODO: Implement navigation to specific stock modules
    }

    private async void LoadModules()
    {
        // Get dynamic translations if database service is available
        var stockAdjustmentTitle = "Stock Adjustment";
        var stockTransferTitle = "Stock Transfer";
        var goodsReceivedTitle = "Goods Received";
        var goodsReturnTitle = "Goods Return";

        if (_databaseLocalizationService != null)
        {
            stockAdjustmentTitle = await _databaseLocalizationService.GetTranslationAsync("stock.adjustment");
            stockTransferTitle = await _databaseLocalizationService.GetTranslationAsync("stock.transfer");
            goodsReceivedTitle = await _databaseLocalizationService.GetTranslationAsync("stock.goods_received");
            goodsReturnTitle = await _databaseLocalizationService.GetTranslationAsync("stock.goods_return");

            // Load button text translations
            BackButtonText = await _databaseLocalizationService.GetTranslationAsync("btn.back");
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("btn.refresh");
        }

        Modules = new ObservableCollection<StockModuleInfo>
        {
            new StockModuleInfo
            {
                Title = stockAdjustmentTitle,
                ModuleType = "StockAdjustment",
                ItemCount = 125,
                ItemCountLabel = "Items",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            },
            new StockModuleInfo
            {
                Title = stockTransferTitle,
                ModuleType = "StockTransfer", 
                ItemCount = 32,
                ItemCountLabel = "Transfers",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            },
            new StockModuleInfo
            {
                Title = goodsReceivedTitle,
                ModuleType = "GoodsReceived",
                ItemCount = 67,
                ItemCountLabel = "Receipts",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            },
            new StockModuleInfo
            {
                Title = goodsReturnTitle,
                ModuleType = "GoodsReturn",
                ItemCount = 15,
                ItemCountLabel = "Returns",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush()
            }
        };
    }

    private Brush GetPrimaryColorBrush()
    {
        // Use the actual color service if available
        if (_colorSchemeService?.CurrentPrimaryColor != null)
        {
            return new SolidColorBrush(_colorSchemeService.CurrentPrimaryColor.Color);
        }
        
        // Fallback to default blue
        return new SolidColorBrush(Colors.DodgerBlue);
    }

    private Brush GetButtonBackgroundBrush()
    {
        // Use the actual theme service if available
        if (_themeService != null)
        {
            return _themeService.CurrentTheme == Theme.Dark 
                ? new SolidColorBrush(Color.FromRgb(45, 45, 45))
                : new SolidColorBrush(Colors.White);
        }
        
        // Fallback based on current theme string
        return CurrentTheme == "Dark" 
            ? new SolidColorBrush(Color.FromRgb(45, 45, 45))
            : new SolidColorBrush(Colors.White);
    }

    /// <summary>
    /// Handle direction changes and reorder modules
    /// </summary>
    private void OnDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        // Reorder modules based on the new direction
        ReorderModulesForDirection(newDirection);
    }

    /// <summary>
    /// Handle database language changes and reload modules
    /// </summary>
    private async void OnDatabaseLanguageChanged(object? sender, string newLanguageCode)
    {
        // Reload modules with new language
        LoadModules();
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
}

public class StockModuleInfo
{
    public string Title { get; set; } = string.Empty;
    public string ModuleType { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string ItemCountLabel { get; set; } = string.Empty;
    public Brush IconBackground { get; set; } = new SolidColorBrush(Colors.DodgerBlue);
    public Brush ButtonBackground { get; set; } = new SolidColorBrush(Colors.White);
}
