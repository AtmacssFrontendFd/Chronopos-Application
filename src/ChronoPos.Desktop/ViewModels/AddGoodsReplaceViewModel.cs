using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for adding new Goods Replace with comprehensive form validation and full settings integration
/// </summary>
public partial class AddGoodsReplaceViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IGoodsReplaceService _goodsReplaceService;
    private readonly IGoodsReplaceItemService _goodsReplaceItemService;
    private readonly IGoodsReturnService _goodsReturnService;
    private readonly IStoreService _storeService;
    private readonly IProductService _productService;
    private readonly IUomService _uomService;
    private readonly IProductBatchService _productBatchService;
    internal readonly IStockService _stockService;
    private readonly Action? _navigateBack;
    private readonly int? _replaceId;
    
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

    // Goods Replace Header Properties
    [ObservableProperty]
    private string replaceNo = string.Empty;

    [ObservableProperty]
    private string status = "Pending";

    [ObservableProperty]
    private int? storeId;

    [ObservableProperty]
    private int? referenceReturnId;

    [ObservableProperty]
    private long? supplierId;

    [ObservableProperty]
    private DateTime replaceDate = DateTime.Today;

    [ObservableProperty]
    private string remarks = string.Empty;

    [ObservableProperty]
    private decimal totalAmount = 0;

    // Collections
    [ObservableProperty]
    private ObservableCollection<StoreDto> stores = new();

    [ObservableProperty]
    private ObservableCollection<GoodsReturnDto> goodsReturns = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> unitsOfMeasurement = new();

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    // Goods Replace Items
    [ObservableProperty]
    private ObservableCollection<GoodsReplaceItemViewModel> replaceItems = new();

    [ObservableProperty]
    private GoodsReplaceItemViewModel? selectedReplaceItem;

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

    // Auto-filled fields
    [ObservableProperty]
    private string supplierName = string.Empty;

    [ObservableProperty]
    private string storeName = string.Empty;

    #endregion

    #region Text Properties for Localization

    [ObservableProperty]
    private string addReplaceTitle = "Add Goods Replace";

    [ObservableProperty]
    private string backButtonText = "Back";

    [ObservableProperty]
    private string replaceHeaderTitle = "Replacement Header";

    [ObservableProperty]
    private string replaceItemsTitle = "Replacement Items";

    [ObservableProperty]
    private string summaryTitle = "Review & Save";

    [ObservableProperty]
    private string replaceNoLabel = "Replace No";

    [ObservableProperty]
    private string statusLabel = "Status";

    [ObservableProperty]
    private string storeLabel = "Store";

    [ObservableProperty]
    private string referenceReturnLabel = "Reference Return No";

    [ObservableProperty]
    private string supplierLabel = "Supplier";

    [ObservableProperty]
    private string replaceDateLabel = "Replace Date";

    [ObservableProperty]
    private string remarksLabel = "Remarks";

    [ObservableProperty]
    private string totalAmountLabel = "Total Amount";

    [ObservableProperty]
    private string addProductButtonText = "Add Product";

    [ObservableProperty]
    private string noItemsMessage = "No items have been added to this replacement";

    /// <summary>
    /// Indicates if this is in edit mode (true) or add mode (false)
    /// </summary>
    public bool IsEditMode => _replaceId.HasValue;

    [ObservableProperty]
    private string totalItemsLabel = "Total Items";

    [ObservableProperty]
    private string totalQuantityLabel = "Total Quantity";

    [ObservableProperty]
    private string cancelButtonText = "Cancel";
    
    [ObservableProperty]
    private string saveDraftButtonText = "Save as Draft";
    
    [ObservableProperty]
    private string postReplaceButtonText = "Post Replace";

    #endregion

    #region Constructor

    public AddGoodsReplaceViewModel(
        IGoodsReplaceService goodsReplaceService,
        IGoodsReplaceItemService goodsReplaceItemService,
        IGoodsReturnService goodsReturnService,
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
        int? replaceId = null)
    {
        _goodsReplaceService = goodsReplaceService ?? throw new ArgumentNullException(nameof(goodsReplaceService));
        _goodsReplaceItemService = goodsReplaceItemService ?? throw new ArgumentNullException(nameof(goodsReplaceItemService));
        _goodsReturnService = goodsReturnService ?? throw new ArgumentNullException(nameof(goodsReturnService));
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
        _replaceId = replaceId;

        // Subscribe to settings changes
        _themeService.ThemeChanged += OnThemeChanged;
        _zoomService.ZoomChanged += OnZoomChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _colorSchemeService.PrimaryColorChanged += OnPrimaryColorChanged;
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;

        // Initialize with current settings
        InitializeSettings();

        // Load initial data
        _ = InitializeAsync();

        AppLogger.LogInfo("AddGoodsReplaceViewModel initialized", "ViewModel", "viewmodel");
    }

    #endregion

    #region Initialization

    private void InitializeSettings()
    {
        // Set current direction
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection switch
        {
            Desktop.Services.LayoutDirection.LeftToRight => FlowDirection.LeftToRight,
            Desktop.Services.LayoutDirection.RightToLeft => FlowDirection.RightToLeft,
            _ => FlowDirection.LeftToRight
        };
    }

    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading data...";

            // Load reference data
            await LoadStoresAsync();
            await LoadGoodsReturnsAsync();
            await LoadProductsAsync();
            await LoadUnitsOfMeasurementAsync();

            // Generate Replace No if new
            if (!IsEditMode)
            {
                await GenerateReplaceNoAsync();
            }
            else
            {
                await LoadReplaceDataAsync(_replaceId!.Value);
            }

            AppLogger.LogInfo("AddGoodsReplaceViewModel initialized successfully", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error initializing AddGoodsReplaceViewModel: {ex.Message}", ex, "viewmodel");
            ValidationMessage = $"Error loading data: {ex.Message}";
            HasValidationErrors = true;
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    private async Task LoadStoresAsync()
    {
        try
        {
            var storesList = await _storeService.GetAllAsync();
            Stores = new ObservableCollection<StoreDto>(storesList);
            AppLogger.LogInfo($"Loaded {Stores.Count} stores", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading stores: {ex.Message}", ex, "viewmodel");
        }
    }

    private async Task LoadGoodsReturnsAsync()
    {
        try
        {
            // Load only Pending and Posted Goods Returns eligible for replacement
            var returnsList = await _goodsReturnService.GetGoodsReturnsAsync(
                searchTerm: null,
                supplierId: null,
                storeId: null,
                status: null,
                startDate: null,
                endDate: null
            );

            // Filter to show only Pending or Posted returns that are NOT totally replaced
            var eligibleReturns = returnsList
                .Where(r => (r.Status == "Pending" || r.Status == "Posted") && !r.IsTotallyReplaced)
                .ToList();

            GoodsReturns = new ObservableCollection<GoodsReturnDto>(eligibleReturns);
            AppLogger.LogInfo($"Loaded {GoodsReturns.Count} eligible goods returns (excluding fully replaced)", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading goods returns: {ex.Message}", ex, "viewmodel");
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var productsList = await _productService.GetAllProductsAsync();
            Products = new ObservableCollection<ProductDto>(productsList);
            AppLogger.LogInfo($"Loaded {Products.Count} products", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading products: {ex.Message}", ex, "viewmodel");
        }
    }

    private async Task LoadUnitsOfMeasurementAsync()
    {
        try
        {
            var uomList = await _uomService.GetAllUomsAsync();
            UnitsOfMeasurement = new ObservableCollection<UnitOfMeasurementDto>(uomList);
            AppLogger.LogInfo($"Loaded {UnitsOfMeasurement.Count} units of measurement", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading units of measurement: {ex.Message}", ex, "viewmodel");
        }
    }

    private async Task GenerateReplaceNoAsync()
    {
        try
        {
            // Get all existing replaces to generate next number
            var allReplaces = await _goodsReplaceService.GetGoodsReplacesAsync(1, 10000, null, null, null, null, null, null);
            var maxNumber = allReplaces.Items.Any() 
                ? allReplaces.Items.Max(r => int.Parse(r.ReplaceNo.Replace("GR", ""))) 
                : 0;
            
            ReplaceNo = $"GR{(maxNumber + 1):D5}";
            AppLogger.LogInfo($"Generated Replace No: {ReplaceNo}", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error generating replace number: {ex.Message}", ex, "viewmodel");
            ReplaceNo = $"GR{DateTime.Now.Ticks:D5}";
        }
    }

    private async Task LoadReplaceDataAsync(int replaceId)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading goods replace data...";

            // Load the goods replace from service
            var goodsReplace = await _goodsReplaceService.GetGoodsReplaceByIdAsync(replaceId);
            
            if (goodsReplace == null)
            {
                throw new InvalidOperationException($"Goods Replace with ID {replaceId} not found");
            }

            // Verify it's in Pending status (only pending can be edited)
            if (goodsReplace.Status != "Pending")
            {
                throw new InvalidOperationException($"Cannot edit Goods Replace with status '{goodsReplace.Status}'. Only 'Pending' replacements can be edited.");
            }

            // Load header data
            ReplaceNo = goodsReplace.ReplaceNo;
            ReplaceDate = goodsReplace.ReplaceDate;
            StoreId = goodsReplace.StoreId;
            StoreName = goodsReplace.StoreName;
            SupplierId = goodsReplace.SupplierId;
            SupplierName = goodsReplace.SupplierName;
            ReferenceReturnId = goodsReplace.ReferenceReturnId;
            Status = goodsReplace.Status;
            Remarks = goodsReplace.Remarks ?? string.Empty;
            TotalAmount = goodsReplace.TotalAmount;

            // Load goods return items to get return quantities
            IEnumerable<GoodsReturnItemDto>? returnItems = null;
            if (goodsReplace.ReferenceReturnId.HasValue)
            {
                try
                {
                    returnItems = await _goodsReturnService.GetGoodsReturnItemsAsync(goodsReplace.ReferenceReturnId.Value);
                }
                catch (Exception ex)
                {
                    AppLogger.LogWarning($"Failed to load return items for return ID {goodsReplace.ReferenceReturnId.Value}: {ex.Message}", "viewmodel");
                }
            }

            // Load items
            ReplaceItems.Clear();
            foreach (var item in goodsReplace.Items)
            {
                // Find matching return item to get return quantity and already replaced quantity
                decimal returnQuantity = 0;
                decimal alreadyReplacedQuantity = 0;
                if (item.ReferenceReturnItemId.HasValue && returnItems != null)
                {
                    var returnItem = returnItems.FirstOrDefault(ri => ri.Id == item.ReferenceReturnItemId.Value);
                    if (returnItem != null)
                    {
                        returnQuantity = returnItem.Quantity;
                        alreadyReplacedQuantity = returnItem.AlreadyReplacedQuantity;
                    }
                }

                var replaceItemVm = new GoodsReplaceItemViewModel(
                    _productService,
                    _uomService,
                    _productBatchService,
                    _stockService,
                    this)
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UomId = item.UomId,
                    UomName = item.UomName,
                    BatchNo = item.BatchNo ?? string.Empty,
                    ExpiryDate = item.ExpiryDate,
                    ReturnQuantity = returnQuantity, // Load from goods return item
                    AlreadyReplacedQuantity = alreadyReplacedQuantity, // Load from goods return item
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    Amount = item.Quantity * item.Rate, // Calculate amount
                    ReferenceReturnItemId = item.ReferenceReturnItemId,
                    Remarks = item.RemarksLine ?? string.Empty
                };

                // Load available stock from batch for the product
                await replaceItemVm.LoadBatchQuantityAsync();

                ReplaceItems.Add(replaceItemVm);
            }

            CalculateTotals();
            
            AppLogger.LogInfo($"Loaded Goods Replace for editing: {goodsReplace.ReplaceNo}", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading goods replace data: {ex.Message}", ex, "viewmodel");
            ValidationMessage = $"Error loading goods replace: {ex.Message}";
            HasValidationErrors = true;
            
            // Navigate back on error
            _navigateBack?.Invoke();
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnReferenceReturnIdChanged(int? value)
    {
        if (value.HasValue)
        {
            _ = LoadReturnDetailsAsync(value.Value);
        }
        else
        {
            // Clear auto-filled fields
            SupplierId = null;
            SupplierName = string.Empty;
            StoreName = string.Empty;
            ReplaceItems.Clear();
            CalculateTotals();
        }
    }

    private async Task LoadReturnDetailsAsync(int returnId)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading return details...";

            var goodsReturn = GoodsReturns.FirstOrDefault(gr => gr.Id == returnId);
            if (goodsReturn != null)
            {
                // Auto-fill Supplier and Store
                SupplierId = goodsReturn.SupplierId;
                SupplierName = goodsReturn.SupplierName ?? "Unknown Supplier";
                StoreId = goodsReturn.StoreId;
                StoreName = goodsReturn.StoreName ?? "Unknown Store";

                // Auto-load items from return
                await LoadReturnItemsAsync(returnId);

                AppLogger.LogInfo($"Loaded details from Return: {goodsReturn.ReturnNo}", "ViewModel", "viewmodel");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading return details: {ex.Message}", ex, "viewmodel");
            ValidationMessage = $"Error loading return details: {ex.Message}";
            HasValidationErrors = true;
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    private async Task LoadReturnItemsAsync(int returnId)
    {
        try
        {
            var returnItems = await _goodsReturnService.GetGoodsReturnItemsAsync(returnId);
            
            ReplaceItems.Clear();
            foreach (var item in returnItems)
            {
                var replaceItemVm = new GoodsReplaceItemViewModel(
                    _productService,
                    _uomService,
                    _productBatchService,
                    _stockService,
                    this)
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName ?? string.Empty,
                    UomId = item.UomId,
                    UomName = item.UomName ?? string.Empty,
                    BatchNo = item.BatchNo ?? string.Empty,
                    ExpiryDate = item.ExpiryDate,
                    ReturnQuantity = item.Quantity, // Return quantity from goods return
                    AlreadyReplacedQuantity = item.AlreadyReplacedQuantity, // Already replaced quantity
                    // PendingQuantity will auto-calculate in partial method
                    Quantity = 0, // User enters replacement quantity
                    Rate = item.CostPrice, // Use CostPrice from return
                    Amount = item.LineTotal, // Amount from LineTotal (editable)
                    ReferenceReturnItemId = item.Id,
                    Remarks = string.Empty
                };

                // Load available stock from batch for the product
                await replaceItemVm.LoadBatchQuantityAsync();

                ReplaceItems.Add(replaceItemVm);
            }

            CalculateTotals();
            AppLogger.LogInfo($"Loaded {ReplaceItems.Count} items from return", "ViewModel", "viewmodel");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading return items: {ex.Message}", ex, "viewmodel");
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void AddReplaceItem()
    {
        var newItem = new GoodsReplaceItemViewModel(
            _productService,
            _uomService,
            _productBatchService,
            _stockService,
            this);

        ReplaceItems.Add(newItem);
        CalculateTotals();
        AppLogger.LogInfo("Added new replace item", "ViewModel", "viewmodel");
    }

    [RelayCommand]
    private void RemoveReplaceItem(GoodsReplaceItemViewModel item)
    {
        if (item != null)
        {
            ReplaceItems.Remove(item);
            CalculateTotals();
            AppLogger.LogInfo("Removed replace item", "ViewModel", "viewmodel");
        }
    }

    [RelayCommand]
    private async Task SaveDraft()
    {
        if (ValidateReplace())
        {
            await SaveReplaceAsync("Pending");
        }
    }

    [RelayCommand]
    private async Task PostReplace()
    {
        if (ValidateReplace())
        {
            var result = MessageBox.Show(
                "Are you sure you want to post this replacement? This will update stock levels and the replacement cannot be edited afterwards.",
                "Confirm Post",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await SaveReplaceAsync("Posted");
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        var result = MessageBox.Show(
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            "Confirm Cancel",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _navigateBack?.Invoke();
            AppLogger.LogInfo("Goods Replace cancelled", "ViewModel", "viewmodel");
        }
    }

    #endregion

    #region Validation

    private bool ValidateReplace()
    {
        HasValidationErrors = false;
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ReplaceNo))
        {
            ValidationMessage = "Replace No is required";
            HasValidationErrors = true;
            return false;
        }

        if (!StoreId.HasValue)
        {
            ValidationMessage = "Please select a Store";
            HasValidationErrors = true;
            return false;
        }

        if (!ReferenceReturnId.HasValue)
        {
            ValidationMessage = "Please select a Reference Return";
            HasValidationErrors = true;
            return false;
        }

        // Filter items with pending quantity > 0 (items that still need replacement)
        var itemsToReplace = ReplaceItems.Where(i => i.PendingQuantity > 0).ToList();

        if (itemsToReplace.Count == 0)
        {
            ValidationMessage = "No items available for replacement. All items are fully replaced.";
            HasValidationErrors = true;
            return false;
        }

        // Validate only items with pending quantity > 0
        foreach (var item in itemsToReplace)
        {
            if (!item.ProductId.HasValue)
            {
                ValidationMessage = "All items with pending quantity must have a product selected";
                HasValidationErrors = true;
                return false;
            }

            if (item.Quantity <= 0)
            {
                ValidationMessage = "All items with pending quantity must have a replacement quantity greater than 0";
                HasValidationErrors = true;
                return false;
            }

            if (item.Quantity > item.PendingQuantity)
            {
                ValidationMessage = $"Replacement quantity ({item.Quantity}) cannot exceed pending quantity ({item.PendingQuantity}) for {item.ProductName}";
                HasValidationErrors = true;
                return false;
            }

            if (item.Rate <= 0)
            {
                ValidationMessage = "All items with pending quantity must have a rate greater than 0";
                HasValidationErrors = true;
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Save Logic

    private async Task SaveReplaceAsync(string saveStatus)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Saving replacement...";

            // Filter items: Only save items with PendingQuantity > 0 and Quantity > 0
            var itemsToSave = ReplaceItems
                .Where(item => item.PendingQuantity > 0 && item.Quantity > 0)
                .ToList();

            if (itemsToSave.Count == 0)
            {
                ValidationMessage = "No items to save. All items are fully replaced or have zero quantity.";
                HasValidationErrors = true;
                return;
            }

            var replaceDto = new CreateGoodsReplaceDto
            {
                SupplierId = (int)SupplierId!.Value,
                StoreId = StoreId!.Value,
                ReferenceReturnId = ReferenceReturnId,
                ReplaceDate = ReplaceDate,
                Status = saveStatus,
                Remarks = Remarks,
                Items = itemsToSave.Select(item => new CreateGoodsReplaceItemDto
                {
                    ProductId = item.ProductId!.Value,
                    UomId = item.UomId!.Value,
                    BatchNo = item.BatchNo,
                    ExpiryDate = item.ExpiryDate,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    ReferenceReturnItemId = item.ReferenceReturnItemId,
                    RemarksLine = item.Remarks
                }).ToList()
            };

            if (IsEditMode)
            {
                // Update always keeps status as Pending
                await _goodsReplaceService.UpdateGoodsReplaceAsync(_replaceId!.Value, replaceDto);
                AppLogger.LogInfo($"Updated Goods Replace: {ReplaceNo}", "ViewModel", "viewmodel");

                // If user clicked "Post Replace" button, post it after updating
                if (saveStatus == "Posted")
                {
                    StatusMessage = "Posting replacement and updating stock...";
                    var posted = await _goodsReplaceService.PostGoodsReplaceAsync(_replaceId!.Value);
                    if (posted)
                    {
                        AppLogger.LogInfo($"Posted Goods Replace: {ReplaceNo} - Stock updated", "ViewModel", "viewmodel");
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to post goods replace");
                    }
                }
            }
            else
            {
                // For new records, create with the specified status
                await _goodsReplaceService.CreateGoodsReplaceAsync(replaceDto);
                AppLogger.LogInfo($"Created Goods Replace: {ReplaceNo} with status: {saveStatus}", "ViewModel", "viewmodel");
            }

            var successMessage = saveStatus == "Posted" 
                ? $"Goods Replacement {(IsEditMode ? "updated and posted" : "posted")} successfully! Stock has been updated."
                : $"Goods Replacement saved as draft successfully!";

            MessageBox.Show(
                successMessage,
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _navigateBack?.Invoke();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error saving goods replace: {ex.Message}", ex, "viewmodel");
            ValidationMessage = $"Error saving: {ex.Message}";
            HasValidationErrors = true;
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    #endregion

    #region Helper Methods

    public void CalculateTotals()
    {
        TotalItems = ReplaceItems.Count;
        TotalQuantity = ReplaceItems.Sum(item => item.Quantity);
        TotalAmount = ReplaceItems.Sum(item => item.Amount);
    }

    #endregion

    #region Settings Event Handlers

    private void OnThemeChanged(Theme theme)
    {
        // Reload theme
    }

    private void OnZoomChanged(ZoomLevel zoomLevel)
    {
        // Handle zoom change
    }

    private void OnLanguageChanged(SupportedLanguage language)
    {
        // Reload localized text
        // TODO: Implement localization
    }

    private void OnPrimaryColorChanged(ColorOption colorOption)
    {
        // Handle color change
    }

    private void OnDirectionChanged(Desktop.Services.LayoutDirection direction)
    {
        CurrentFlowDirection = direction switch
        {
            Desktop.Services.LayoutDirection.LeftToRight => FlowDirection.LeftToRight,
            Desktop.Services.LayoutDirection.RightToLeft => FlowDirection.RightToLeft,
            _ => FlowDirection.LeftToRight
        };
    }

    private void OnDatabaseLanguageChanged(object? sender, string languageCode)
    {
        // Reload database text
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        _themeService.ThemeChanged -= OnThemeChanged;
        _zoomService.ZoomChanged -= OnZoomChanged;
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _colorSchemeService.PrimaryColorChanged -= OnPrimaryColorChanged;
        _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
        
        AppLogger.LogInfo("AddGoodsReplaceViewModel disposed", "ViewModel", "viewmodel");
    }

    #endregion
}

/// <summary>
/// ViewModel for individual Goods Replace Item in the grid
/// </summary>
public partial class GoodsReplaceItemViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IUomService _uomService;
    private readonly IProductBatchService _productBatchService;
    private readonly IStockService _stockService;
    private readonly AddGoodsReplaceViewModel _parentViewModel;

    [ObservableProperty]
    private int? productId;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private long? uomId;

    [ObservableProperty]
    private string uomName = string.Empty;

    [ObservableProperty]
    private string batchNo = string.Empty;

    [ObservableProperty]
    private DateTime? expiryDate;

    [ObservableProperty]
    private decimal returnQuantity = 0; // Quantity from goods return

    [ObservableProperty]
    private decimal alreadyReplacedQuantity = 0; // Already replaced quantity from goods return item

    [ObservableProperty]
    private decimal pendingQuantity = 0; // Pending = ReturnQuantity - AlreadyReplacedQuantity

    [ObservableProperty]
    private decimal quantity = 0;

    [ObservableProperty]
    private decimal rate = 0;

    [ObservableProperty]
    private decimal amount = 0;

    [ObservableProperty]
    private int? referenceReturnItemId;

    [ObservableProperty]
    private string remarks = string.Empty;

    [ObservableProperty]
    private decimal availableStock = 0; // Available quantity from batch

    [ObservableProperty]
    private bool hasStockWarning = false;

    [ObservableProperty]
    private string stockWarningMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    public GoodsReplaceItemViewModel(
        IProductService productService,
        IUomService uomService,
        IProductBatchService productBatchService,
        IStockService stockService,
        AddGoodsReplaceViewModel parentViewModel)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
        _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
    }

    partial void OnProductIdChanged(int? value)
    {
        if (value.HasValue)
        {
            _ = LoadProductDetailsAsync(value.Value);
        }
    }

    partial void OnReturnQuantityChanged(decimal value)
    {
        // Recalculate pending quantity when return quantity changes
        PendingQuantity = ReturnQuantity - AlreadyReplacedQuantity;
    }

    partial void OnAlreadyReplacedQuantityChanged(decimal value)
    {
        // Recalculate pending quantity when already replaced quantity changes
        PendingQuantity = ReturnQuantity - AlreadyReplacedQuantity;
    }

    partial void OnQuantityChanged(decimal value)
    {
        // Don't auto-calculate Amount, it's editable from LineTotal
        _parentViewModel.CalculateTotals();
        CheckStockWarning();
    }

    partial void OnRateChanged(decimal value)
    {
        // Don't auto-calculate Amount, it's editable from LineTotal
        _parentViewModel.CalculateTotals();
    }

    partial void OnAmountChanged(decimal value)
    {
        // Amount is editable, recalculate parent totals when changed
        _parentViewModel.CalculateTotals();
    }

    partial void OnBatchNoChanged(string value)
    {
        // When batch changes, reload the batch quantity
        if (!string.IsNullOrEmpty(value) && ProductId.HasValue)
        {
            _ = LoadBatchQuantityAsync();
        }
    }

    private async Task LoadProductDetailsAsync(int productId)
    {
        try
        {
            var product = _parentViewModel.Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                ProductName = product.Name;
                UomId = product.UnitOfMeasurementId;
                
                // Load batches for this product
                await LoadProductBatchesAsync(productId);
                
                // Load available stock
                await LoadAvailableStockAsync();
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading product details: {ex.Message}", ex, "viewmodel");
        }
    }

    private async Task LoadProductBatchesAsync(int productId)
    {
        try
        {
            var batches = await _productBatchService.GetProductBatchesByProductIdAsync(productId);
            ProductBatches = new ObservableCollection<ProductBatchDto>(batches);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading product batches: {ex.Message}", ex, "viewmodel");
        }
    }

    /// <summary>
    /// Load available quantity from the specific batch
    /// </summary>
    public async Task LoadBatchQuantityAsync()
    {
        if (ProductId.HasValue && !string.IsNullOrEmpty(BatchNo))
        {
            try
            {
                var batches = await _productBatchService.GetProductBatchesByProductIdAsync(ProductId.Value);
                var batch = batches.FirstOrDefault(b => b.BatchNo == BatchNo);
                
                if (batch != null)
                {
                    AvailableStock = batch.Quantity; // Available qty from batch
                    AppLogger.LogInfo($"Loaded batch quantity: {AvailableStock} for batch {BatchNo}", "ViewModel", "viewmodel");
                }
                else
                {
                    AvailableStock = 0;
                    AppLogger.LogWarning($"Batch {BatchNo} not found for product {ProductId}", "ViewModel", "viewmodel");
                }
                
                CheckStockWarning();
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error loading batch quantity: {ex.Message}", ex, "viewmodel");
            }
        }
    }

    /// <summary>
    /// Load total available stock for the product (optional - for reference)
    /// </summary>
    public async Task LoadAvailableStockAsync()
    {
        if (ProductId.HasValue && _parentViewModel.StoreId.HasValue)
        {
            try
            {
                var stock = await _stockService.GetStockLevelAsync(
                    ProductId.Value,
                    _parentViewModel.StoreId.Value);
                
                // This is total stock, but we use batch quantity for AvailableStock
                var totalStock = stock?.CurrentStock ?? 0;
                AppLogger.LogInfo($"Total stock for product: {totalStock}", "ViewModel", "viewmodel");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error loading available stock: {ex.Message}", ex, "viewmodel");
            }
        }
    }

    private void CheckStockWarning()
    {
        // No stock warning for replacement as we're receiving items
        HasStockWarning = false;
        StockWarningMessage = string.Empty;
    }
}
