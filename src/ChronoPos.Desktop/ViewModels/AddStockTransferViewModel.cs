using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using System.Globalization;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for adding new Stock Transfer with comprehensive form validation and full settings integration
/// </summary>
public partial class AddStockTransferViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IStockTransferService _stockTransferService;
    private readonly IStockTransferItemService _stockTransferItemService;
    private readonly IStoreService _storeService;
    private readonly IProductService _productService;
    private readonly IUomService _uomService;
    private readonly IProductBatchService _productBatchService;
    internal readonly IStockService _stockService;
    private readonly Action? _navigateBack;
    private readonly int? _transferId;
    
    // Settings services
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    
    #endregion

    #region Observable Properties

    // Stock Transfer Header Properties
    [ObservableProperty]
    private string transferNo = string.Empty;

    [ObservableProperty]
    private string status = "Pending";

    [ObservableProperty]
    private int? fromStoreId;

    [ObservableProperty]
    private int? toStoreId;

    [ObservableProperty]
    private DateTime transferDate = DateTime.Today;

    [ObservableProperty]
    private string remarks = string.Empty;

    // Collections
    [ObservableProperty]
    private ObservableCollection<StoreDto> stores = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> unitsOfMeasurement = new();

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    // Stock Transfer Items
    [ObservableProperty]
    private ObservableCollection<StockTransferItemViewModel> transferItems = new();

    [ObservableProperty]
    private StockTransferItemViewModel? selectedTransferItem;

    // Summary Properties
    [ObservableProperty]
    private int totalItems = 0;

    [ObservableProperty]
    private decimal totalQuantity = 0;

    // UI State Properties
    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationErrors = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    // Settings Properties
    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    #endregion

    #region Text Properties for Localization

    [ObservableProperty]
    private string addTransferTitle = "Add Stock Transfer";

    [ObservableProperty]
    private string backButtonText = "Back";

    [ObservableProperty]
    private string transferHeaderTitle = "Transfer Header";

    [ObservableProperty]
    private string transferItemsTitle = "Transfer Items";

    [ObservableProperty]
    private string summaryTitle = "Summary";

    [ObservableProperty]
    private string transferNoLabel = "Transfer No";

    [ObservableProperty]
    private string statusLabel = "Status";

    [ObservableProperty]
    private string fromStoreLabel = "From Store";

    [ObservableProperty]
    private string toStoreLabel = "To Store";

    [ObservableProperty]
    private string transferDateLabel = "Transfer Date";

    [ObservableProperty]
    private string remarksLabel = "Remarks";

    [ObservableProperty]
    private string addProductButtonText = "Add Product";

    [ObservableProperty]
    private string noItemsMessage = "No items have been added to this transfer";

    /// <summary>
    /// Indicates if this is in edit mode (true) or add mode (false)
    /// </summary>
    public bool IsEditMode => _transferId.HasValue;

    [ObservableProperty]
    private string totalItemsLabel = "Total Items";

    [ObservableProperty]
    private string totalQuantityLabel = "Total Quantity";

    [ObservableProperty]
    private string cancelButtonText = "Cancel";
    [ObservableProperty]
    private string saveDraftButtonText = "Save as Draft";
    [ObservableProperty]
    private string submitForApprovalButtonText = "Submit for Approval";
    [ObservableProperty]
    private string createTransferButtonText = "Complete Transfer";

    #endregion

    #region Constructor

    public AddStockTransferViewModel(
        IStockTransferService stockTransferService,
        IStockTransferItemService stockTransferItemService,
        IStoreService storeService,
        IProductService productService,
        IUomService uomService,
        IProductBatchService productBatchService,
        IStockService stockService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action? navigateBack = null,
        int? transferId = null)
    {
        _stockTransferService = stockTransferService ?? throw new ArgumentNullException(nameof(stockTransferService));
        _stockTransferItemService = stockTransferItemService ?? throw new ArgumentNullException(nameof(stockTransferItemService));
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _navigateBack = navigateBack;
        _transferId = transferId;

        // Subscribe to settings changes
        SubscribeToSettingsChanges();

        // Subscribe to transfer items collection changes
        TransferItems.CollectionChanged += TransferItems_CollectionChanged;

        // Initialize data
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize the view model with required data
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = await GetTranslationAsync("common.loading", "Loading data...");

            // Load reference data first
            await Task.WhenAll(
                LoadStoresAsync(),
                LoadProductsAsync(),
                LoadUnitsOfMeasurementAsync(),
                LoadLocalizedTextAsync()
            );
            
            // Handle edit mode or add mode
            if (IsEditMode)
            {
                // Load existing transfer data for editing
                await LoadTransferForEditingAsync(_transferId!.Value);
            }
            else
            {
                // Generate new transfer number for add mode
                TransferNo = await _stockTransferService.GenerateTransferNumberAsync();
            }
            
            // Apply current settings
            await ApplyCurrentSettingsAsync();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error initializing Add Stock Transfer: {ex.Message}");
            ShowValidationError(await GetTranslationAsync("common.initialization_error", "Failed to initialize the form"));
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    /// <summary>
    /// Load available stores
    /// </summary>
    private async Task LoadStoresAsync()
    {
        try
        {
            AppLogger.LogInfo("Starting to load stores...");
            var storesList = await _storeService.GetAllAsync();
            AppLogger.LogInfo($"Retrieved {storesList.Count()} stores from service");
            
            Stores.Clear();
            foreach (var store in storesList)
            {
                Stores.Add(store);
                AppLogger.LogInfo($"Added store: ID={store.Id}, Name={store.Name}");
            }
            
            AppLogger.LogInfo($"Total stores in collection: {Stores.Count}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading stores: {ex.Message}");
        }
    }

    /// <summary>
    /// Load available products
    /// </summary>
    private async Task LoadProductsAsync()
    {
        try
        {
            var productsList = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in productsList)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading products: {ex.Message}");
        }
    }

    /// <summary>
    /// Load units of measurement
    /// </summary>
    private async Task LoadUnitsOfMeasurementAsync()
    {
        try
        {
            var uomList = await _uomService.GetAllUomsAsync();
            UnitsOfMeasurement.Clear();
            foreach (var uom in uomList)
            {
                UnitsOfMeasurement.Add(uom);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading units of measurement: {ex.Message}");
        }
    }

    /// <summary>
    /// Load product batches for a specific product
    /// </summary>
    private async Task LoadProductBatchesAsync(int productId)
    {
        try
        {
            var batches = await _productBatchService.GetProductBatchesByProductIdAsync(productId);
            ProductBatches.Clear();
            foreach (var batch in batches)
            {
                ProductBatches.Add(batch);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading product batches: {ex.Message}");
        }
    }

    /// <summary>
    /// Load product batches for a specific transfer item
    /// </summary>
    private async Task LoadProductBatchesForItemAsync(int productId, StockTransferItemViewModel item)
    {
        try
        {
            var batches = await _productBatchService.GetProductBatchesByProductIdAsync(productId);
            item.ProductBatches.Clear();
            foreach (var batch in batches)
            {
                item.ProductBatches.Add(batch);
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading product batches for item: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle from store selection change - update stock for all items
    /// </summary>
    partial void OnFromStoreIdChanged(int? value)
    {
        if (value.HasValue)
        {
            // Update stock for all existing items
            foreach (var item in TransferItems)
            {
                if (item.ProductId.HasValue)
                {
                    item.UpdateAvailableStock();
                }
            }
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void Cancel()
    {
        _navigateBack?.Invoke();
    }

    [RelayCommand]
    private void AddTransferItem()
    {
        try
        {
            var newItem = new StockTransferItemViewModel(this);
            TransferItems.Add(newItem);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error adding transfer item: {ex.Message}");
            ShowValidationError("Failed to add new item");
        }
    }

    [RelayCommand]
    private void RemoveTransferItem(StockTransferItemViewModel item)
    {
        try
        {
            if (item != null && TransferItems.Contains(item))
            {
                TransferItems.Remove(item);
                CalculateTotals();
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error removing transfer item: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveDraft()
    {
        AppLogger.LogInfo("Starting stock transfer draft save", filename: "stock_transfer");
        
        try
        {
            AppLogger.LogInfo("Validating form data for draft", filename: "stock_transfer");
            if (!ValidateForm())
            {
                AppLogger.LogWarning("Form validation failed for draft", filename: "stock_transfer");
                return;
            }

            IsLoading = true;
            StatusMessage = IsEditMode ? "Updating draft..." : "Saving draft...";
            
            if (IsEditMode && _transferId.HasValue)
            {
                // Update existing transfer as draft
                var updateDto = BuildCreateStockTransferDto("Draft");
                AppLogger.LogInfo($"Updating transfer draft with {updateDto.Items?.Count ?? 0} items", 
                    $"Transfer ID: {_transferId.Value}, Transfer No: {TransferNo}", "stock_transfer");
                
                var result = await _stockTransferService.UpdateStockTransferAsync(_transferId.Value, updateDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"Transfer draft updated successfully", 
                        $"Transfer No: {result.TransferNo}, Status: Draft", "stock_transfer");
                    new MessageDialog(
                        "Success",
                        "Transfer saved as draft successfully!",
                        MessageDialog.MessageType.Success
                    ).ShowDialog();
                    _navigateBack?.Invoke();
                }
            }
            else
            {
                // Create new transfer as draft
                var createDto = BuildCreateStockTransferDto("Draft");
                AppLogger.LogInfo($"Creating transfer draft with {createDto.Items?.Count ?? 0} items", filename: "stock_transfer");
                
                var result = await _stockTransferService.CreateStockTransferAsync(createDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"Transfer draft created successfully", 
                        $"Transfer No: {result.TransferNo}, Status: Draft", "stock_transfer");
                    new MessageDialog(
                        "Success",
                        "Transfer saved as draft successfully!",
                        MessageDialog.MessageType.Success
                    ).ShowDialog();
                    _navigateBack?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to save transfer draft", ex, 
                $"Items: {TransferItems.Count}, FromStore: {FromStoreId}, ToStore: {ToStoreId}", "stock_transfer");
            ShowValidationError("Failed to save transfer draft");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }



    [RelayCommand]
    private async Task SubmitForApproval()
    {
        AppLogger.LogInfo("Starting stock transfer submission for approval", filename: "stock_transfer");
        
        try
        {
            AppLogger.LogInfo("Validating form data for approval", filename: "stock_transfer");
            if (!ValidateForm())
            {
                AppLogger.LogWarning("Form validation failed for approval", filename: "stock_transfer");
                return;
            }

            IsLoading = true;
            StatusMessage = IsEditMode ? "Updating for approval..." : "Submitting for approval...";
            
            if (IsEditMode && _transferId.HasValue)
            {
                // Update existing transfer for approval
                var updateDto = BuildCreateStockTransferDto("Pending");
                AppLogger.LogInfo($"Updating transfer for approval with {updateDto.Items?.Count ?? 0} items", 
                    $"Transfer ID: {_transferId.Value}, Transfer No: {TransferNo}", "stock_transfer");
                
                var result = await _stockTransferService.UpdateStockTransferAsync(_transferId.Value, updateDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"Transfer submitted for approval successfully", 
                        $"Transfer No: {result.TransferNo}, Status: Pending", "stock_transfer");
                    new MessageDialog(
                        "Success",
                        "Transfer submitted for approval successfully!",
                        MessageDialog.MessageType.Success
                    ).ShowDialog();
                    _navigateBack?.Invoke();
                }
            }
            else
            {
                // Create new transfer for approval
                var createDto = BuildCreateStockTransferDto("Pending");
                AppLogger.LogInfo($"Creating transfer for approval with {createDto.Items?.Count ?? 0} items", filename: "stock_transfer");
                
                var result = await _stockTransferService.CreateStockTransferAsync(createDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"Transfer submitted for approval successfully", 
                        $"Transfer No: {result.TransferNo}, Status: Pending", "stock_transfer");
                    new MessageDialog(
                        "Success",
                        "Transfer submitted for approval successfully!",
                        MessageDialog.MessageType.Success
                    ).ShowDialog();
                    _navigateBack?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to submit transfer for approval", ex, 
                $"Items: {TransferItems.Count}, FromStore: {FromStoreId}, ToStore: {ToStoreId}", "stock_transfer");
            ShowValidationError("Failed to submit transfer for approval");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task CreateTransfer()
    {
        AppLogger.LogInfo("Starting stock transfer completion process", filename: "stock_transfer");
        
        try
        {
            AppLogger.LogInfo("Validating form data", filename: "stock_transfer");
            if (!ValidateForm())
            {
                AppLogger.LogWarning("Form validation failed", filename: "stock_transfer");
                return;
            }

            IsLoading = true;
            StatusMessage = "Completing transfer...";
            
            AppLogger.LogInfo("Building CreateStockTransferDto", filename: "stock_transfer");
            var createDto = BuildCreateStockTransferDto("Completed");
            
            // Log the DTO details for debugging
            AppLogger.LogInfo($"Transfer DTO created - FromStore: {createDto.FromStoreId}, ToStore: {createDto.ToStoreId}, Items: {createDto.Items?.Count ?? 0}", 
                filename: "stock_transfer");
                
            // Log each item for debugging
            if (createDto.Items != null)
            {
                for (int i = 0; i < createDto.Items.Count; i++)
                {
                    var item = createDto.Items[i];
                    AppLogger.LogInfo($"Item {i + 1}: ProductId={item.ProductId}, UomId={item.UomId}, Quantity={item.QuantitySent}, Batch={item.BatchNo}", 
                        filename: "stock_transfer");
                }
            }

            AppLogger.LogInfo("Calling stock transfer service CreateStockTransferAsync", filename: "stock_transfer");
            var createdTransfer = await _stockTransferService.CreateStockTransferAsync(createDto);
            
            AppLogger.LogInfo($"Stock transfer created successfully with ID: {createdTransfer?.TransferId}", filename: "stock_transfer");

            new MessageDialog(
                "Success",
                "Stock transfer completed successfully! Stock has been reduced.",
                MessageDialog.MessageType.Success
            ).ShowDialog();

            _navigateBack?.Invoke();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error creating stock transfer", ex, $"FromStore: {FromStoreId}, ToStore: {ToStoreId}, Items: {TransferItems.Count}", "stock_transfer");
            
            // Show specific error message for insufficient batch stock
            string errorMessage = "Failed to create transfer";
            if (ex.Message.Contains("Insufficient stock in batch"))
            {
                errorMessage = ex.Message; // Show the detailed batch stock error
            }
            
            ShowValidationError(errorMessage);
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Build CreateStockTransferDto from form data
    /// </summary>
    private CreateStockTransferDto BuildCreateStockTransferDto(string status = "Pending")
    {
        AppLogger.LogInfo($"Building DTO with FromStoreId: {FromStoreId}, ToStoreId: {ToStoreId}, TransferItems count: {TransferItems.Count}, Status: {status}", filename: "stock_transfer");
        
        var dto = new CreateStockTransferDto
        {
            TransferDate = TransferDate,
            FromStoreId = FromStoreId ?? 0,
            ToStoreId = ToStoreId ?? 0,
            Status = status,
            Remarks = Remarks,
            Items = TransferItems.Select(item => new CreateStockTransferItemDto
            {
                ProductId = item.ProductId ?? 0,
                UomId = item.UomId ?? 0,
                BatchNo = item.BatchNo,
                ExpiryDate = item.ExpiryDate,
                QuantitySent = item.Quantity,
                RemarksLine = item.Remarks
            }).ToList()
        };
        
        AppLogger.LogInfo($"DTO built successfully with {dto.Items.Count} items", filename: "stock_transfer");
        return dto;
    }

    /// <summary>
    /// Validate the form data
    /// </summary>
    private bool ValidateForm()
    {
        AppLogger.LogInfo($"Starting validation - FromStore: {FromStoreId}, ToStore: {ToStoreId}, Items: {TransferItems.Count}", filename: "stock_transfer");
        
        var validationErrors = new List<string>();

        // Validate header
        if (!FromStoreId.HasValue)
        {
            validationErrors.Add("From Store is required");
            AppLogger.LogWarning("Validation failed: From Store is required", filename: "stock_transfer");
        }

        if (!ToStoreId.HasValue)
        {
            validationErrors.Add("To Store is required");
            AppLogger.LogWarning("Validation failed: To Store is required", filename: "stock_transfer");
        }

        if (FromStoreId == ToStoreId)
        {
            validationErrors.Add("From Store and To Store cannot be the same");
            AppLogger.LogWarning($"Validation failed: Same store selected for both From ({FromStoreId}) and To ({ToStoreId})", filename: "stock_transfer");
        }

        if (TransferDate > DateTime.Today)
        {
            validationErrors.Add("Transfer date cannot be in the future");
            AppLogger.LogWarning($"Validation failed: Future transfer date {TransferDate:yyyy-MM-dd}", filename: "stock_transfer");
        }

        // Validate items
        if (!TransferItems.Any())
        {
            validationErrors.Add("At least one item is required");
            AppLogger.LogWarning("Validation failed: No transfer items added", filename: "stock_transfer");
        }

        foreach (var item in TransferItems.Select((value, index) => new { Index = index + 1, Item = value }))
        {
            var itemErrors = item.Item.Validate();
            foreach (var error in itemErrors)
            {
                validationErrors.Add($"Row {item.Index}: {error}");
                AppLogger.LogWarning($"Item validation failed for row {item.Index}: {error}", filename: "stock_transfer");
            }
        }

        if (validationErrors.Any())
        {
            HasValidationErrors = true;
            ValidationMessage = string.Join("\n", validationErrors);
            AppLogger.LogError($"Form validation failed with {validationErrors.Count} errors: {string.Join("; ", validationErrors)}", filename: "stock_transfer");
            return false;
        }

        HasValidationErrors = false;
        ValidationMessage = string.Empty;
        AppLogger.LogInfo("Form validation passed successfully", filename: "stock_transfer");
        return true;
    }

    /// <summary>
    /// Show validation error message
    /// </summary>
    private void ShowValidationError(string message)
    {
        HasValidationErrors = true;
        ValidationMessage = message;
    }

    /// <summary>
    /// Calculate totals from transfer items
    /// </summary>
    private void CalculateTotals()
    {
        TotalItems = TransferItems.Count;
        TotalQuantity = TransferItems.Sum(item => item.Quantity);
    }

    /// <summary>
    /// Handle layout direction changes
    /// </summary>
    private void OnLayoutDirectionChanged(object? sender, FlowDirection direction)
    {
        CurrentFlowDirection = direction;
    }

    /// <summary>
    /// Handle transfer items collection changes
    /// </summary>
    private void TransferItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Handle items being added
        if (e.NewItems != null)
        {
            foreach (StockTransferItemViewModel item in e.NewItems)
            {
                item.PropertyChanged += TransferItem_PropertyChanged;
            }
        }

        // Handle items being removed
        if (e.OldItems != null)
        {
            foreach (StockTransferItemViewModel item in e.OldItems)
            {
                item.PropertyChanged -= TransferItem_PropertyChanged;
            }
        }

        CalculateTotals();
    }

    /// <summary>
    /// Handle individual transfer item property changes
    /// </summary>
    private void TransferItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Recalculate totals when quantity changes
        if (e.PropertyName == nameof(StockTransferItemViewModel.Quantity))
        {
            CalculateTotals();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when a product is selected in an item row
    /// </summary>
    public async Task OnProductSelectedAsync(int productId, StockTransferItemViewModel item)
    {
        try
        {
            await LoadProductBatchesForItemAsync(productId, item);
            
            // Update available stock for the selected product
            item.UpdateAvailableStock();
            
            // Auto-select the first UOM if available
            if (UnitsOfMeasurement.Any())
            {
                item.UomId = UnitsOfMeasurement.First().Id;
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error handling product selection: {ex.Message}");
        }
    }

    /// <summary>
    /// Get product batches for the selected product
    /// </summary>
    public ObservableCollection<ProductBatchDto> GetProductBatches() => ProductBatches;

    #endregion

    #region Cleanup

    public void Dispose()
    {
        if (_layoutDirectionService != null)
        {
            // Event unsubscription will be implemented when events are added
        }

        if (TransferItems != null)
        {
            TransferItems.CollectionChanged -= TransferItems_CollectionChanged;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<string> GetTranslationAsync(string key, string fallback)
    {
        try
        {
            var translation = await _databaseLocalizationService.GetTranslationAsync(key);
            return !string.IsNullOrEmpty(translation) && translation != key ? translation : fallback;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error getting translation for key '{key}': {ex.Message}");
            return fallback;
        }
    }

    private async Task LoadLocalizedTextAsync()
    {
        try
        {
            // This method will be implemented when the localized text properties are added
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading localized text: {ex.Message}");
        }
    }

    private async Task ApplyCurrentSettingsAsync()
    {
        try
        {
            // This method will be implemented when the settings properties are added
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error applying current settings: {ex.Message}");
        }
    }

    private void SubscribeToSettingsChanges()
    {
        try
        {
            // This will be implemented when the event handlers are added
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error subscribing to settings changes: {ex.Message}");
        }
    }

    #endregion
    
    /// <summary>
    /// Load existing stock transfer data for editing
    /// </summary>
    private async Task LoadTransferForEditingAsync(int transferId)
    {
        try
        {
            AppLogger.LogInfo("LoadTransferForEditing", $"Loading transfer ID: {transferId}", "stock_transfer");
            
            var transfer = await _stockTransferService.GetStockTransferByIdAsync(transferId);
            if (transfer == null)
            {
                throw new InvalidOperationException($"Stock transfer with ID {transferId} not found");
            }

            // Load transfer header data
            TransferNo = transfer.TransferNo;
            Status = transfer.Status;
            FromStoreId = transfer.FromStoreId;
            ToStoreId = transfer.ToStoreId;
            TransferDate = transfer.TransferDate;
            Remarks = transfer.Remarks ?? string.Empty;

            // Load transfer items
            var items = await _stockTransferItemService.GetByTransferIdAsync(transferId);
            TransferItems.Clear();
            
            foreach (var item in items)
            {
                var itemViewModel = new StockTransferItemViewModel(this)
                {
                    ProductId = item.ProductId,
                    UomId = item.UomId,
                    BatchNo = item.BatchNo,
                    ExpiryDate = item.ExpiryDate,
                    Quantity = item.QuantitySent,
                    Remarks = item.RemarksLine ?? string.Empty
                };
                
                TransferItems.Add(itemViewModel);
            }

            AppLogger.LogInfo("LoadTransferForEditing", $"Loaded transfer with {TransferItems.Count} items", "stock_transfer");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadTransferForEditing", ex, $"Error loading transfer ID: {transferId}", "stock_transfer");
            throw;
        }
    }
}

/// <summary>
/// ViewModel for individual stock transfer items
/// </summary>
public partial class StockTransferItemViewModel : ObservableObject
{
    private readonly AddStockTransferViewModel _parentViewModel;

    [ObservableProperty]
    private int? productId;

    [ObservableProperty]
    private long? uomId;

    [ObservableProperty]
    private string? batchNo;

    [ObservableProperty]
    private DateTime? expiryDate;

    [ObservableProperty]
    private decimal quantity = 0;

    [ObservableProperty]
    private string remarks = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    // Stock validation properties
    [ObservableProperty]
    private decimal availableStock = 0;

    [ObservableProperty]
    private bool hasStockWarning = false;

    [ObservableProperty]
    private string stockWarningMessage = string.Empty;

    [ObservableProperty]
    private bool isQuantityValid = true;

    public StockTransferItemViewModel(AddStockTransferViewModel parentViewModel)
    {
        _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
    }

    /// <summary>
    /// Handle product selection change
    /// </summary>
    partial void OnProductIdChanged(int? value)
    {
        if (value.HasValue)
        {
            _ = _parentViewModel.OnProductSelectedAsync(value.Value, this);
        }
    }

    /// <summary>
    /// Handle batch selection change
    /// </summary>
    partial void OnBatchNoChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var selectedBatch = ProductBatches
                .FirstOrDefault(b => b.BatchNo == value);
            
            if (selectedBatch != null)
            {
                ExpiryDate = selectedBatch.ExpiryDate;
            }
        }
    }

    /// <summary>
    /// Handle quantity change and validate against available stock
    /// </summary>
    partial void OnQuantityChanged(decimal value)
    {
        ValidateQuantity();
    }

    /// <summary>
    /// Update available stock for the selected product using product's initial stock
    /// </summary>
    public void UpdateAvailableStock()
    {
        if (!ProductId.HasValue)
        {
            AvailableStock = 0;
            return;
        }

        try
        {
            // Get the selected product from the parent's products collection
            var selectedProduct = _parentViewModel.Products.FirstOrDefault(p => p.Id == ProductId.Value);
            if (selectedProduct != null)
            {
                // Use the product's initial stock as available stock
                AvailableStock = selectedProduct.InitialStock;
                AppLogger.LogInfo($"Updated available stock for product {selectedProduct.Name}: {AvailableStock}");
            }
            else
            {
                AvailableStock = 0;
                AppLogger.LogWarning($"Product with ID {ProductId.Value} not found in products collection");
            }
            
            ValidateQuantity();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error updating available stock for product {ProductId}: {ex.Message}");
            AvailableStock = 0;
        }
    }

    /// <summary>
    /// Validate quantity against available stock
    /// </summary>
    private void ValidateQuantity()
    {
        if (Quantity > AvailableStock)
        {
            HasStockWarning = true;
            IsQuantityValid = false;
            StockWarningMessage = $"Warning: Only {AvailableStock} items available in stock";
        }
        else
        {
            HasStockWarning = false;
            IsQuantityValid = true;
            StockWarningMessage = string.Empty;
        }
    }

    /// <summary>
    /// Validate the item data
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();

        if (!ProductId.HasValue)
            errors.Add("Product is required");

        if (!UomId.HasValue)
            errors.Add("Unit of Measurement is required");

        if (Quantity <= 0)
            errors.Add("Quantity must be greater than 0");

        return errors;
    }
}