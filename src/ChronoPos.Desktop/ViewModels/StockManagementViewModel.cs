using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopFileLogger = ChronoPos.Desktop.Services.FileLogger;
using ChronoPos.Desktop.Models;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;

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
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;

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

    // Module Selection Properties
    [ObservableProperty]
    private bool _isStockAdjustmentSelected = true;

    [ObservableProperty]
    private bool _isStockTransferSelected = false;

    [ObservableProperty]
    private bool _isGoodsReceivedSelected = false;

    [ObservableProperty]
    private bool _isGoodsReturnSelected = false;

    [ObservableProperty]
    private bool _noModuleSelected = false;

    // Stock Transfer Properties
    [ObservableProperty]
    private ObservableCollection<StockTransferDto> _stockTransfers = new();

    [ObservableProperty]
    private StockTransferDto? _selectedTransfer;

    // Stock Adjustment Properties
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentDto> _stockAdjustments = new();

    [ObservableProperty]
    private StockAdjustmentDto? _selectedAdjustment;

    // Flattened collection for item-level display with financial calculations
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentItemDto> _stockAdjustmentItems = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private bool _isTransferFormPanelOpen = false;

    [ObservableProperty]
    private bool _isAdjustProductPanelOpen = false;

    [ObservableProperty]
    private TransferProductModel _transferProduct = new();

    [ObservableProperty]
    private AdjustProductModel _adjustProduct = new();

    // Product Search Properties
    [ObservableProperty]
    private ObservableCollection<ProductDto> _searchResults = new();

    [ObservableProperty]
    private ObservableCollection<StockAdjustmentReasonDto> _adjustmentReasons = new();

    // Services for data access
    private readonly IProductService? _productService;
    private readonly IStockAdjustmentService? _stockAdjustmentService;

    // Debouncing timer for search
    private readonly DispatcherTimer _searchDebounceTimer;
    private string _pendingSearchTerm = string.Empty;
    private bool _isUpdatingFromSelection = false;
    private ProductDto? _lastSelectedProduct = null;

    // Common Properties for both modules
    [ObservableProperty]
    private string _backButtonText = "Back";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    #endregion

    #region Logging

    /// <summary>
    /// Log a message to both console and file
    /// </summary>
    private static void LogMessage(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        System.Diagnostics.Debug.WriteLine(logEntry);
        Console.WriteLine(logEntry);
        
        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
            var logFilePath = Path.Combine(chronoPosPath, "app.log");
            
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors
        }
    }

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
    /// Command to select a specific module
    /// </summary>
    [RelayCommand]
    private void SelectModule(string moduleType)
    {
        // Reset all selections
        IsStockAdjustmentSelected = false;
        IsStockTransferSelected = false;
        IsGoodsReceivedSelected = false;
        IsGoodsReturnSelected = false;
        NoModuleSelected = false;

        // Update module selections
        foreach (var module in Modules)
        {
            module.IsSelected = module.ModuleType == moduleType;
        }

        // Set the appropriate selection
        switch (moduleType)
        {
            case "StockAdjustment":
                IsStockAdjustmentSelected = true;
                break;
            case "StockTransfer":
                IsStockTransferSelected = true;
                LoadStockTransfersAsync();
                break;
            case "GoodsReceived":
                IsGoodsReceivedSelected = true;
                break;
            case "GoodsReturn":
                IsGoodsReturnSelected = true;
                break;
            default:
                NoModuleSelected = true;
                break;
        }
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
    /// Command to refresh stock adjustments data specifically
    /// </summary>
    [RelayCommand]
    private async Task RefreshStockAdjustments()
    {
        LogMessage("[RefreshStockAdjustments] Refreshing stock adjustments data");
        await LoadStockAdjustmentsAsync();
    }

    /// <summary>
    /// Command to go back to management page
    /// </summary>
    public ICommand? GoBackCommand { get; set; }

    // Stock Transfer Commands
    [RelayCommand]
    private void CreateNewTransfer()
    {
        System.Diagnostics.Debug.WriteLine("CreateNewTransfer command executed!");
        
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        IsTransferFormPanelOpen = true;
        TransferProduct = new TransferProductModel();
        
        System.Diagnostics.Debug.WriteLine($"IsTransferFormPanelOpen set to: {IsTransferFormPanelOpen}");
    }

    [RelayCommand]
    private void OpenTransferFormPanel()
    {
        // Close adjust panel if open
        IsAdjustProductPanelOpen = false;
        
        IsTransferFormPanelOpen = true;
        TransferProduct = new TransferProductModel();
    }

    [RelayCommand]
    private void CloseTransferFormPanel()
    {
        IsTransferFormPanelOpen = false;
    }

    [RelayCommand]
    private void SaveTransferProduct()
    {
        // TODO: Implement save transfer logic
        IsTransferFormPanelOpen = false;
    }

    [RelayCommand]
    private void EditTransfer(StockTransferDto transfer)
    {
        // TODO: Implement edit transfer logic
    }

    [RelayCommand]
    private void DeleteTransfer(StockTransferDto transfer)
    {
        // TODO: Implement delete transfer logic
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatus = "All";
        
        // Refresh data after clearing filters
        await LoadStockAdjustmentsAsync();
    }

    // Stock Adjustment Commands
    [RelayCommand]
    private async Task OpenAdjustProductPanel()
    {
        LogMessage("[StockManagementViewModel] OpenAdjustProductPanel called");
        
        // Close transfer panel if open
        IsTransferFormPanelOpen = false;
        
        IsAdjustProductPanelOpen = true;
        AdjustProduct = new AdjustProductModel();
        
        // Subscribe to AdjustProduct property changes for search functionality
        AdjustProduct.PropertyChanged += OnAdjustProductPropertyChanged;
        LogMessage("[StockManagementViewModel] Subscribed to AdjustProduct.PropertyChanged");
        
        // Load adjustment reasons
        await LoadAdjustmentReasonsAsync();
        LogMessage("[StockManagementViewModel] Adjustment reasons loaded");
    }

    [RelayCommand]
    private void CloseAdjustProductPanel()
    {
        LogMessage("[StockManagementViewModel] CloseAdjustProductPanel called");
        
        // Unsubscribe from property changes
        if (AdjustProduct != null)
        {
            AdjustProduct.PropertyChanged -= OnAdjustProductPropertyChanged;
            LogMessage("[StockManagementViewModel] Unsubscribed from AdjustProduct.PropertyChanged");
        }
        
        IsAdjustProductPanelOpen = false;
    }

    [RelayCommand]
    private async Task SaveAdjustProduct()
    {
        try
        {
            DesktopFileLogger.Log("[SaveAdjustProduct] === SAVE BUTTON CLICKED ===");
            DesktopFileLogger.Log("[SaveAdjustProduct] === STARTING SAVE OPERATION ===");
            Console.WriteLine("[SaveAdjustProduct] SAVE BUTTON CLICKED - CONSOLE LOG");
            
            // Check if service is available first
            DesktopFileLogger.Log($"[SaveAdjustProduct] StockAdjustmentService null check: {_stockAdjustmentService == null}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] AdjustProduct null check: {AdjustProduct == null}");
            
            // Validation
            DesktopFileLogger.Log($"[SaveAdjustProduct] Validating form data...");
            DesktopFileLogger.Log($"[SaveAdjustProduct] SelectedProduct: {AdjustProduct.SelectedProduct?.Name ?? "NULL"}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] ProductId: {AdjustProduct.ProductId}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] CurrentStock: {AdjustProduct.CurrentStock}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] NewQuantity: {AdjustProduct.NewQuantity}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] ReasonText: '{AdjustProduct.ReasonText}'");
            DesktopFileLogger.Log($"[SaveAdjustProduct] DifferenceQuantity: {AdjustProduct.DifferenceQuantity}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] ExpiryDate: {AdjustProduct.ExpiryDate}");
            
            if (AdjustProduct.SelectedProduct == null)
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: No product selected");
                MessageBox.Show("Please select a product.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(AdjustProduct.ReasonText))
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: No reason provided");
                MessageBox.Show("Please provide a reason for the adjustment.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AdjustProduct.DifferenceQuantity == 0)
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: No quantity difference");
                MessageBox.Show("No quantity change detected. Please enter a different quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_stockAdjustmentService == null)
            {
                DesktopFileLogger.Log("[SaveAdjustProduct] ERROR: StockAdjustmentService is null");
                MessageBox.Show("Stock adjustment service not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DesktopFileLogger.Log("[SaveAdjustProduct] Validation passed. Creating reason...");

            // First, create or get the reason
            int reasonId;
            try 
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] Calling CreateReasonIfNotExistsAsync with reason: '{AdjustProduct.ReasonText}'");
                reasonId = await _stockAdjustmentService.CreateReasonIfNotExistsAsync(AdjustProduct.ReasonText);
                DesktopFileLogger.Log($"[SaveAdjustProduct] Reason created/found with ID: {reasonId}");
            }
            catch (Exception ex)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] ERROR creating reason: {ex.Message}");
                DesktopFileLogger.Log($"[SaveAdjustProduct] Exception details: {ex}");
                MessageBox.Show($"Error creating adjustment reason: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Create the stock adjustment DTO
            DesktopFileLogger.Log("[SaveAdjustProduct] Creating stock adjustment DTO...");
            
            // Get UOM ID from StockAdjustmentService (let it handle the lookup)
            DesktopFileLogger.Log($"[SaveAdjustProduct] Getting UOM ID for ProductId: {AdjustProduct.SelectedProduct.Id}");
            
            // For now, we'll let the service handle UOM validation and use 1 as default
            // TODO: Add proper UOM lookup based on product configuration
            var productUomId = 1;
            DesktopFileLogger.Log($"[SaveAdjustProduct] Using UomId: {productUomId} (will be validated by service)");
            
            var createDto = new CreateStockAdjustmentDto
            {
                AdjustmentDate = DateTime.Now,
                StoreLocationId = 1, // TODO: Get default store location or let user select
                ReasonId = reasonId,
                Remarks = $"Stock adjustment for {AdjustProduct.SelectedProduct.Name}",
                Items = new List<CreateStockAdjustmentItemDto>
                {
                    new CreateStockAdjustmentItemDto
                    {
                        ProductId = AdjustProduct.SelectedProduct.Id,
                        UomId = productUomId,
                        BatchNo = null,
                        ExpiryDate = AdjustProduct.ExpiryDate,
                        QuantityBefore = AdjustProduct.CurrentStock,
                        QuantityAfter = AdjustProduct.NewQuantity,
                        ReasonLine = AdjustProduct.ReasonText,
                        RemarksLine = $"Adjusted from {AdjustProduct.CurrentStock} to {AdjustProduct.NewQuantity}"
                    }
                }
            };

            DesktopFileLogger.Log($"[SaveAdjustProduct] DTO created:");
            DesktopFileLogger.Log($"  - AdjustmentDate: {createDto.AdjustmentDate}");
            DesktopFileLogger.Log($"  - StoreLocationId: {createDto.StoreLocationId}");
            DesktopFileLogger.Log($"  - ReasonId: {createDto.ReasonId}");
            DesktopFileLogger.Log($"  - Remarks: {createDto.Remarks}");
            DesktopFileLogger.Log($"  - Items Count: {createDto.Items.Count}");
            if (createDto.Items.Any())
            {
                var item = createDto.Items.First();
                DesktopFileLogger.Log($"  - Item ProductId: {item.ProductId}");
                DesktopFileLogger.Log($"  - Item QuantityBefore: {item.QuantityBefore}");
                DesktopFileLogger.Log($"  - Item QuantityAfter: {item.QuantityAfter}");
                DesktopFileLogger.Log($"  - Item ReasonLine: {item.ReasonLine}");
            }

            // Save the adjustment
            DesktopFileLogger.Log("[SaveAdjustProduct] Calling CreateStockAdjustmentAsync...");
            var result = await _stockAdjustmentService.CreateStockAdjustmentAsync(createDto);
            DesktopFileLogger.Log($"[SaveAdjustProduct] Service call completed. Result: {result}");

            if (result != null)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] SUCCESS! Adjustment saved with number: {result.AdjustmentNo}");
                MessageBox.Show("Stock adjustment saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reset form and close panel
                DesktopFileLogger.Log("[SaveAdjustProduct] Resetting form and closing panel...");
                AdjustProduct.Reset();
                IsAdjustProductPanelOpen = false;
                
                // Refresh data
                DesktopFileLogger.Log("[SaveAdjustProduct] Refreshing data...");
                await LoadModuleDataAsync();
                DesktopFileLogger.Log("[SaveAdjustProduct] Data refresh completed");
            }
            else
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] ERROR: Service returned null result");
                MessageBox.Show("Failed to save stock adjustment. No records were created.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"[SaveAdjustProduct] FATAL ERROR: {ex.Message}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] Exception type: {ex.GetType().Name}");
            DesktopFileLogger.Log($"[SaveAdjustProduct] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                DesktopFileLogger.Log($"[SaveAdjustProduct] Inner exception: {ex.InnerException.Message}");
            }
            MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            DesktopFileLogger.Log("[SaveAdjustProduct] === SAVE OPERATION COMPLETED ===");
        }
    }

    /// <summary>
    /// Handles property changes in the AdjustProduct model, particularly for search functionality
    /// </summary>
    private async void OnAdjustProductPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        DesktopFileLogger.LogSeparator($"PROPERTY CHANGED: {e.PropertyName}");
        DesktopFileLogger.Log($"Property: {e.PropertyName}");
        DesktopFileLogger.Log($"Current SearchText: '{AdjustProduct.SearchText}'");
        DesktopFileLogger.Log($"Current SelectedProduct: {AdjustProduct.SelectedProduct?.Name ?? "NULL"} (ID: {AdjustProduct.SelectedProduct?.Id ?? 0})");
        DesktopFileLogger.Log($"Last Selected Product: {_lastSelectedProduct?.Name ?? "NULL"} (ID: {_lastSelectedProduct?.Id ?? 0})");
        DesktopFileLogger.Log($"IsUpdatingFromSelection flag: {_isUpdatingFromSelection}");
        
        if (e.PropertyName == nameof(AdjustProductModel.SearchText))
        {
            DesktopFileLogger.Log("=== SEARCHTEXT CHANGED ===");
            
            // Don't trigger search if we're updating from a selection
            if (_isUpdatingFromSelection)
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: Updating from selection");
                return;
            }
            
            // Only search if user is actually typing (not selecting)
            var currentText = AdjustProduct.SearchText ?? string.Empty;
            var selectedProductName = AdjustProduct.SelectedProduct?.Name ?? string.Empty;
            
            DesktopFileLogger.Log($"Comparing: '{currentText}' vs '{selectedProductName}'");
            
            // If the search text matches the selected product exactly, don't search
            if (!string.IsNullOrEmpty(selectedProductName) && currentText.Equals(selectedProductName, StringComparison.OrdinalIgnoreCase))
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: SearchText matches selected product name");
                return;
            }
            
            _pendingSearchTerm = currentText;
            DesktopFileLogger.Log($"TRIGGERING SEARCH for: '{_pendingSearchTerm}'");
            
            // Cancel previous search timer
            _searchDebounceTimer?.Stop();
            
            // Start new search timer with 300ms delay
            _searchDebounceTimer?.Start();
            DesktopFileLogger.Log("Search timer started");
        }
        else if (e.PropertyName == nameof(AdjustProductModel.SelectedProduct))
        {
            DesktopFileLogger.Log("=== SELECTEDPRODUCT CHANGED ===");
            
            var currentProduct = AdjustProduct.SelectedProduct;
            var isActualChange = currentProduct?.Id != _lastSelectedProduct?.Id;
            
            DesktopFileLogger.Log($"Is actual product change: {isActualChange}");
            
            if (isActualChange)
            {
                DesktopFileLogger.Log($"PRODUCT SELECTION CHANGED: {_lastSelectedProduct?.Name ?? "NULL"} → {currentProduct?.Name ?? "NULL"}");
                
                // Update search text with selected product name
                if (currentProduct != null)
                {
                    DesktopFileLogger.Log($"Updating SearchText to match selected product: '{currentProduct.Name}'");
                    
                    _isUpdatingFromSelection = true;
                    AdjustProduct.SearchText = currentProduct.Name;
                    _isUpdatingFromSelection = false;
                    
                    DesktopFileLogger.Log($"SearchText updated successfully");
                }
                
                // Update last selected product
                _lastSelectedProduct = currentProduct;
                
                // Load stock for the selected product
                DesktopFileLogger.Log("Loading stock for selected product...");
                await LoadCurrentStockForSelectedProduct();
            }
            else
            {
                DesktopFileLogger.Log("IGNORING: Same product selected again");
            }
        }
        
        DesktopFileLogger.LogSeparator("END PROPERTY CHANGE");
    }

    [RelayCommand]
    private void SearchProduct()
    {
        // TODO: Implement product search logic
    }

    // New Enhanced Commands for Stock Adjustment
    [RelayCommand]
    private void Keypad(string input)
    {
        switch (input.ToLower())
        {
            case "clear":
                AdjustProduct.NewQuantity = 0;
                break;
            case "backspace":
                var currentText = AdjustProduct.NewQuantity.ToString();
                if (currentText.Length > 0)
                {
                    currentText = currentText[..^1];
                    if (decimal.TryParse(currentText, out var newValue))
                        AdjustProduct.NewQuantity = newValue;
                    else
                        AdjustProduct.NewQuantity = 0;
                }
                break;
            case "enter":
                // Focus next field or save
                break;
            case "+":
                AdjustProduct.NewQuantity = AdjustProduct.CurrentStock + 1;
                break;
            case "-":
                AdjustProduct.NewQuantity = Math.Max(0, AdjustProduct.CurrentStock - 1);
                break;
            case ".":
                // Handle decimal point
                break;
            default:
                if (int.TryParse(input, out var digit))
                {
                    var currentValue = AdjustProduct.NewQuantity;
                    var newValue = currentValue * 10 + digit;
                    AdjustProduct.NewQuantity = newValue;
                }
                break;
        }
    }

    [RelayCommand]
    private async Task SaveStockAdjustment()
    {
        if (!AdjustProduct.IsValid)
        {
            // Show validation error
            return;
        }

        try
        {
            // Create stock adjustment DTO
            var adjustmentDto = new CreateStockAdjustmentDto
            {
                AdjustmentDate = DateTime.Now,
                StoreLocationId = 1, // TODO: Get from selected store
                ReasonId = AdjustProduct.ReasonId ?? 1, // Default to 1 if null
                Notes = $"Stock adjustment for {AdjustProduct.ProductName}",
                Items = new List<CreateStockAdjustmentItemDto>
                {
                    new CreateStockAdjustmentItemDto
                    {
                        ProductId = AdjustProduct.ProductId,
                        UomId = 1, // TODO: Get from product
                        BatchNo = null,
                        ExpiryDate = AdjustProduct.ExpiryDate,
                        QuantityBefore = AdjustProduct.CurrentStock,
                        QuantityAfter = AdjustProduct.NewQuantity,
                        ReasonLine = "Stock adjustment via touch interface"
                    }
                }
            };

            // Save via service
            if (_stockAdjustmentService != null)
            {
                var result = await _stockAdjustmentService.CreateStockAdjustmentAsync(adjustmentDto);
                
                // Show success message
                MessageBox.Show($"Stock adjustment saved successfully!\nAdjustment No: {result.AdjustmentNo}", 
                               "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reset form and close panel
                AdjustProduct.Reset();
                CloseAdjustProductPanel();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving stock adjustment: {ex.Message}", 
                           "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Constructor

    public StockManagementViewModel(
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        IProductService? productService = null,
        IStockAdjustmentService? stockAdjustmentService = null)
    {
        LogMessage("[StockManagementViewModel] Constructor called");
        LogMessage($"[StockManagementViewModel] ProductService is null: {productService == null}");
        LogMessage($"[StockManagementViewModel] StockAdjustmentService is null: {stockAdjustmentService == null}");
        
        _themeService = themeService;
        _zoomService = zoomService;
        _localizationService = localizationService;
        _colorSchemeService = colorSchemeService;
        _layoutDirectionService = layoutDirectionService;
        _fontService = fontService;
        _databaseLocalizationService = databaseLocalizationService;
        _productService = productService;
        _stockAdjustmentService = stockAdjustmentService;

        // Initialize search debouncing timer
        _searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300) // 300ms debounce
        };
        _searchDebounceTimer.Tick += async (sender, e) =>
        {
            _searchDebounceTimer.Stop();
            
            // Check if this is for product search (in adjust panel) or stock adjustments table search
            if (IsAdjustProductPanelOpen && !string.IsNullOrEmpty(_pendingSearchTerm))
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing product search for: '{_pendingSearchTerm}'");
                await PerformSearchAsync(_pendingSearchTerm);
            }
            else
            {
                LogMessage($"[StockManagementViewModel] Timer tick - performing stock adjustments search for: '{SearchText}'");
                await LoadStockAdjustmentsAsync();
            }
        };

        // Subscribe to settings changes
        _themeService.ThemeChanged += OnThemeChanged;
        _zoomService.ZoomChanged += OnZoomChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _colorSchemeService.PrimaryColorChanged += OnPrimaryColorChanged;
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;

        // Initialize with current settings
        InitializeSettings();
        
        // Setup product search event handler
        AdjustProduct.PropertyChanged += OnAdjustProductPropertyChanged;
        
        // Setup search text change handler for stock adjustments table
        PropertyChanged += OnViewModelPropertyChanged;
        
        // Initialize modules immediately for instant UI display
        InitializeModulesSync();
        
        // Load async data in background
        _ = LoadAsyncDataInBackground();
    }

    /// <summary>
    /// Handle property changes in the main ViewModel (e.g., SearchText)
    /// </summary>
    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            LogMessage($"[SearchText Changed] New value: '{SearchText}'");
            
            // Cancel previous search timer
            _searchDebounceTimer?.Stop();
            
            // Start new search timer with 300ms delay
            _searchDebounceTimer?.Start();
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize modules synchronously for immediate UI display
    /// </summary>
    private void InitializeModulesSync()
    {
        var modules = new List<StockModuleInfo>
        {
            new StockModuleInfo
            {
                Title = "Stock Adjustment", // Default text, will be updated by async localization
                ModuleType = "StockAdjustment",
                ItemCount = 125,
                ItemCountLabel = "Items",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsStockAdjustmentSelected
            },
            new StockModuleInfo
            {
                Title = "Stock Transfer", // Default text, will be updated by async localization
                ModuleType = "StockTransfer", 
                ItemCount = 32,
                ItemCountLabel = "Transfers",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsStockTransferSelected
            },
            new StockModuleInfo
            {
                Title = "Goods Received", // Default text, will be updated by async localization
                ModuleType = "GoodsReceived",
                ItemCount = 67,
                ItemCountLabel = "Receipts",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsGoodsReceivedSelected
            },
            new StockModuleInfo
            {
                Title = "Goods Return", // Default text, will be updated by async localization
                ModuleType = "GoodsReturn",
                ItemCount = 15,
                ItemCountLabel = "Returns",
                IconBackground = GetPrimaryColorBrush(),
                ButtonBackground = GetButtonBackgroundBrush(),
                IsSelected = IsGoodsReturnSelected
            }
        };

        Modules.Clear();
        foreach (var module in modules)
        {
            Modules.Add(module);
        }
    }

    /// <summary>
    /// Load async data in background without affecting UI initialization
    /// </summary>
    private async Task LoadAsyncDataInBackground()
    {
        try
        {
            // Load localized titles in background
            await UpdateModuleTitlesAsync();
            
            // Load adjustment reasons
            await LoadAdjustmentReasonsAsync();
            
            // Load stock adjustments data
            await LoadStockAdjustmentsAsync();
            
            // Load stock transfers data
            await LoadStockTransfersAsync();
        }
        catch (Exception ex)
        {
            LogMessage($"Error loading background data: {ex.Message}");
        }
    }

    /// <summary>
    /// Update module titles with localized text
    /// </summary>
    private async Task UpdateModuleTitlesAsync()
    {
        if (Modules.Count >= 4)
        {
            Modules[0].Title = await _databaseLocalizationService.GetTranslationAsync("stock.adjustment");
            Modules[1].Title = await _databaseLocalizationService.GetTranslationAsync("stock.transfer");
            Modules[2].Title = await _databaseLocalizationService.GetTranslationAsync("stock.goods_received");
            Modules[3].Title = await _databaseLocalizationService.GetTranslationAsync("stock.goods_return");
        }
    }

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
    /// Load stock module data (now just calls the initialization methods)
    /// </summary>
    private async Task LoadModuleDataAsync()
    {
        // Refresh modules with latest data
        InitializeModulesSync();
        
        // Update with localized titles
        await UpdateModuleTitlesAsync();
        
        // Load adjustment reasons
        await LoadAdjustmentReasonsAsync();
        
        // Load stock adjustments
        await LoadStockAdjustmentsAsync();
        
        // Load stock transfers
        await LoadStockTransfersAsync();
    }

    /// <summary>
    /// Load stock adjustment reasons from database
    /// </summary>
    private async Task LoadAdjustmentReasonsAsync()
    {
        try
        {
            if (_stockAdjustmentService != null)
            {
                var reasons = await _stockAdjustmentService.GetAdjustmentReasonsAsync();
                AdjustmentReasons.Clear();
                foreach (var reason in reasons)
                {
                    AdjustmentReasons.Add(reason);
                }
            }
            else
            {
                // Add default reasons if service is not available
                AdjustmentReasons.Clear();
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 1, Name = "Stock count correction" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 2, Name = "Damaged items" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 3, Name = "Expired products" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 4, Name = "Theft/Loss" });
                AdjustmentReasons.Add(new StockAdjustmentReasonDto { Id = 5, Name = "New stock addition" });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading adjustment reasons: {ex.Message}");
        }
    }

    /// <summary>
    /// Perform the actual product search with database integration
    /// </summary>
    private async Task PerformSearchAsync(string searchTerm)
    {
        try
        {
            DesktopFileLogger.LogSeparator("SEARCH START");
            DesktopFileLogger.Log($"Search term: '{searchTerm}'");
            DesktopFileLogger.Log($"ProductService is null: {_productService == null}");
            DesktopFileLogger.Log($"Current SelectedProduct: {AdjustProduct.SelectedProduct?.Name ?? "NULL"}");
            DesktopFileLogger.Log($"SearchResults count before: {SearchResults.Count}");
            
            // Don't clear results if we have a selected product and the search term matches
            if (AdjustProduct.SelectedProduct != null && 
                !string.IsNullOrEmpty(AdjustProduct.SelectedProduct.Name) &&
                searchTerm.Equals(AdjustProduct.SelectedProduct.Name, StringComparison.OrdinalIgnoreCase))
            {
                DesktopFileLogger.Log("SKIPPING SEARCH: Search term matches selected product, keeping current results");
                return;
            }
            
            DesktopFileLogger.Log("CLEARING SearchResults...");
            SearchResults.Clear();
            DesktopFileLogger.Log($"SearchResults cleared, count: {SearchResults.Count}");
            
            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 2)
            {
                LogMessage("Search term too short, returning empty results");
                return;
            }

            if (_productService != null)
            {
                LogMessage("Calling _productService.SearchProductsAsync...");
                
                // Use the same logic as ProductManagementViewModel
                var products = await _productService.SearchProductsAsync(searchTerm);
                
                LogMessage($"Raw products returned: {products?.Count() ?? 0}");
                
                if (products != null)
                {
                    var productList = products.ToList();
                    DesktopFileLogger.Log($"Products list count: {productList.Count}");
                    
                    foreach (var product in productList)
                    {
                        DesktopFileLogger.Log($"Product found: {product.Name} (ID: {product.Id})");
                    }
                    
                    foreach (var product in productList.Take(10)) // Limit to 10 results for dropdown
                    {
                        SearchResults.Add(product);
                        DesktopFileLogger.Log($"Added to SearchResults: {product.Name}");
                    }
                }
                
                DesktopFileLogger.Log($"Final SearchResults count: {SearchResults.Count}");
                
                // Force UI update
                OnPropertyChanged(nameof(SearchResults));
                DesktopFileLogger.Log("UI PropertyChanged event fired for SearchResults");
            }
            else
            {
                DesktopFileLogger.Log("ERROR: ProductService is null!");
            }
            
            DesktopFileLogger.LogSeparator("SEARCH END");
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"SEARCH ERROR: {ex.Message}");
            DesktopFileLogger.Log($"Stack trace: {ex.StackTrace}");
            SearchResults.Clear();
        }
    }

    /// <summary>
    /// Load current stock level for selected product
    /// </summary>
    private async Task LoadCurrentStockForSelectedProduct()
    {
        try
        {
            DesktopFileLogger.LogSeparator("LOAD STOCK");
            DesktopFileLogger.Log($"Selected Product: {AdjustProduct.SelectedProduct?.Name ?? "NULL"}");
            
            if (AdjustProduct.SelectedProduct != null)
            {
                DesktopFileLogger.Log($"Loading stock for product: {AdjustProduct.SelectedProduct.Name} (ID: {AdjustProduct.SelectedProduct.Id})");
                
                // TODO: Load actual stock level from database
                // For now, simulate with product stock quantity
                var currentStock = (decimal)AdjustProduct.SelectedProduct.StockQuantity;
                AdjustProduct.CurrentStock = currentStock;
                AdjustProduct.NewQuantity = currentStock; // Initialize with current stock
                
                DesktopFileLogger.Log($"Current Stock set to: {currentStock}");
                DesktopFileLogger.Log($"New Quantity initialized to: {currentStock}");
            }
            else
            {
                DesktopFileLogger.Log("No product selected - skipping stock load");
            }
            
            DesktopFileLogger.LogSeparator("LOAD STOCK END");
        }
        catch (Exception ex)
        {
            DesktopFileLogger.Log($"ERROR loading current stock: {ex.Message}");
            DesktopFileLogger.Log($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Load stock transfers data
    /// </summary>
    private async Task LoadStockTransfersAsync()
    {
        // Removed artificial delay for immediate UI loading
        
        // TODO: Replace with actual data loading from service
        var transfers = new List<StockTransferDto>
        {
            new StockTransferDto
            {
                TransferId = 1,
                TransferNo = "ST00001",
                TransferDate = DateTime.Today.AddDays(-5),
                FromStoreName = "Main Warehouse",
                ToStoreName = "Retail Store A",
                Status = "Completed",
                TotalItems = 5,
                CreatedByName = "John Doe",
                CreatedAt = DateTime.Today.AddDays(-5),
                Remarks = "Weekly stock replenishment"
            },
            new StockTransferDto
            {
                TransferId = 2,
                TransferNo = "ST00002",
                TransferDate = DateTime.Today.AddDays(-3),
                FromStoreName = "Retail Store A",
                ToStoreName = "Retail Store B",
                Status = "Pending",
                TotalItems = 3,
                CreatedByName = "Jane Smith",
                CreatedAt = DateTime.Today.AddDays(-3),
                Remarks = "Product redistribution"
            },
            new StockTransferDto
            {
                TransferId = 3,
                TransferNo = "ST00003",
                TransferDate = DateTime.Today.AddDays(-1),
                FromStoreName = "Main Warehouse",
                ToStoreName = "Retail Store C",
                Status = "In-Transit",
                TotalItems = 8,
                CreatedByName = "Mike Johnson",
                CreatedAt = DateTime.Today.AddDays(-1),
                Remarks = "New product launch stock"
            }
        };

        StockTransfers.Clear();
        foreach (var transfer in transfers)
        {
            StockTransfers.Add(transfer);
        }
    }

    /// <summary>
    /// Load stock adjustments data with financial calculations
    /// </summary>
    private async Task LoadStockAdjustmentsAsync()
    {
        try
        {
            if (_stockAdjustmentService != null)
            {
                // Load actual data from service with search filter
                var searchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
                var result = await _stockAdjustmentService.GetStockAdjustmentsAsync(1, 100, searchTerm);
                
                StockAdjustments.Clear();
                StockAdjustmentItems.Clear();
                
                foreach (var adjustment in result.Items)
                {
                    StockAdjustments.Add(adjustment);
                    
                    // Get detailed adjustment with items
                    var detailedAdjustment = await _stockAdjustmentService.GetStockAdjustmentByIdAsync(adjustment.AdjustmentId);
                    if (detailedAdjustment?.Items != null)
                    {
                        foreach (var item in detailedAdjustment.Items)
                        {
                            // Add adjustment info to each item for the flattened view
                            item.AdjustmentId = detailedAdjustment.AdjustmentId;
                            // Add to flattened collection
                            StockAdjustmentItems.Add(item);
                        }
                    }
                }
            }
            else
            {
                // Fallback with sample data for testing
                StockAdjustmentItems.Clear();
                
                var sampleItems = new List<StockAdjustmentItemDto>
                {
                    new StockAdjustmentItemDto
                    {
                        Id = 1,
                        AdjustmentId = 1,
                        ProductId = 1,
                        ProductName = "Cheese Burger",
                        ProductSku = "CHB001",
                        UomName = "PCS",
                        QuantityBefore = 50,
                        QuantityAfter = 45,
                        DifferenceQty = -5,
                        CostPrice = 5.50m,
                        TaxRate = 0.10m, // 10% tax
                        ReasonLine = "Stock Count Adjustment",
                        RemarksLine = "Monthly inventory count",
                        AdjustmentNo = "ADJ202509080001",
                        AdjustmentDate = DateTime.Today.AddDays(-1)
                    },
                    new StockAdjustmentItemDto
                    {
                        Id = 2,
                        AdjustmentId = 2,
                        ProductId = 2,
                        ProductName = "French Fries",
                        ProductSku = "FF001",
                        UomName = "PCS",
                        QuantityBefore = 30,
                        QuantityAfter = 35,
                        DifferenceQty = 5,
                        CostPrice = 3.25m,
                        TaxRate = 0.10m, // 10% tax
                        ReasonLine = "Supplier Delivery",
                        RemarksLine = "Weekly stock replenishment",
                        AdjustmentNo = "ADJ202509080002",
                        AdjustmentDate = DateTime.Today
                    }
                };
                
                foreach (var item in sampleItems)
                {
                    StockAdjustmentItems.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"[LoadStockAdjustmentsAsync] ERROR: {ex.Message}");
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
    private bool _isSelected = false;

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

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
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

/// <summary>
/// Model for transfer product form
/// </summary>
public class TransferProductModel : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
