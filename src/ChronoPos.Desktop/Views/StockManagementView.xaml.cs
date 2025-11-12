using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Models;
using DesktopFileLogger = ChronoPos.Desktop.Services.FileLogger;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for StockManagementView.xaml
/// </summary>
public partial class StockManagementView : UserControl
{
    public StockManagementView()
    {
        InitializeComponent();
        
        // DataContext will be set by MainWindowViewModel
        // DataContext = new StockManagementSimpleViewModel();
    }
    
    private async void SearchComboBox_DropDownOpened(object sender, EventArgs e)
    {
        DesktopFileLogger.Log("[StockManagementView] SearchComboBox_DropDownOpened called");
        AppLogger.LogInfo("SearchComboBox_DropDownOpened event triggered", "User opened product dropdown", "ui_events");
        
        // Get the ViewModel and trigger initial load if SearchResults is empty
        if (DataContext is ChronoPos.Desktop.ViewModels.StockManagementViewModel viewModel)
        {
            DesktopFileLogger.Log("[StockManagementView] Calling LoadInitialSearchResultsAsync");
            AppLogger.LogInfo("Calling LoadInitialSearchResultsAsync", "ViewModel found, loading products", "ui_events");
            await viewModel.LoadInitialSearchResultsAsync();
        }
        else
        {
            DesktopFileLogger.Log("[StockManagementView] DataContext is not StockManagementViewModel");
            AppLogger.LogWarning("DataContext is not StockManagementViewModel", "Unexpected DataContext type", "ui_events");
        }
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
    public ICommand SelectModuleCommand { get; set; }
    public ICommand CreateNewAdjustmentCommand { get; set; }
    public ICommand EditAdjustmentCommand { get; set; }
    public ICommand DeleteAdjustmentCommand { get; set; }
    public ICommand CompleteAdjustmentCommand { get; set; }
    public ICommand ClearFiltersCommand { get; set; }
    
    // Sidebar commands
    public ICommand OpenAdjustProductPanelCommand { get; set; }
    public ICommand CloseAdjustProductPanelCommand { get; set; }
    public ICommand SaveAdjustProductCommand { get; set; }
    public ICommand SearchProductCommand { get; set; }

    // Stock Transfer Commands
    public ICommand CreateNewTransferCommand { get; set; }
    public ICommand OpenTransferFormPanelCommand { get; set; }
    public ICommand CloseTransferFormPanelCommand { get; set; }
    public ICommand SaveTransferProductCommand { get; set; }

    // Content visibility properties
    public bool IsStockAdjustmentSelected { get; set; } = true; // Default to stock adjustment
    public bool IsStockTransferSelected { get; set; } = false;
    public bool IsGoodsReceivedSelected { get; set; } = false;
    public bool IsGoodsReturnSelected { get; set; } = false;
    public bool IsGoodsReplacedSelected { get; set; } = false;
    public bool NoModuleSelected { get; set; } = false;

    // Stock Adjustment properties
    public ObservableCollection<object> StockAdjustments { get; set; } = new();
    public object? SelectedAdjustment { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public string SelectedStatus { get; set; } = "All";

    // Sidebar properties
    private bool _isAdjustProductPanelOpen = false;
    private AdjustProductModel _adjustProduct = new();

    public bool IsAdjustProductPanelOpen
    {
        get => _isAdjustProductPanelOpen;
        set
        {
            _isAdjustProductPanelOpen = value;
            OnPropertyChanged();
        }
    }

    public AdjustProductModel AdjustProduct
    {
        get => _adjustProduct;
        set
        {
            _adjustProduct = value;
            OnPropertyChanged();
        }
    }

    // Transfer Panel Properties
    private bool _isTransferFormPanelOpen = false;
    private TransferProductModel _transferProduct = new();

    public bool IsTransferFormPanelOpen
    {
        get => _isTransferFormPanelOpen;
        set
        {
            _isTransferFormPanelOpen = value;
            OnPropertyChanged();
        }
    }

    public TransferProductModel TransferProduct
    {
        get => _transferProduct;
        set
        {
            _transferProduct = value;
            OnPropertyChanged();
        }
    }

    // Service references for theme integration
    private IThemeService? _themeService;
    private IColorSchemeService? _colorSchemeService;
    private ILocalizationService? _localizationService;
    private IZoomService? _zoomService;
    private ILayoutDirectionService? _layoutDirectionService;
    private IFontService? _fontService;
    private IDatabaseLocalizationService? _databaseLocalizationService;
    
    /// <summary>
    /// Navigation action for module navigation (set by MainWindowViewModel)
    /// </summary>
    public Action<string>? NavigateToModuleAction { get; set; }

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
        SelectModuleCommand = new RelayCommand<string>(SelectModule);
        CreateNewAdjustmentCommand = new RelayCommand(CreateNewAdjustment);
        EditAdjustmentCommand = new RelayCommand<object>(EditAdjustment);
        DeleteAdjustmentCommand = new RelayCommand<object>(DeleteAdjustment);
        CompleteAdjustmentCommand = new RelayCommand<object>(CompleteAdjustment);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        
        // Initialize sidebar commands
        OpenAdjustProductPanelCommand = new RelayCommand(OpenAdjustProductPanel);
        CloseAdjustProductPanelCommand = new RelayCommand(CloseAdjustProductPanel);
        SaveAdjustProductCommand = new RelayCommand(SaveAdjustProduct);
        SearchProductCommand = new RelayCommand(SearchProduct);

        // Initialize transfer commands
        CreateNewTransferCommand = new RelayCommand(CreateNewTransfer);
        OpenTransferFormPanelCommand = new RelayCommand(OpenTransferFormPanel);
        CloseTransferFormPanelCommand = new RelayCommand(CloseTransferFormPanel);
        SaveTransferProductCommand = new RelayCommand(SaveTransferProduct);

        // Initialize empty collection - modules will be loaded after theme services are set
        Modules = new ObservableCollection<StockModuleInfo>();
        
        // Initialize adjust product model
        InitializeAdjustProduct();
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
        
        // Call the navigation action if set (from MainWindowViewModel)
        NavigateToModuleAction?.Invoke(moduleType ?? "");
    }

    private void SelectModule(string? moduleType)
    {
        System.Diagnostics.Debug.WriteLine($"Select module: {moduleType}");
        
        // Reset all selections
        IsStockAdjustmentSelected = false;
        IsStockTransferSelected = false;
        IsGoodsReceivedSelected = false;
        IsGoodsReturnSelected = false;
        IsGoodsReplacedSelected = false;
        NoModuleSelected = false;

        // Set the selected module
        switch (moduleType)
        {
            case "StockAdjustment":
                IsStockAdjustmentSelected = true;
                LoadStockAdjustments();
                break;
            case "StockTransfer":
                IsStockTransferSelected = true;
                break;
            case "GoodsReceived":
                IsGoodsReceivedSelected = true;
                break;
            case "GoodsReturn":
                IsGoodsReturnSelected = true;
                break;
            case "GoodsReplaced":
                IsGoodsReplacedSelected = true;
                break;
            default:
                NoModuleSelected = true;
                break;
        }

        // Update module selection state
        foreach (var module in Modules)
        {
            module.IsSelected = module.ModuleType == moduleType;
        }

        // Notify property changes
        OnPropertyChanged(nameof(IsStockAdjustmentSelected));
        OnPropertyChanged(nameof(IsStockTransferSelected));
        OnPropertyChanged(nameof(IsGoodsReceivedSelected));
        OnPropertyChanged(nameof(IsGoodsReturnSelected));
        OnPropertyChanged(nameof(IsGoodsReplacedSelected));
        OnPropertyChanged(nameof(NoModuleSelected));
        OnPropertyChanged(nameof(Modules));
    }

    private void LoadStockAdjustments()
    {
        // TODO: Load actual stock adjustments from service
        // For now, add some dummy data
        StockAdjustments.Clear();
        
        var dummyAdjustments = new[]
        {
            new { AdjustmentNo = "ADJ-001", AdjustmentDate = DateTime.Now.AddDays(-1), Status = "Pending", LocationName = "Main Store", TotalItems = 5, CreatedByName = "Admin", Remarks = "Stock count correction" },
            new { AdjustmentNo = "ADJ-002", AdjustmentDate = DateTime.Now.AddDays(-2), Status = "Completed", LocationName = "Warehouse", TotalItems = 3, CreatedByName = "Manager", Remarks = "Damaged items removal" },
            new { AdjustmentNo = "ADJ-003", AdjustmentDate = DateTime.Now.AddDays(-3), Status = "Pending", LocationName = "Main Store", TotalItems = 8, CreatedByName = "Staff", Remarks = "New stock addition" }
        };

        foreach (var adj in dummyAdjustments)
        {
            StockAdjustments.Add(adj);
        }

        OnPropertyChanged(nameof(StockAdjustments));
    }

    private void CreateNewAdjustment()
    {
        System.Diagnostics.Debug.WriteLine("Create new adjustment");
        // Open the adjust product panel
        OpenAdjustProductPanel();
    }

    private void OpenAdjustProductPanel()
    {
        IsAdjustProductPanelOpen = true;
    }

    private void CloseAdjustProductPanel()
    {
        IsAdjustProductPanelOpen = false;
        InitializeAdjustProduct(); // Reset form
    }

    private void SaveAdjustProduct()
    {
        // TODO: Implement save logic
        new MessageDialog("Info", $"Saving adjustment: {AdjustProduct.ProductName} {AdjustProduct.AdjustmentType} by {AdjustProduct.Quantity}", MessageDialog.MessageType.Info).ShowDialog();
        CloseAdjustProductPanel();
    }

    private void SearchProduct()
    {
        // TODO: Implement product search
        new MessageDialog("Info", "Product search functionality will be implemented", MessageDialog.MessageType.Info).ShowDialog();
    }

    private void InitializeAdjustProduct()
    {
        AdjustProduct = new AdjustProductModel
        {
            ProductName = "",
            CurrentStock = 0m,
            AdjustmentType = "Increase",
            Quantity = "0",
            Reason = "Stock count correction"
        };
    }

    private void EditAdjustment(object? adjustment)
    {
        System.Diagnostics.Debug.WriteLine($"Edit adjustment: {adjustment}");
        // TODO: Implement edit adjustment
    }

    private void DeleteAdjustment(object? adjustment)
    {
        System.Diagnostics.Debug.WriteLine($"Delete adjustment: {adjustment}");
        // TODO: Implement delete adjustment
    }

    private void CompleteAdjustment(object? adjustment)
    {
        System.Diagnostics.Debug.WriteLine($"Complete adjustment: {adjustment}");
        // TODO: Implement complete adjustment
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatus = "All";
        OnPropertyChanged(nameof(SearchText));
        OnPropertyChanged(nameof(SelectedStatus));
        LoadStockAdjustments();
    }

    // Stock Transfer Methods
    private void CreateNewTransfer()
    {
        System.Diagnostics.Debug.WriteLine("CreateNewTransfer command executed!");
        
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        IsTransferFormPanelOpen = true;
        TransferProduct = new TransferProductModel();
        
        System.Diagnostics.Debug.WriteLine($"IsTransferFormPanelOpen set to: {IsTransferFormPanelOpen}");
    }

    private void OpenTransferFormPanel()
    {
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        IsTransferFormPanelOpen = true;
        TransferProduct = new TransferProductModel();
    }

    private void CloseTransferFormPanel()
    {
        IsTransferFormPanelOpen = false;
    }

    private void SaveTransferProduct()
    {
        // TODO: Implement save transfer logic
        new MessageDialog("Info", $"Saving transfer: {TransferProduct.ProductName} from {TransferProduct.FromShop} to {TransferProduct.ToShop} qty: {TransferProduct.Quantity}", MessageDialog.MessageType.Info).ShowDialog();
        CloseTransferFormPanel();
    }

    private async void LoadModules()
    {
        // Get dynamic translations if database service is available
        var stockAdjustmentTitle = "Stock Adjustment";
        var stockTransferTitle = "Stock Transfer";
        var goodsReceivedTitle = "Goods Received";
        var goodsReturnTitle = "Goods Return";
        var goodsReplacedTitle = "Goods Replaced";

        if (_databaseLocalizationService != null)
        {
            stockAdjustmentTitle = await _databaseLocalizationService.GetTranslationAsync("stock.adjustment");
            stockTransferTitle = await _databaseLocalizationService.GetTranslationAsync("stock.transfer");
            goodsReceivedTitle = await _databaseLocalizationService.GetTranslationAsync("stock.goods_received");
            goodsReturnTitle = await _databaseLocalizationService.GetTranslationAsync("stock.goods_return");
            goodsReplacedTitle = await _databaseLocalizationService.GetTranslationAsync("stock.goods_replaced");

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
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = true // Default selection
            },
            new StockModuleInfo
            {
                Title = stockTransferTitle,
                ModuleType = "StockTransfer", 
                ItemCount = 32,
                ItemCountLabel = "Transfers",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = false
            },
            new StockModuleInfo
            {
                Title = goodsReceivedTitle,
                ModuleType = "GoodsReceived",
                ItemCount = 67,
                ItemCountLabel = "Receipts",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = false
            },
            new StockModuleInfo
            {
                Title = goodsReturnTitle,
                ModuleType = "GoodsReturn",
                ItemCount = 15,
                ItemCountLabel = "Returns",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = false
            },
            new StockModuleInfo
            {
                Title = goodsReplacedTitle,
                ModuleType = "GoodsReplaced",
                ItemCount = 8,
                ItemCountLabel = "Replacements",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = false
            }
        };
        
        // Load stock adjustments by default
        LoadStockAdjustments();
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

public class StockModuleInfo : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isSelected = false;
    
    public string Title { get; set; } = string.Empty;
    public string ModuleType { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string ItemCountLabel { get; set; } = string.Empty;
    public Brush IconBackground { get; set; } = new SolidColorBrush(Colors.DodgerBlue);
    public Brush ButtonBackground { get; set; } = new SolidColorBrush(Colors.White);
    
    public bool IsSelected 
    { 
        get => _isSelected;
        set 
        { 
            _isSelected = value; 
            OnPropertyChanged();
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}

public class TransferProductModel : System.ComponentModel.INotifyPropertyChanged
{
    private int _productId = 0;
    private string _productName = string.Empty;
    private string _fromShop = string.Empty;
    private string _toShop = string.Empty;
    private string _productUnit = string.Empty;
    private decimal _quantity = 0;

    public int ProductId
    {
        get => _productId;
        set
        {
            if (_productId != value)
            {
                _productId = value;
                OnPropertyChanged();
            }
        }
    }

    public string ProductName
    {
        get => _productName;
        set
        {
            if (_productName != value)
            {
                _productName = value;
                OnPropertyChanged();
            }
        }
    }

    public string FromShop
    {
        get => _fromShop;
        set
        {
            if (_fromShop != value)
            {
                _fromShop = value;
                OnPropertyChanged();
            }
        }
    }

    public string ToShop
    {
        get => _toShop;
        set
        {
            if (_toShop != value)
            {
                _toShop = value;
                OnPropertyChanged();
            }
        }
    }

    public string ProductUnit
    {
        get => _productUnit;
        set
        {
            if (_productUnit != value)
            {
                _productUnit = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
