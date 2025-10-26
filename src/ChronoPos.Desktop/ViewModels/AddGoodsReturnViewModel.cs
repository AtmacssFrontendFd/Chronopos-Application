using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using System.Globalization;
using ChronoPos.Application.Logging;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for adding new Goods Return with comprehensive form validation and full settings integration
/// </summary>
public partial class AddGoodsReturnViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IGoodsReturnService _goodsReturnService;
    private readonly IGoodsReturnItemService _goodsReturnItemService;
    private readonly IGoodsReceivedService _goodsReceivedService;
    // Note: IGoodsReceivedItemService not included in constructor - functionality deferred
    private readonly IStoreService _storeService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;
    private readonly IUomService _uomService;
    private readonly IProductBatchService _productBatchService;
    private readonly Action? _navigateBack;
    private readonly int? _returnId;
    
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

    // Goods Return Header Properties
    [ObservableProperty]
    private string returnNo = string.Empty;

    [ObservableProperty]
    private DateTime returnDate = DateTime.Today;

    [ObservableProperty]
    private int? storeId;

    [ObservableProperty]
    private long? supplierId;

    [ObservableProperty]
    private int? grnId;

    [ObservableProperty]
    private string invoiceNo = string.Empty;

    [ObservableProperty]
    private string reason = "Others";

    [ObservableProperty]
    private string remarks = string.Empty;

    [ObservableProperty]
    private string status = "Pending";

    [ObservableProperty]
    private int? editGoodsReturnId = null;

    [ObservableProperty]
    private bool isEditMode = false;

    // Collections
    [ObservableProperty]
    private ObservableCollection<StoreDto> stores = new();

    [ObservableProperty]
    private ObservableCollection<SupplierDto> suppliers = new();

    [ObservableProperty]
    private ObservableCollection<GoodsReceivedDto> goodsReceivedNotes = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> unitsOfMeasurement = new();

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    // Goods Return Items
    [ObservableProperty]
    private ObservableCollection<GoodsReturnItemViewModel> returnItems = new();

    [ObservableProperty]
    private GoodsReturnItemViewModel? selectedReturnItem;

    // Status and validation properties
    [ObservableProperty]
    private bool isSaving = false;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationErrors = false;

    // Localized titles and labels
    [ObservableProperty]
    private string returnHeaderTitle = "Header";

    [ObservableProperty]
    private string returnItemsTitle = "Items";

    [ObservableProperty]
    private string summaryTitle = "Review & Save";

    [ObservableProperty]
    private string pageTitle = "Add Goods Return";

    // Field labels
    [ObservableProperty]
    private string returnNoLabel = "Return No";

    [ObservableProperty]
    private string statusLabel = "Status";

    [ObservableProperty]
    private string returnDateLabel = "Return Date";

    [ObservableProperty]
    private string storeLabel = "Store";

    [ObservableProperty]
    private string supplierLabel = "Supplier";

    [ObservableProperty]
    private string grnLabel = "GRN";

    [ObservableProperty]
    private string reasonLabel = "Reason";

    [ObservableProperty]
    private string backButtonText = "Back to Stock Management";

    [ObservableProperty]
    private string cancelButtonText = "Cancel";

    [ObservableProperty]
    private string saveDraftButtonText = "Save as Draft";

    [ObservableProperty]
    private string postReturnButtonText = "Post Return";

    // Computed properties for UI text based on edit mode
    public string SaveButtonText => IsEditMode ? "Update Return" : "Save as Draft";
    public string WindowTitle => IsEditMode ? "Edit Goods Return" : "Add Goods Return";

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(SaveButtonText));
        OnPropertyChanged(nameof(WindowTitle));
    }

    [ObservableProperty]
    private string addItemButtonText = "Add Item";

    // Settings properties
    [ObservableProperty]
    private double currentZoomLevel = 1.0;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private CultureInfo currentCulture = CultureInfo.CurrentCulture;

    // Dropdown collections
    public ObservableCollection<string> ReasonOptions { get; } = new()
    {
        "Expired",
        "Damaged", 
        "Wrong Item",
        "Others"
    };

    // Computed properties
    public decimal TotalReturnAmount => ReturnItems.Sum(item => item.LineTotal);

    /// <summary>
    /// Updates the total return amount property
    /// </summary>
    public void UpdateTotalReturnAmount()
    {
        OnPropertyChanged(nameof(TotalReturnAmount));
    }

    /// <summary>
    /// Clears validation errors
    /// </summary>
    private void ClearValidationErrors()
    {
        ValidationMessage = string.Empty;
        HasValidationErrors = false;
    }
    public int TotalReturnItems => ReturnItems.Count;

    #endregion

    #region Constructor

    public AddGoodsReturnViewModel(
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        IGoodsReturnService goodsReturnService,
        IGoodsReturnItemService goodsReturnItemService,
        IGoodsReceivedService goodsReceivedService,
        IStoreService storeService,
        ISupplierService supplierService,
        IProductService productService,
        IUomService uomService,
        IProductBatchService productBatchService,
        Action? navigateBack = null,
        int? returnId = null)
    {
        AppLogger.LogInfo("AddGoodsReturnViewModel initialized", 
            $"GoodsReturnService: {(goodsReturnService != null ? "Available" : "Null")}", 
            "goods_return");

        // Store services
        _goodsReturnService = goodsReturnService ?? throw new ArgumentNullException(nameof(goodsReturnService));
        _goodsReturnItemService = goodsReturnItemService ?? throw new ArgumentNullException(nameof(goodsReturnItemService));
        _goodsReceivedService = goodsReceivedService ?? throw new ArgumentNullException(nameof(goodsReceivedService));
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _navigateBack = navigateBack;
        _returnId = returnId;

        // Set edit mode if returnId is provided
        if (returnId.HasValue)
        {
            EditGoodsReturnId = returnId.Value;
            IsEditMode = true;
        }

        // Store settings services
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

        // Load initial data
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            AppLogger.LogInfo("🚀 Starting AddGoodsReturnViewModel initialization", "Loading initial data", "goods_return_dropdowns");
            AppLogger.LogInfo($"🔧 Service availability check - StoreService: {_storeService != null}, SupplierService: {_supplierService != null}, GoodsReceivedService: {_goodsReceivedService != null}", "Service injection verification", "goods_return_dropdowns");

            // Generate return number if creating new return
            if (!_returnId.HasValue)
            {
                ReturnNo = await _goodsReturnService.GetNextReturnNumberAsync();
                AppLogger.LogInfo($"Generated return number: {ReturnNo}", "goods_return");
            }

            // Load reference data in parallel
            await Task.WhenAll(
                LoadStoresAsync(),
                LoadSuppliersAsync(),
                LoadProductsAsync(),
                LoadUnitsOfMeasurementAsync()
            );

            // If editing existing return, load the data
            if (_returnId.HasValue)
            {
                await LoadExistingReturnAsync(_returnId.Value);
            }
            else
            {
                // Add one empty row for new returns
                AddNewItem();
            }

            AppLogger.LogInfo("Initialization completed successfully", "goods_return");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to initialize AddGoodsReturnViewModel", ex, "Initialization error", "goods_return");
            ValidationMessage = $"Failed to load initial data: {ex.Message}";
            HasValidationErrors = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStoresAsync()
    {
        try
        {
            var storesList = await _storeService.GetAllAsync();
            Stores.Clear();
            foreach (var store in storesList)
            {
                Stores.Add(store);
            }
            AppLogger.LogInfo($"Loaded {Stores.Count} stores", "goods_return");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to load stores", ex, "Loading stores", "goods_return");
        }
    }

    private async Task LoadSuppliersAsync()
    {
        AppLogger.LogSeparator("LoadSuppliersAsync", "goods_return_dropdowns");
        AppLogger.LogInfo("🔄 Starting supplier dropdown loading", "Goods Return form initialization", "goods_return_dropdowns");
        
        try
        {
            // Log service availability
            if (_supplierService == null)
            {
                AppLogger.LogError("❌ SupplierService is null", null, "Service injection issue", "goods_return_dropdowns");
                return;
            }
            
            AppLogger.LogInfo("✅ SupplierService is available, calling GetAllAsync()", "Service ready", "goods_return_dropdowns");
            var startTime = DateTime.Now;
            
            var suppliers = await _supplierService.GetAllAsync();
            var duration = DateTime.Now - startTime;
            
            AppLogger.LogPerformance("SupplierService.GetAllAsync", duration, "Database query completed", "goods_return_dropdowns");
            
            if (suppliers == null)
            {
                AppLogger.LogWarning("⚠️ SupplierService.GetAllAsync returned null", "Empty result from service", "goods_return_dropdowns");
                return;
            }
            
            var suppliersList = suppliers.ToList();
            AppLogger.LogInfo($"📊 Retrieved {suppliersList.Count} suppliers from service", "Data retrieval successful", "goods_return_dropdowns");
            
            // Log existing collection state
            AppLogger.LogDebug($"🗂️ Current Suppliers collection count: {Suppliers.Count}", "Pre-clear state", "goods_return_dropdowns");
            
            Suppliers.Clear();
            AppLogger.LogDebug("🧹 Suppliers collection cleared", "Collection reset", "goods_return_dropdowns");
            
            foreach (var supplier in suppliersList)
            {
                Suppliers.Add(supplier);
                AppLogger.LogDebug($"➕ Added supplier: ID={supplier.SupplierId}, Company='{supplier.CompanyName}', IsActive={supplier.IsActive}", 
                    "Supplier added to dropdown", "goods_return_dropdowns");
            }
            
            AppLogger.LogInfo($"✅ Supplier dropdown loaded successfully with {Suppliers.Count} items", 
                "Dropdown population completed", "goods_return_dropdowns");

            // TEST: Let's also check if there are any GRNs in the database at all
            await TestLoadAllGrnsAsync();
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ Failed to load suppliers: {ex.Message}", ex, "Supplier dropdown loading error", "goods_return_dropdowns");
        }
        
        AppLogger.LogSeparator("LoadSuppliersAsync Complete", "goods_return_dropdowns");
    }

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
            AppLogger.LogInfo($"Loaded {Products.Count} products", "goods_return");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to load products", ex, "Loading products", "goods_return");
        }
    }

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
            AppLogger.LogInfo($"Loaded {UnitsOfMeasurement.Count} units of measurement", "goods_return");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to load units of measurement", ex, "Loading UOMs", "goods_return");
        }
    }

    private async Task LoadExistingReturnAsync(int returnId)
    {
        try
        {
            AppLogger.LogInfo($"Loading existing return: {returnId}", "goods_return");
            var existingReturn = await _goodsReturnService.GetGoodsReturnByIdAsync(returnId);
            
            if (existingReturn != null)
            {
                // Load header data
                ReturnNo = existingReturn.ReturnNo;
                ReturnDate = existingReturn.ReturnDate;
                StoreId = existingReturn.StoreId;
                SupplierId = existingReturn.SupplierId;
                
                // Trigger supplier change to load GRNs first
                if (SupplierId.HasValue && StoreId.HasValue)
                {
                    await LoadGoodsReceivedNotesAsync();
                }
                
                // Then set GRN ID after GRNs are loaded
                GrnId = existingReturn.ReferenceGrnId;
                
                // InvoiceNo is not part of GoodsReturnDto - it's handled via GRN reference
                Reason = existingReturn.Remarks ?? "Others";
                Remarks = existingReturn.Remarks ?? string.Empty;
                Status = existingReturn.Status;

                // Load items
                var items = await _goodsReturnService.GetGoodsReturnItemsAsync(returnId);
                ReturnItems.Clear();
                foreach (var item in items)
                {
                    var itemViewModel = new GoodsReturnItemViewModel(this)
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        BatchNo = item.BatchNo, // Load the actual batch number
                        UomId = item.UomId, // Load the UOM ID
                        ReturnQuantity = item.Quantity,
                        CostPrice = item.CostPrice,
                        Remarks = item.Reason ?? string.Empty
                    };
                    
                    // Load product batches for this item to populate batch dropdown
                    if (item.ProductId > 0)
                    {
                        await LoadProductBatchesForItemAsync(item.ProductId, itemViewModel);
                    }
                    
                    ReturnItems.Add(itemViewModel);
                }

                AppLogger.LogInfo($"Loaded return with {ReturnItems.Count} items", "goods_return");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Failed to load existing return {returnId}", ex, "Loading existing return", "goods_return");
            throw;
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task SaveAsDraft()
    {
        try
        {
            IsSaving = true;
            
            if (!ValidateReturn())
            {
                new MessageDialog("Validation Error", ValidationMessage, MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            Status = "Pending";
            var returnId = await SaveReturnAsync(false);
            
            if (returnId > 0)
            {
                ClearValidationErrors();
                new MessageDialog("Success", "Goods Return saved as draft successfully!", MessageDialog.MessageType.Success).ShowDialog();
                _navigateBack?.Invoke(); // Close the screen like PostReturn does
            }
            else
            {
                new MessageDialog("Error", "Failed to save return as draft. Please try again.", MessageDialog.MessageType.Error).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to save return as draft", ex, "Save draft operation", "goods_return");
            new MessageDialog("Error", $"Failed to save return: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task PostReturn()
    {
        try
        {
            IsSaving = true;
            
            if (!ValidateReturn())
            {
                new MessageDialog("Validation Error", ValidationMessage, MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Save as Pending first
            Status = "Pending";
            var returnId = await SaveReturnAsync(true);
            
            if (returnId > 0)
            {
                // Post the return to update inventory and change status to Posted
                var postSuccess = await _goodsReturnService.PostGoodsReturnAsync(returnId);
                if (postSuccess)
                {
                    Status = "Posted";
                    new MessageDialog("Success", "Goods Return posted successfully!", MessageDialog.MessageType.Success).ShowDialog();
                    _navigateBack?.Invoke();
                }
                else
                {
                    new MessageDialog("Error", "Failed to post return. Please try again.", MessageDialog.MessageType.Error).ShowDialog();
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to post return", ex, "Post return operation", "goods_return");
            new MessageDialog("Error", $"Failed to post return: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        var dialog = new ConfirmationDialog(
            "Confirm Cancel",
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            ConfirmationDialog.DialogType.Warning);
        
        var result = dialog.ShowDialog();
        
        if (result == true)
        {
            _navigateBack?.Invoke();
        }
    }

    [RelayCommand]
    private void AddNewItem()
    {
        var newItem = new GoodsReturnItemViewModel(this);
        ReturnItems.Add(newItem);
        ClearValidationErrors(); // Clear any previous errors when item is successfully added
        AppLogger.LogInfo($"Added new return item. Total items: {ReturnItems.Count}", "goods_return");
        OnPropertyChanged(nameof(TotalReturnItems));
    }

    [RelayCommand]
    private void RemoveItem(GoodsReturnItemViewModel item)
    {
        if (item != null && ReturnItems.Contains(item))
        {
            ReturnItems.Remove(item);
            ClearValidationErrors(); // Clear any previous errors when item is successfully removed
            AppLogger.LogInfo($"Removed return item. Total items: {ReturnItems.Count}", "goods_return");
            OnPropertyChanged(nameof(TotalReturnItems));
            OnPropertyChanged(nameof(TotalReturnAmount));
        }
    }



    #endregion

    #region GRN Selection and Auto-population

    partial void OnGrnIdChanged(int? value)
    {
        if (value.HasValue)
        {
            _ = LoadGrnDetailsAsync(value.Value);
        }
    }

    private async Task LoadGrnDetailsAsync(int grnId)
    {
        try
        {
            AppLogger.LogInfo($"Loading GRN details for ID: {grnId}", "goods_return");
            
            var grn = await _goodsReceivedService.GetByIdAsync(grnId);
            if (grn != null)
            {
                // Auto-populate header fields
                SupplierId = grn.SupplierId;
                InvoiceNo = grn.InvoiceNo ?? string.Empty;

                // TODO: Load GRN items - GoodsReceivedItemService not available in current constructor
                // For now, items will be manually added by the user
                ReturnItems.Clear();

                AppLogger.LogInfo($"Loaded GRN with {ReturnItems.Count} items", "goods_return");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Failed to load GRN details for ID: {grnId}", ex, "Loading GRN details", "goods_return");
            ValidationMessage = $"Failed to load GRN details: {ex.Message}";
            HasValidationErrors = true;
        }
    }

    partial void OnStoreIdChanged(int? value)
    {
        AppLogger.LogInfo($"🏪 Store selection changed to: {value}", "Store dropdown interaction", "goods_return_dropdowns");
        AppLogger.LogInfo($"📊 Current state - StoreId: {value}, SupplierId: {SupplierId}", "Selection state check", "goods_return_dropdowns");
        
        if (value.HasValue && SupplierId.HasValue)
        {
            AppLogger.LogInfo("✅ Both Store and Supplier selected, triggering GRN reload", "Conditions met for GRN loading", "goods_return_dropdowns");
            _ = LoadGoodsReceivedNotesAsync();
        }
        else
        {
            AppLogger.LogInfo("⚠️ Missing Store or Supplier selection, skipping GRN reload", "Conditions not met", "goods_return_dropdowns");
        }
    }

    partial void OnSupplierIdChanged(long? value)
    {
        AppLogger.LogInfo($"🏢 Supplier selection changed to: {value}", "Supplier dropdown interaction", "goods_return_dropdowns");
        AppLogger.LogInfo($"📊 Current state - StoreId: {StoreId}, SupplierId: {value}", "Selection state check", "goods_return_dropdowns");
        
        if (value.HasValue && StoreId.HasValue)
        {
            AppLogger.LogInfo("✅ Both Store and Supplier selected, triggering GRN reload", "Conditions met for GRN loading", "goods_return_dropdowns");
            _ = LoadGoodsReceivedNotesAsync();
        }
        else
        {
            AppLogger.LogInfo("⚠️ Missing Store or Supplier selection, skipping GRN reload", "Conditions not met", "goods_return_dropdowns");
        }
    }

    private async Task LoadGoodsReceivedNotesAsync()
    {
        AppLogger.LogSeparator("LoadGoodsReceivedNotesAsync", "goods_return_dropdowns");
        AppLogger.LogInfo("🔄 Starting GRN dropdown loading", "Goods Return form initialization", "goods_return_dropdowns");
        
        try
        {
            // Check prerequisites
            if (!StoreId.HasValue || !SupplierId.HasValue)
            {
                AppLogger.LogWarning($"⚠️ Prerequisites not met - StoreId: {StoreId}, SupplierId: {SupplierId}", 
                    "Cannot load GRNs without both Store and Supplier selected", "goods_return_dropdowns");
                return;
            }

            AppLogger.LogInfo($"📋 Prerequisites met - Store: {StoreId}, Supplier: {SupplierId}", "Ready to load GRNs", "goods_return_dropdowns");

            // Log service availability
            if (_goodsReceivedService == null)
            {
                AppLogger.LogError("❌ GoodsReceivedService is null", null, "Service injection issue", "goods_return_dropdowns");
                return;
            }
            
            AppLogger.LogInfo("✅ GoodsReceivedService is available, calling GetBySupplierIdAsync()", "Service ready", "goods_return_dropdowns");
            var startTime = DateTime.Now;
            
            // Get posted GRNs for the selected store and supplier
            var grns = await _goodsReceivedService.GetBySupplierIdAsync((int)SupplierId.Value);
            var duration = DateTime.Now - startTime;
            
            AppLogger.LogPerformance("GoodsReceivedService.GetBySupplierIdAsync", duration, "Database query completed", "goods_return_dropdowns");
            
            if (grns == null)
            {
                AppLogger.LogWarning("⚠️ GoodsReceivedService.GetBySupplierIdAsync returned null", "Empty result from service", "goods_return_dropdowns");
                return;
            }
            
            var allGrns = grns.ToList();
            AppLogger.LogInfo($"📊 Retrieved {allGrns.Count} total GRNs for supplier {SupplierId.Value}", "Raw data retrieval successful", "goods_return_dropdowns");
            
            var filteredGrns = allGrns.Where(g => g.StoreId == StoreId.Value && g.Status == "Posted").ToList();
            AppLogger.LogInfo($"🔍 After filtering (StoreId={StoreId.Value}, Status='Posted'): {filteredGrns.Count} GRNs", 
                "Filtering completed", "goods_return_dropdowns");
            
            // Log existing collection state
            AppLogger.LogDebug($"🗂️ Current GoodsReceivedNotes collection count: {GoodsReceivedNotes.Count}", "Pre-clear state", "goods_return_dropdowns");
            
            GoodsReceivedNotes.Clear();
            AppLogger.LogDebug("🧹 GoodsReceivedNotes collection cleared", "Collection reset", "goods_return_dropdowns");
            
            foreach (var grn in filteredGrns)
            {
                GoodsReceivedNotes.Add(grn);
                AppLogger.LogDebug($"➕ Added GRN: ID={grn.Id}, GrnNo='{grn.GrnNo}', Date={grn.ReceivedDate:yyyy-MM-dd}, Status='{grn.Status}'", 
                    "GRN added to dropdown", "goods_return_dropdowns");
            }
            
            AppLogger.LogInfo($"✅ GRN dropdown loaded successfully with {GoodsReceivedNotes.Count} items", 
                "Dropdown population completed", "goods_return_dropdowns");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ Failed to load GRNs: {ex.Message}", ex, "GRN dropdown loading error", "goods_return_dropdowns");
        }
        
        AppLogger.LogSeparator("LoadGoodsReceivedNotesAsync Complete", "goods_return_dropdowns");
    }

    private async Task TestLoadAllGrnsAsync()
    {
        AppLogger.LogSeparator("TestLoadAllGrnsAsync", "goods_return_dropdowns");
        AppLogger.LogInfo("🧪 TEST: Checking all GRNs in database", "Database test", "goods_return_dropdowns");
        
        try
        {
            if (_goodsReceivedService == null)
            {
                AppLogger.LogError("❌ TEST: GoodsReceivedService is null", null, "Service test", "goods_return_dropdowns");
                return;
            }

            // First, let's try to get all GRNs for each supplier
            var allSuppliers = await _supplierService.GetAllAsync();
            foreach (var supplier in allSuppliers)
            {
                AppLogger.LogInfo($"🔍 TEST: Checking GRNs for Supplier ID {supplier.SupplierId} ({supplier.CompanyName})", "Supplier GRN check", "goods_return_dropdowns");
                
                var supplierGrns = await _goodsReceivedService.GetBySupplierIdAsync((int)supplier.SupplierId);
                var grnList = supplierGrns.ToList();
                
                AppLogger.LogInfo($"📊 TEST: Found {grnList.Count} total GRNs for supplier {supplier.CompanyName}", "GRN count", "goods_return_dropdowns");
                
                foreach (var grn in grnList.Take(5)) // Show first 5 to avoid spam
                {
                    AppLogger.LogDebug($"📝 TEST GRN: ID={grn.Id}, GrnNo='{grn.GrnNo}', StoreId={grn.StoreId}, Status='{grn.Status}', Date={grn.ReceivedDate:yyyy-MM-dd}", 
                        "GRN details", "goods_return_dropdowns");
                }
                
                var postedGrns = grnList.Where(g => g.Status == "Posted").ToList();
                AppLogger.LogInfo($"✅ TEST: {postedGrns.Count} Posted GRNs found for supplier {supplier.CompanyName}", "Posted GRN count", "goods_return_dropdowns");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ TEST: Failed to load GRNs: {ex.Message}", ex, "GRN test error", "goods_return_dropdowns");
        }
        
        AppLogger.LogSeparator("TestLoadAllGrnsAsync Complete", "goods_return_dropdowns");
    }

    #endregion

    #region Validation

    private bool ValidateReturn()
    {
        var errors = new List<string>();

        // Header validation
        if (string.IsNullOrWhiteSpace(ReturnNo))
            errors.Add("Return No is required");

        if (!StoreId.HasValue)
            errors.Add("Store selection is required");

        if (ReturnDate > DateTime.Today)
            errors.Add("Return date cannot be in the future");

        // Items validation
        if (!ReturnItems.Any())
            errors.Add("At least one return item is required");

        var validItems = ReturnItems.Where(item => item.ProductId.HasValue && item.ReturnQuantity > 0).ToList();
        if (!validItems.Any())
            errors.Add("At least one item must have a product selected and return quantity greater than 0");

        // Check for duplicate products
        var productIds = validItems.Where(item => item.ProductId.HasValue).Select(item => item.ProductId.Value).ToList();
        if (productIds.Count != productIds.Distinct().Count())
            errors.Add("Duplicate products are not allowed");

        // Validate quantities
        foreach (var item in validItems)
        {
            if (item.ReturnQuantity > item.AvailableStock)
                errors.Add($"Return quantity cannot exceed available stock for {item.ProductName}");
        }

        ValidationMessage = string.Join(Environment.NewLine, errors);
        HasValidationErrors = errors.Any();
        return !errors.Any();
    }

    #endregion

    #region Save Operations

    private async Task<int> SaveReturnAsync(bool updateInventory)
    {
        try
        {
            AppLogger.LogInfo($"Saving goods return. Update inventory: {updateInventory}", "goods_return");
            AppLogger.LogInfo($"Return data - StoreId: {StoreId}, SupplierId: {SupplierId}, GrnId: {GrnId}, Status: {Status}", "goods_return");
            AppLogger.LogInfo($"GrnId value being saved: '{GrnId}' (Type: {GrnId?.GetType().Name ?? "null"})", "goods_return");
            AppLogger.LogInfo($"Return has {ReturnItems.Count} total items, {ReturnItems.Count(i => i.ProductId.HasValue && i.ReturnQuantity > 0)} valid items", "goods_return");

            AppLogger.LogInfo($"Creating DTO with GrnId: {GrnId}", "goods_return");
            
            var createDto = new CreateGoodsReturnDto
            {
                ReturnDate = ReturnDate,
                StoreId = StoreId!.Value,
                SupplierId = SupplierId.Value,
                ReferenceGrnId = GrnId,
                Remarks = Reason,
                Status = Status,
                Items = ReturnItems
                    .Where(item => item.ProductId.HasValue && item.ReturnQuantity > 0)
                    .Select(item => {
                        AppLogger.LogInfo($"Processing item - ProductId: {item.ProductId}, BatchNo: {item.BatchNo}, UomId: {item.UomId}, Qty: {item.ReturnQuantity}, Cost: {item.CostPrice}", "goods_return");
                        return new CreateGoodsReturnItemDto
                        {
                            ProductId = item.ProductId!.Value,
                            BatchNo = item.BatchNo,
                            ExpiryDate = item.ExpiryDate,
                            Quantity = item.ReturnQuantity,
                            UomId = item.UomId ?? 1, // Use selected UOM or default to 1
                            CostPrice = item.CostPrice,
                            Reason = item.Remarks
                        };
                    }).ToList()
            };

            AppLogger.LogInfo($"DTO created with ReferenceGrnId: {createDto.ReferenceGrnId}, Items count: {createDto.Items.Count}", "goods_return");

            GoodsReturnDto? result;
            if (_returnId.HasValue)
            {
                result = await _goodsReturnService.UpdateGoodsReturnAsync(_returnId.Value, createDto);
            }
            else
            {
                result = await _goodsReturnService.CreateGoodsReturnAsync(createDto);
            }

            if (result != null)
            {
                AppLogger.LogInfo($"Successfully saved goods return: {result.ReturnNo}", "goods_return");
                return result.Id;
            }

            throw new Exception("Failed to save goods return - no result returned");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to save goods return", ex, "Save operation", "goods_return");
            throw;
        }
    }

    #endregion

    #region Settings Event Handlers

    private void InitializeSettings()
    {
        CurrentZoomLevel = ConvertZoomLevelToDouble(_zoomService.CurrentZoomLevel);
        CurrentFlowDirection = ConvertLayoutDirectionToFlowDirection(_layoutDirectionService.CurrentDirection);
        CurrentCulture = _localizationService.CurrentCulture;
        UpdateLocalizedTitles();
    }

    private void OnThemeChanged(Theme newTheme) => OnPropertyChanged(nameof(ReturnNo));  // Just trigger a property update
    private void OnZoomChanged(ZoomLevel newZoom) => OnPropertyChanged(nameof(CurrentZoomLevel));
    private void OnLanguageChanged(SupportedLanguage newLanguage) => OnPropertyChanged(nameof(ReturnNo));  // Just trigger a property update
    private void OnPrimaryColorChanged(ColorOption newColor) => OnPropertyChanged(nameof(ReturnNo));  // Just trigger a property update
    private void OnDirectionChanged(LayoutDirection newDirection) => OnPropertyChanged(nameof(CurrentFlowDirection));
    private void OnDatabaseLanguageChanged(object? sender, string newLanguage) => UpdateLocalizedTitles();

    private void UpdateLocalizedTitles()
    {
        ReturnHeaderTitle = "Header";
        ReturnItemsTitle = "Items";
        SummaryTitle = "Review & Save";
        PageTitle = _returnId.HasValue ? "Edit Goods Return" : "Add Goods Return";
    }

    private double ConvertZoomLevelToDouble(ZoomLevel zoomLevel)
    {
        return (double)zoomLevel / 100.0;
    }

    private System.Windows.FlowDirection ConvertLayoutDirectionToFlowDirection(LayoutDirection direction)
    {
        return direction == LayoutDirection.RightToLeft 
            ? System.Windows.FlowDirection.RightToLeft 
            : System.Windows.FlowDirection.LeftToRight;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when a product is selected in an item row
    /// </summary>
    public async Task OnProductSelectedAsync(int productId, GoodsReturnItemViewModel item)
    {
        try
        {
            await LoadProductBatchesForItemAsync(productId, item);
            
            // Auto-select the first UOM if available
            if (UnitsOfMeasurement.Any())
            {
                item.UomId = UnitsOfMeasurement.First().Id;
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error handling product selection: {ex.Message}", ex, "Product selection", "goods_return");
        }
    }

    /// <summary>
    /// Load product batches for a specific item
    /// </summary>
    private async Task LoadProductBatchesForItemAsync(int productId, GoodsReturnItemViewModel item)
    {
        try
        {
            var batches = await _productBatchService.GetProductBatchesByProductIdAsync(productId);
            item.ProductBatches.Clear();
            foreach (var batch in batches)
            {
                item.ProductBatches.Add(batch);
            }
            AppLogger.LogInfo($"Loaded {batches.Count()} product batches for product {productId}", "goods_return");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error loading product batches for product {productId}: {ex.Message}", ex, "Loading product batches", "goods_return");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        // Unsubscribe from events
        _themeService.ThemeChanged -= OnThemeChanged;
        _zoomService.ZoomChanged -= OnZoomChanged;
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _colorSchemeService.PrimaryColorChanged -= OnPrimaryColorChanged;
        _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
    }

    #endregion
}

/// <summary>
/// ViewModel for individual goods return items
/// </summary>
public partial class GoodsReturnItemViewModel : ObservableObject
{
    private readonly AddGoodsReturnViewModel _parentViewModel;

    #region Observable Properties

    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private int? productId;

    [ObservableProperty]
    private long? uomId;

    [ObservableProperty]
    private string? batchNo;

    [ObservableProperty]
    private decimal grnQuantity;

    [ObservableProperty]
    private decimal availableStock;

    [ObservableProperty]
    private decimal returnQuantity;

    [ObservableProperty]
    private decimal costPrice;

    [ObservableProperty]
    private string remarks = string.Empty;

    [ObservableProperty]
    private DateTime? expiryDate;

    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    // Display properties
    public string ProductName => Products.FirstOrDefault(p => p.Id == ProductId)?.Name ?? "";
    public string UomName => Products.FirstOrDefault(p => p.Id == ProductId)?.ProductUnits?.FirstOrDefault()?.UnitName ?? "";
    public decimal LineTotal => ReturnQuantity * CostPrice;

    // Collections from parent
    public ObservableCollection<ProductDto> Products => _parentViewModel.Products;

    #endregion

    public GoodsReturnItemViewModel(AddGoodsReturnViewModel parentViewModel)
    {
        _parentViewModel = parentViewModel;
    }

    partial void OnProductIdChanged(int? value)
    {
        if (value.HasValue)
        {
            _ = _parentViewModel.OnProductSelectedAsync(value.Value, this);
            
            // Update available stock when product changes
            UpdateAvailableStock();
            
            AppLogger.LogInfo($"🏷️ Product selected: ID={value}, triggering batch and UOM loading", 
                "Product selection", "goods_return_items");
        }
        else
        {
            // Clear values when no product is selected
            AvailableStock = 0;
            GrnQuantity = 0;
            CostPrice = 0;
            BatchNo = null;
            UomId = null;
            
            AppLogger.LogInfo("🧹 Product deselected, clearing related fields", 
                "Product deselection", "goods_return_items");
        }
        
        OnPropertyChanged(nameof(ProductName));
        OnPropertyChanged(nameof(UomName));
    }

    partial void OnBatchNoChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var selectedBatch = ProductBatches.FirstOrDefault(b => b.BatchNo == value);
            if (selectedBatch != null)
            {
                // Set expiry date
                ExpiryDate = selectedBatch.ExpiryDate;
                
                // Set GRN Quantity from batch quantity
                GrnQuantity = selectedBatch.Quantity;
                
                // Set Cost Price from batch cost price
                CostPrice = selectedBatch.CostPrice ?? 0m;
                
                AppLogger.LogInfo($"🔄 Batch '{value}' selected - GRN Qty: {GrnQuantity}, Cost Price: {CostPrice:C}, Expiry: {ExpiryDate:yyyy-MM-dd}", 
                    "Batch selection updated", "goods_return_items");
            }
        }
        
        // Update available stock from product
        UpdateAvailableStock();
    }
    
    /// <summary>
    /// Update available stock for the selected product
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
            var selectedProduct = Products.FirstOrDefault(p => p.Id == ProductId.Value);
            if (selectedProduct != null)
            {
                // Use the product's initial stock as available stock
                AvailableStock = selectedProduct.InitialStock;
                AppLogger.LogInfo($"📊 Updated available stock for product '{selectedProduct.Name}': {AvailableStock}", 
                    "Available stock updated", "goods_return_items");
            }
            else
            {
                AvailableStock = 0;
                AppLogger.LogWarning($"⚠️ Product with ID {ProductId.Value} not found in products collection", 
                    "Product lookup failed", "goods_return_items");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ Error updating available stock for product {ProductId}: {ex.Message}", ex, 
                "Available stock update error", "goods_return_items");
            AvailableStock = 0;
        }
    }

    partial void OnReturnQuantityChanged(decimal value)
    {
        OnPropertyChanged(nameof(LineTotal));
        _parentViewModel.UpdateTotalReturnAmount();
    }

    partial void OnCostPriceChanged(decimal value)
    {
        OnPropertyChanged(nameof(LineTotal));
        _parentViewModel.UpdateTotalReturnAmount();
    }


}