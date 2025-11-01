using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using System.Globalization;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for adding new GRN (Goods Received Note) with comprehensive form validation and full settings integration
/// </summary>
public partial class AddGrnViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IGoodsReceivedService _goodsReceivedService;
    private readonly ISupplierService _supplierService;
    private readonly IStoreService _storeService;
    private readonly IProductService _productService;
    private readonly IUomService _uomService;
    private readonly IProductBatchService _productBatchService;
    private readonly Action? _navigateBack;
    
    // Settings services
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    private readonly IActiveCurrencyService _activeCurrencyService;
    
    #endregion

    #region Observable Properties

    // GRN Header Properties
    [ObservableProperty]
    private string grnNo = string.Empty;

    [ObservableProperty]
    private string status = "Pending";

    [ObservableProperty]
    private long? supplierId;

    [ObservableProperty]
    private int? storeId;

    [ObservableProperty]
    private string invoiceNo = string.Empty;

    [ObservableProperty]
    private DateTime invoiceDate = DateTime.Today;

    [ObservableProperty]
    private DateTime receivedDate = DateTime.Today;

    [ObservableProperty]
    private string remarks = string.Empty;

    // Collections
    [ObservableProperty]
    private ObservableCollection<SupplierDto> suppliers = new();

    [ObservableProperty]
    private ObservableCollection<StoreDto> stores = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> products = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> unitsOfMeasurement = new();

    // GRN Items
    [ObservableProperty]
    private ObservableCollection<GrnItemViewModel> grnItems = new();

    [ObservableProperty]
    private GrnItemViewModel? selectedGrnItem;

    // Summary Properties
    [ObservableProperty]
    private int totalItems = 0;

    [ObservableProperty]
    private decimal totalQuantity = 0;

    [ObservableProperty]
    private decimal totalAmount = 0;

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
    private double currentZoomLevel = 1.0;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private string currentTheme = "Light";

    [ObservableProperty]
    private string currentLanguage = "en";

    #endregion

    #region Localized Text Properties

    // Header Text
    [ObservableProperty]
    private string addGrnTitle = "Add Goods Received Note";

    [ObservableProperty]
    private string backButtonText = "Back";

    // Section Titles
    [ObservableProperty]
    private string grnHeaderTitle = "GRN Header";

    [ObservableProperty]
    private string grnItemsTitle = "GRN Items";

    [ObservableProperty]
    private string summaryTitle = "Summary";

    // Form Labels - GRN Header
    [ObservableProperty]
    private string grnNoLabel = "GRN No:";

    [ObservableProperty]
    private string statusLabel = "Status:";

    [ObservableProperty]
    private string supplierLabel = "Supplier:";

    [ObservableProperty]
    private string storeLabel = "Store:";

    [ObservableProperty]
    private string invoiceNoLabel = "Invoice No:";

    [ObservableProperty]
    private string invoiceDateLabel = "Invoice Date:";

    [ObservableProperty]
    private string receivedDateLabel = "Received Date:";

    [ObservableProperty]
    private string remarksLabel = "Remarks:";

    // Form Labels - GRN Items
    [ObservableProperty]
    private string addProductButtonText = "+ Add Product";

    [ObservableProperty]
    private string productLabel = "Product";

    [ObservableProperty]
    private string quantityLabel = "Quantity";

    [ObservableProperty]
    private string uomLabel = "UOM";

    [ObservableProperty]
    private string costPriceLabel = "Cost Price";

    [ObservableProperty]
    private string batchNoLabel = "Batch No";

    [ObservableProperty]
    private string manufactureDateLabel = "Mfg Date";

    [ObservableProperty]
    private string expiryDateLabel = "Expiry Date";

    [ObservableProperty]
    private string lineTotalLabel = "Line Total";

    [ObservableProperty]
    private string actionsLabel = "Actions";

    // Form Labels - Summary
    [ObservableProperty]
    private string totalItemsLabel = "Total Items:";

    [ObservableProperty]
    private string totalQuantityLabel = "Total Quantity:";

    [ObservableProperty]
    private string totalAmountLabel = "Total Amount:";

    // Button Text
    [ObservableProperty]
    private string saveDraftButtonText = "Save Draft";

    [ObservableProperty]
    private string postGrnButtonText = "Post GRN";

    [ObservableProperty]
    private string cancelButtonText = "Cancel";

    // Messages
    [ObservableProperty]
    private string noItemsMessage = "No items added to GRN";

    [ObservableProperty]
    private string addItemsInstruction = "Click 'Add Product' to start adding items";

    #endregion

    #region Commands

    [RelayCommand]
    private async Task Cancel()
    {
        _navigateBack?.Invoke();
    }

    [RelayCommand]
    private void AddGrnItem()
    {
        try
        {
            // Generate unique dates for each item to avoid batch date conflicts
            var currentTime = DateTime.Now;
            var newItem = new GrnItemViewModel
            {
                Id = 0,
                ProductId = 0,
                Quantity = 1,
                UomId = 1,
                CostPrice = 0,
                BatchNo = string.Empty,
                ManufactureDate = null, // No default - user must enter
                ExpiryDate = null, // No default - user must enter  
                LineTotal = 0
            };

            // Wire up event to recalculate summary when line total changes
            newItem.LineTotalChangedEvent = () => CalculateSummary();

            AppLogger.LogInfo($"New GRN item added (dates must be entered by user)", 
                $"ItemId: {newItem.Id}, MfgDate: {(newItem.ManufactureDate?.ToString("yyyy-MM-dd") ?? "NULL")}, ExpDate: {(newItem.ExpiryDate?.ToString("yyyy-MM-dd") ?? "NULL")}", "grn_item_dates");

            GrnItems.Add(newItem);
            CalculateSummary();
        }
        catch (Exception ex)
        {
            ShowErrorAsync($"Failed to add item: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RemoveGrnItem(GrnItemViewModel item)
    {
        try
        {
            if (item != null)
            {
                GrnItems.Remove(item);
                CalculateSummary();
            }
        }
        catch (Exception ex)
        {
            ShowErrorAsync($"Failed to remove item: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SaveDraft()
    {
        try
        {
            IsLoading = true;
            StatusMessage = IsEditMode ? "Updating draft..." : "Saving draft...";

            if (!ValidateForm())
            {
                return;
            }

            if (IsEditMode && EditingGrnId.HasValue)
            {
                // Update existing GRN in draft mode
                var updateDto = CreateUpdateGrnDto();
                updateDto.Status = "Draft"; // ✅ Keep as Draft during editing

                AppLogger.LogInfo($"Updating GRN draft with {updateDto.Items?.Count ?? 0} items", 
                    $"GRN ID: {EditingGrnId.Value}, GRN No: {updateDto.GrnNo}", "grn_save");
                
                var result = await _goodsReceivedService.UpdateAsync(updateDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"GRN draft updated successfully", 
                        $"GRN No: {result.GrnNo}, Status: {result.Status}", "grn_save");
                    ShowSuccessAsync("GRN updated successfully!");
                    _navigateBack?.Invoke();
                }
            }
            else
            {
            // Create new GRN
            var createDto = CreateGrnDto();
            createDto.Status = "Draft";

            // Log date information for debugging
            foreach (var item in createDto.Items)
            {
                AppLogger.LogInfo($"Creating GRN item with dates", 
                    $"ProductId: {item.ProductId}, BatchNo: '{item.BatchNo}', MfgDate: {item.ManufactureDate:yyyy-MM-dd}, ExpDate: {item.ExpiryDate:yyyy-MM-dd}", "grn_dates_flow");
            }                var result = await _goodsReceivedService.CreateAsync(createDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"GRN draft created successfully", 
                        $"GRN No: {result.GrnNo}, Status: Draft", "grn_save");
                    ShowSuccessAsync("GRN saved as draft successfully!");
                    _navigateBack?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to save GRN draft", ex, 
                $"EditMode: {IsEditMode}, GRN ID: {EditingGrnId}", "grn_save");
            ShowErrorAsync($"Failed to save draft: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    [RelayCommand]
    private async Task PostGrn()
    {
        try
        {
            IsLoading = true;
            StatusMessage = IsEditMode ? "Updating and posting GRN..." : "Posting GRN...";

            if (!ValidateForm())
            {
                return;
            }

            if (GrnItems.Count == 0)
            {
                ShowErrorAsync("Cannot post GRN without items. Please add at least one item.");
                return;
            }

            if (IsEditMode && EditingGrnId.HasValue)
            {
                // Update existing GRN and prepare for posting
                var updateDto = CreateUpdateGrnDto();
                updateDto.Status = "Draft"; // ✅ Keep as Draft first for transactional posting

                AppLogger.LogInfo($"Updating GRN before posting with {updateDto.Items?.Count ?? 0} items", 
                    $"GRN ID: {EditingGrnId.Value}, GRN No: {updateDto.GrnNo}", "grn_post");
                
                var result = await _goodsReceivedService.UpdateAsync(updateDto);
                
                if (result != null)
                {
                    // Now post the updated GRN using the transactional method
                    AppLogger.LogInfo($"GRN updated, now posting transactionally", 
                        $"GRN ID: {EditingGrnId.Value}, GRN No: {result.GrnNo}", "grn_post");
                    
                    var postResult = await _goodsReceivedService.PostGrnAsync((int)EditingGrnId.Value);
                    
                    if (postResult)
                    {
                        AppLogger.LogInfo($"GRN updated and posted successfully", 
                            $"GRN No: {result.GrnNo}, Status: Posted", "grn_post");
                        ShowSuccessAsync("GRN updated and posted successfully!");
                        _navigateBack?.Invoke();
                    }
                    else
                    {
                        AppLogger.LogError("Failed to post updated GRN", null, 
                            $"GRN ID: {EditingGrnId.Value}", "grn_post");
                        ShowErrorAsync("Failed to post GRN. Please check the logs for details.");
                    }
                }
                return;
            }
            else
            {
                // Create new GRN as Draft first, then post it
                var createDto = CreateGrnDto();
                createDto.Status = "Draft"; // ✅ Create as Draft first

                AppLogger.LogInfo($"Creating GRN as Draft with {createDto.Items?.Count ?? 0} items", 
                    $"GRN No: {createDto.GrnNo}", "grn_post");

                var result = await _goodsReceivedService.CreateAsync(createDto);
                
                if (result != null)
                {
                    AppLogger.LogInfo($"GRN created successfully, now posting to update stock", 
                        $"GRN ID: {result.Id}, GRN No: {result.GrnNo}, Items: {result.Items?.Count ?? 0}", "grn_post");

                    // Now post the GRN to create ProductBatches and update stock
                    await _goodsReceivedService.PostGrnAsync(result.Id);
                    
                    AppLogger.LogInfo($"GRN posted successfully with stock updates", 
                        $"GRN No: {result.GrnNo}, Final Status: Posted", "grn_post");
                    
                    ShowSuccessAsync("GRN posted successfully! Stock has been updated and product batches created.");
                    _navigateBack?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            ShowErrorAsync($"Failed to post GRN: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
        }
    }

    #endregion

    #region Constructor

    public AddGrnViewModel(
        IGoodsReceivedService goodsReceivedService,
        ISupplierService supplierService,
        IStoreService storeService,
        IProductService productService,
        IUomService uomService,
        IProductBatchService productBatchService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        IActiveCurrencyService activeCurrencyService,
        Action? navigateBack = null)
    {
        AppLogger.LogSeparator("AddGrnViewModel Constructor", "grn_viewmodel_lifecycle");
        AppLogger.LogInfo("🎯 Creating AddGrnViewModel instance", "ViewModel construction started", "grn_viewmodel_lifecycle");
        
        _goodsReceivedService = goodsReceivedService ?? throw new ArgumentNullException(nameof(goodsReceivedService));
        _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
        _navigateBack = navigateBack;
        
        AppLogger.LogInfo("✅ All services injected successfully", "Service injection completed", "grn_viewmodel_lifecycle");

        // Subscribe to settings changes
        AppLogger.LogInfo("📡 Subscribing to settings changes", "Event subscription", "grn_viewmodel_lifecycle");
        SubscribeToSettingsChanges();

        // Initialize
        AppLogger.LogInfo("🚀 Starting async initialization", "Initialization trigger", "grn_viewmodel_lifecycle");
        _ = InitializeAsync();
        
        AppLogger.LogInfo("✅ AddGrnViewModel constructor completed", "ViewModel construction finished", "grn_viewmodel_lifecycle");
    }

    #endregion

    #region Private Methods

    private async Task InitializeAsync()
    {
        AppLogger.LogSeparator("AddGrnViewModel InitializeAsync", "grn_initialization");
        AppLogger.LogInfo("🚀 Starting AddGrn screen initialization", "AddGrn screen loaded", "grn_initialization");
        
        try
        {
            IsLoading = true;
            StatusMessage = "Loading...";
            AppLogger.LogInfo("⏳ Setting loading state and status message", "UI state update", "grn_initialization");

            // Generate GRN number
            AppLogger.LogInfo("🔢 Generating GRN number", "GRN number generation", "grn_initialization");
            var startTimeGrn = DateTime.Now;
            GrnNo = await _goodsReceivedService.GenerateGrnNoAsync();
            var durationGrn = DateTime.Now - startTimeGrn;
            AppLogger.LogPerformance("GenerateGrnNoAsync", durationGrn, "GRN number generation completed", "grn_initialization");
            AppLogger.LogInfo($"✅ Generated GRN number: {GrnNo}", "GRN number ready", "grn_initialization");

            // Ensure translation keywords are in database
            AppLogger.LogInfo("🌐 Ensuring AddGrn translation keywords are in database", "Translation setup", "grn_initialization");
            FileLogger.Log("🌐 Ensuring AddGrn translation keywords are in database");
            var startTimeTranslations = DateTime.Now;
            await AddGrnTranslations.EnsureTranslationKeywordsAsync(_databaseLocalizationService);
            var durationTranslations = DateTime.Now - startTimeTranslations;
            AppLogger.LogPerformance("EnsureTranslationKeywordsAsync", durationTranslations, "Translation keywords ensured", "grn_initialization");
            FileLogger.Log("✅ AddGrn translation keywords ensured");

            // Load data in parallel
            AppLogger.LogInfo("📊 Starting parallel data loading tasks", "Dropdown data loading", "grn_initialization");
            var startTimeParallel = DateTime.Now;
            
            var loadTasks = new[]
            {
                LoadSuppliersAsync(),
                LoadStoresAsync(),
                LoadProductsAsync(),
                LoadUnitsOfMeasurementAsync(),
                LoadLocalizedTextAsync()
            };

            await Task.WhenAll(loadTasks);
            
            var durationParallel = DateTime.Now - startTimeParallel;
            AppLogger.LogPerformance("Parallel data loading", durationParallel, "All dropdown data loaded", "grn_initialization");

            // Apply current settings
            AppLogger.LogInfo("⚙️ Applying current UI settings", "Settings application", "grn_initialization");
            await ApplyCurrentSettingsAsync();
            
            AppLogger.LogInfo("✅ AddGrn initialization completed successfully", "Initialization successful", "grn_initialization");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ AddGrn initialization failed: {ex.Message}", ex, "Critical initialization error", "grn_initialization");
            ShowErrorAsync($"Failed to initialize: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = string.Empty;
            AppLogger.LogInfo("🏁 Setting final loading state to false", "Cleanup loading state", "grn_initialization");
        }
        
        AppLogger.LogSeparator("AddGrnViewModel InitializeAsync Complete", "grn_initialization");
    }

    private async Task LoadSuppliersAsync()
    {
        AppLogger.LogSeparator("LoadSuppliersAsync", "grn_dropdown_loading");
        AppLogger.LogInfo("🔄 Starting supplier dropdown loading", "GRN form initialization", "grn_dropdown_loading");
        
        try
        {
            // Log service availability
            if (_supplierService == null)
            {
                AppLogger.LogError("❌ SupplierService is null", null, "Service injection issue", "grn_dropdown_loading");
                return;
            }
            
            AppLogger.LogInfo("✅ SupplierService is available, calling GetAllAsync()", "Service ready", "grn_dropdown_loading");
            var startTime = DateTime.Now;
            
            var suppliers = await _supplierService.GetAllAsync();
            var duration = DateTime.Now - startTime;
            
            AppLogger.LogPerformance("SupplierService.GetAllAsync", duration, "Database query completed", "grn_dropdown_loading");
            
            if (suppliers == null)
            {
                AppLogger.LogWarning("⚠️ SupplierService.GetAllAsync returned null", "Empty result from service", "grn_dropdown_loading");
                return;
            }
            
            var suppliersList = suppliers.ToList();
            AppLogger.LogInfo($"📊 Retrieved {suppliersList.Count} suppliers from service", "Data retrieval successful", "grn_dropdown_loading");
            
            // Log existing collection state
            AppLogger.LogDebug($"🗂️ Current Suppliers collection count: {Suppliers.Count}", "Pre-clear state", "grn_dropdown_loading");
            
            Suppliers.Clear();
            AppLogger.LogDebug("🧹 Suppliers collection cleared", "Collection reset", "grn_dropdown_loading");
            
            foreach (var supplier in suppliersList)
            {
                Suppliers.Add(supplier);
                AppLogger.LogDebug($"➕ Added supplier: ID={supplier.SupplierId}, Company='{supplier.CompanyName}', IsActive={supplier.IsActive}", 
                    "Supplier added to dropdown", "grn_dropdown_loading");
            }
            
            AppLogger.LogInfo($"✅ Supplier dropdown loaded successfully with {Suppliers.Count} items", 
                "Dropdown population completed", "grn_dropdown_loading");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ Failed to load suppliers: {ex.Message}", ex, "Supplier dropdown loading error", "grn_dropdown_loading");
            FileLogger.Log($"❌ Failed to load suppliers: {ex.Message}");
        }
        
        AppLogger.LogSeparator("LoadSuppliersAsync Complete", "grn_dropdown_loading");
    }

    private async Task LoadStoresAsync()
    {
        AppLogger.LogSeparator("LoadStoresAsync", "grn_dropdown_loading");
        AppLogger.LogInfo("🔄 Starting stores dropdown loading", "GRN form initialization", "grn_dropdown_loading");
        
        try
        {
            // Log service availability
            if (_storeService == null)
            {
                AppLogger.LogError("❌ StoreService is null", null, "Service injection issue", "grn_dropdown_loading");
                return;
            }
            
            AppLogger.LogInfo("✅ StoreService is available, calling GetAllAsync()", "Service ready", "grn_dropdown_loading");
            var startTime = DateTime.Now;
            
            var stores = await _storeService.GetAllAsync();
            var duration = DateTime.Now - startTime;
            
            AppLogger.LogPerformance("StoreService.GetAllAsync", duration, "Database query completed", "grn_dropdown_loading");
            
            if (stores == null)
            {
                AppLogger.LogWarning("⚠️ StoreService.GetAllAsync returned null", "Empty result from service", "grn_dropdown_loading");
                return;
            }
            
            var storesList = stores.ToList();
            AppLogger.LogInfo($"📊 Retrieved {storesList.Count} stores from service", "Data retrieval successful", "grn_dropdown_loading");
            
            // Log existing collection state
            AppLogger.LogDebug($"🗂️ Current Stores collection count: {Stores.Count}", "Pre-clear state", "grn_dropdown_loading");
            
            Stores.Clear();
            AppLogger.LogDebug("🧹 Stores collection cleared", "Collection reset", "grn_dropdown_loading");
            
            foreach (var store in storesList)
            {
                Stores.Add(store);
                AppLogger.LogDebug($"➕ Added store: ID={store.Id}, Name='{store.Name}', IsActive={store.IsActive}", 
                    "Store added to dropdown", "grn_dropdown_loading");
            }
            
            AppLogger.LogInfo($"✅ Stores dropdown loaded successfully with {Stores.Count} items", 
                "Dropdown population completed", "grn_dropdown_loading");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ Failed to load stores: {ex.Message}", ex, "Stores dropdown loading error", "grn_dropdown_loading");
            FileLogger.Log($"❌ Failed to load stores: {ex.Message}");
        }
        
        AppLogger.LogSeparator("LoadStoresAsync Complete", "grn_dropdown_loading");
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Failed to load products: {ex.Message}");
        }
    }

    private async Task LoadUnitsOfMeasurementAsync()
    {
        AppLogger.LogSeparator("LoadUnitsOfMeasurementAsync", "grn_dropdown_loading");
        AppLogger.LogInfo("🔄 Starting UOM (Units of Measurement) dropdown loading", "GRN form initialization", "grn_dropdown_loading");
        
        try
        {
            // Log service availability
            if (_uomService == null)
            {
                AppLogger.LogError("❌ UomService is null", null, "Service injection issue", "grn_dropdown_loading");
                return;
            }
            
            AppLogger.LogInfo("✅ UomService is available, calling GetAllAsync()", "Service ready", "grn_dropdown_loading");
            var startTime = DateTime.Now;
            
            var units = await _uomService.GetAllAsync();
            var duration = DateTime.Now - startTime;
            
            AppLogger.LogPerformance("UomService.GetAllAsync", duration, "Database query completed", "grn_dropdown_loading");
            
            if (units == null)
            {
                AppLogger.LogWarning("⚠️ UomService.GetAllAsync returned null", "Empty result from service", "grn_dropdown_loading");
                return;
            }
            
            var unitsList = units.ToList();
            AppLogger.LogInfo($"📊 Retrieved {unitsList.Count} units of measurement from service", "Data retrieval successful", "grn_dropdown_loading");
            
            // Log existing collection state
            AppLogger.LogDebug($"🗂️ Current UnitsOfMeasurement collection count: {UnitsOfMeasurement.Count}", "Pre-clear state", "grn_dropdown_loading");
            
            UnitsOfMeasurement.Clear();
            AppLogger.LogDebug("🧹 UnitsOfMeasurement collection cleared", "Collection reset", "grn_dropdown_loading");
            
            foreach (var unit in unitsList)
            {
                UnitsOfMeasurement.Add(unit);
                AppLogger.LogDebug($"➕ Added UOM: ID={unit.Id}, Name='{unit.Name}', Abbreviation='{unit.Abbreviation}', IsActive={unit.IsActive}", 
                    "UOM added to dropdown", "grn_dropdown_loading");
            }
            
            AppLogger.LogInfo($"✅ UOM dropdown loaded successfully with {UnitsOfMeasurement.Count} items", 
                "Dropdown population completed", "grn_dropdown_loading");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"❌ Failed to load units of measurement: {ex.Message}", ex, "UOM dropdown loading error", "grn_dropdown_loading");
            FileLogger.Log($"❌ Failed to load units of measurement: {ex.Message}");
        }
        
        AppLogger.LogSeparator("LoadUnitsOfMeasurementAsync Complete", "grn_dropdown_loading");
    }

    private async Task LoadLocalizedTextAsync()
    {
        try
        {
            if (_databaseLocalizationService == null) return;

            FileLogger.Log("🌐 Loading AddGrn translations");

            // Header Text
            AddGrnTitle = await GetTranslationAsync("grn.add_title", "Add Goods Received Note");
            BackButtonText = await GetTranslationAsync("common.back", "Back");

            // Section Titles
            GrnHeaderTitle = await GetTranslationAsync("grn.header_section", "GRN Header");
            GrnItemsTitle = await GetTranslationAsync("grn.items_section", "GRN Items");
            SummaryTitle = await GetTranslationAsync("grn.summary_section", "Summary");

            // Form Labels
            GrnNoLabel = await GetTranslationAsync("grn.grn_no_label", "GRN No:");
            StatusLabel = await GetTranslationAsync("grn.status_label", "Status:");
            SupplierLabel = await GetTranslationAsync("grn.supplier_label", "Supplier:");
            StoreLabel = await GetTranslationAsync("grn.store_label", "Store:");
            InvoiceNoLabel = await GetTranslationAsync("grn.invoice_no_label", "Invoice No:");
            InvoiceDateLabel = await GetTranslationAsync("grn.invoice_date_label", "Invoice Date:");
            ReceivedDateLabel = await GetTranslationAsync("grn.received_date_label", "Received Date:");
            RemarksLabel = await GetTranslationAsync("grn.remarks_label", "Remarks:");

            // Button Text
            SaveDraftButtonText = await GetTranslationAsync("grn.save_draft_button", "Save Draft");
            PostGrnButtonText = await GetTranslationAsync("grn.post_grn_button", "Post GRN");
            CancelButtonText = await GetTranslationAsync("common.cancel", "Cancel");

            FileLogger.Log("✅ AddGrn translations loaded successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Failed to load localized text: {ex.Message}");
            // Use default English text if translation fails
            AddGrnTitle = "Add Goods Received Note";
            BackButtonText = "Back";
            GrnHeaderTitle = "GRN Header";
            GrnItemsTitle = "GRN Items";
            SummaryTitle = "Summary";
        }
    }

    private void CalculateSummary()
    {
        TotalItems = GrnItems.Count;
        TotalQuantity = GrnItems.Sum(item => item.Quantity);
        TotalAmount = GrnItems.Sum(item => item.LineTotal);
    }

    private bool ValidateForm()
    {
        HasValidationErrors = false;
        ValidationMessage = string.Empty;

        var errors = new List<string>();

        // Validate GRN Header
        if (string.IsNullOrWhiteSpace(GrnNo))
            errors.Add("GRN No is required");

        if (!SupplierId.HasValue || SupplierId.Value <= 0)
            errors.Add("Supplier is required");

        if (!StoreId.HasValue || StoreId.Value <= 0)
            errors.Add("Store is required");

        if (ReceivedDate == default)
            errors.Add("Received Date is required");

        // Validate GRN Items for Post operation
        if (GrnItems.Count == 0)
        {
            errors.Add("At least one item is required");
        }
        else
        {
            for (int i = 0; i < GrnItems.Count; i++)
            {
                var item = GrnItems[i];
                if (item.ProductId <= 0)
                    errors.Add($"Product is required for item {i + 1}");
                if (item.Quantity <= 0)
                    errors.Add($"Quantity must be greater than 0 for item {i + 1}");
                if (item.CostPrice <= 0)
                    errors.Add($"Cost Price must be greater than 0 for item {i + 1}");
                if (item.UomId <= 0)
                    errors.Add($"UOM is required for item {i + 1}");
                if (!item.ManufactureDate.HasValue)
                    errors.Add($"Manufacture Date is required for item {i + 1}");
                if (!item.ExpiryDate.HasValue)
                    errors.Add($"Expiry Date is required for item {i + 1}");
                else if (item.ManufactureDate.HasValue && item.ExpiryDate.HasValue && 
                         item.ExpiryDate.Value <= item.ManufactureDate.Value)
                    errors.Add($"Expiry Date must be after Manufacture Date for item {i + 1}");
            }
        }

        if (errors.Any())
        {
            HasValidationErrors = true;
            ValidationMessage = string.Join("\n", errors);
            return false;
        }

        return true;
    }

    private CreateGoodsReceivedDto CreateGrnDto()
    {
        return new CreateGoodsReceivedDto
        {
            GrnNo = GrnNo,
            SupplierId = SupplierId ?? 0,
            StoreId = StoreId ?? 0,
            InvoiceNo = InvoiceNo,
            InvoiceDate = InvoiceDate,
            ReceivedDate = ReceivedDate,
            TotalAmount = TotalAmount,
            Remarks = Remarks,
            Status = Status,
            Items = GrnItems.Select(item => {
                AppLogger.LogInfo($"Mapping GrnItem to CreateGoodsReceivedItemDto", 
                    $"ProductId: {item.ProductId}, BatchNo: '{item.BatchNo}', MfgDate: {item.ManufactureDate:yyyy-MM-dd}, ExpDate: {item.ExpiryDate:yyyy-MM-dd}", "grn_dto_mapping");
                
                return new CreateGoodsReceivedItemDto
                {
                    GrnId = 0, // Will be set by service
                    ProductId = item.ProductId,
                    BatchNo = item.BatchNo,
                    ManufactureDate = item.ManufactureDate,
                    ExpiryDate = item.ExpiryDate,
                    Quantity = item.Quantity,
                    UomId = item.UomId,
                    CostPrice = item.CostPrice,
                    LandedCost = item.LandedCost
                };
            }).ToList()
        };
    }

    private UpdateGoodsReceivedDto CreateUpdateGrnDto()
    {
        if (!EditingGrnId.HasValue)
            throw new InvalidOperationException("Cannot create update DTO without GRN ID");

        return new UpdateGoodsReceivedDto
        {
            Id = (int)EditingGrnId.Value,
            GrnNo = GrnNo,
            SupplierId = SupplierId ?? 0,
            StoreId = StoreId ?? 0,
            InvoiceNo = InvoiceNo,
            InvoiceDate = InvoiceDate,
            ReceivedDate = ReceivedDate,
            TotalAmount = TotalAmount,
            Remarks = Remarks,
            Status = Status,
            Items = GrnItems.Select(item => new UpdateGoodsReceivedItemDto
            {
                Id = item.Id, // Use existing item ID for updates
                GrnId = (int)EditingGrnId.Value,
                ProductId = item.ProductId,
                BatchNo = item.BatchNo,
                ManufactureDate = item.ManufactureDate,
                ExpiryDate = item.ExpiryDate,
                Quantity = item.Quantity,
                UomId = item.UomId,
                CostPrice = item.CostPrice,
                LandedCost = item.LandedCost
            }).ToList()
        };
    }

    private void SubscribeToSettingsChanges()
    {
        if (_themeService != null)
            _themeService.ThemeChanged += OnThemeChanged;

        if (_zoomService != null)
            _zoomService.ZoomChanged += OnZoomChanged;

        if (_localizationService != null)
            _localizationService.LanguageChanged += OnLanguageChanged;

        if (_layoutDirectionService != null)
            _layoutDirectionService.DirectionChanged += OnLayoutDirectionChanged;
    }

    private async Task ApplyCurrentSettingsAsync()
    {
        try
        {
            CurrentZoomLevel = _zoomService?.CurrentZoomScale ?? 1.0;
            CurrentTheme = _themeService?.CurrentTheme.ToString() ?? "Light";
            CurrentLanguage = _localizationService?.CurrentLanguage.ToString() ?? "en";
            CurrentFlowDirection = _layoutDirectionService?.CurrentDirection == ChronoPos.Desktop.Services.LayoutDirection.RightToLeft 
                ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Failed to apply current settings: {ex.Message}");
        }
    }

    private void OnThemeChanged(Theme newTheme)
    {
        CurrentTheme = newTheme.ToString();
    }

    private void OnZoomChanged(ZoomLevel newZoomLevel)
    {
        CurrentZoomLevel = (double)newZoomLevel / 100.0;
    }

    private async void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        CurrentLanguage = newLanguage.ToString();
        await LoadLocalizedTextAsync();
    }

    private void OnLayoutDirectionChanged(ChronoPos.Desktop.Services.LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == ChronoPos.Desktop.Services.LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void ShowErrorAsync(string message)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            new MessageDialog("Error", message, MessageDialog.MessageType.Error).ShowDialog();
        });
    }

    private void ShowSuccessAsync(string message)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            new MessageDialog("Success", message, MessageDialog.MessageType.Success).ShowDialog();
        });
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_themeService != null)
            _themeService.ThemeChanged -= OnThemeChanged;

        if (_zoomService != null)
            _zoomService.ZoomChanged -= OnZoomChanged;

        if (_localizationService != null)
            _localizationService.LanguageChanged -= OnLanguageChanged;

        if (_layoutDirectionService != null)
            _layoutDirectionService.DirectionChanged -= OnLayoutDirectionChanged;
    }

    private async Task<string> GetTranslationAsync(string key, string fallback)
    {
        try
        {
            FileLogger.Log($"🔍 [GetTranslation] Looking for key: '{key}', fallback: '{fallback}'");
            var translation = await _databaseLocalizationService.GetTranslationAsync(key);
            if (translation != null && translation != key)
            {
                FileLogger.Log($"✅ [GetTranslation] Found translation for '{key}': '{translation}'");
                return translation;
            }
            else
            {
                FileLogger.Log($"⚠️ [GetTranslation] No translation found for '{key}', using fallback: '{fallback}'");
                return fallback;
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ [GetTranslation] Error getting translation for '{key}': {ex.Message}");
            return fallback;
        }
    }

    #endregion

    #region Property Change Handlers

    partial void OnSupplierIdChanged(long? value)
    {
        AppLogger.LogDebug($"🔄 SupplierId changed to: {value}", 
            "Supplier selection changed", "grn_supplier_selection");
        
        if (value.HasValue && value.Value > 0)
        {
            var selectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierId == value.Value);
            if (selectedSupplier != null)
            {
                AppLogger.LogInfo($"✅ Supplier selected: ID={selectedSupplier.SupplierId}, Name='{selectedSupplier.DisplayName}'", 
                    "Valid supplier selection", "grn_supplier_selection");
            }
            else
            {
                AppLogger.LogWarning($"⚠️ Selected supplier ID {value.Value} not found in Suppliers collection", 
                    "Supplier not found", "grn_supplier_selection");
            }
        }
        else
        {
            AppLogger.LogDebug($"🔄 Supplier selection cleared (value: {value})", 
                "Supplier deselected", "grn_supplier_selection");
        }
    }

    #endregion

    #region Edit Mode Support

    /// <summary>
    /// Load GRN data for editing mode
    /// </summary>
    public async Task LoadForEditAsync(long grnId)
    {
        try
        {
            AppLogger.LogSeparator($"LOADING GRN FOR EDIT - ID: {grnId}", "grn_edit_mode");
            AppLogger.LogInfo($"Starting LoadForEditAsync", $"GRN ID: {grnId}", "grn_edit_mode");
            
            IsLoading = true;
            StatusMessage = "Loading GRN for editing...";

            // Load GRN details from service
            if (_goodsReceivedService != null)
            {
                var grn = await _goodsReceivedService.GetByIdAsync((int)grnId);
                
                if (grn != null)
                {
                    AppLogger.LogInfo($"GRN loaded successfully", 
                        $"GRN No: {grn.GrnNo}, Status: {grn.Status}, Supplier: {grn.SupplierName}", "grn_edit_mode");
                    
                    // Populate form fields with GRN data
                    GrnNo = grn.GrnNo;
                    SupplierId = grn.SupplierId;
                    StoreId = grn.StoreId;
                    ReceivedDate = grn.ReceivedDate;
                    InvoiceNo = grn.InvoiceNo ?? string.Empty;
                    InvoiceDate = grn.InvoiceDate ?? DateTime.Today;
                    Remarks = grn.Remarks ?? string.Empty;
                    Status = grn.Status;
                    
                    // Load GRN items
                    GrnItems.Clear();
                    if (grn.Items?.Any() == true)
                    {
                        foreach (var item in grn.Items)
                        {
                            var grnItem = new GrnItemViewModel
                            {
                                Id = item.Id,
                                ProductId = item.ProductId,
                                ProductName = item.ProductName ?? string.Empty,
                                Quantity = item.Quantity,
                                UomId = item.UomId,
                                UomName = item.UomName ?? string.Empty,
                                CostPrice = item.CostPrice,
                                BatchNo = item.BatchNo ?? string.Empty,
                                ManufactureDate = item.ManufactureDate,
                                ExpiryDate = item.ExpiryDate,
                                LandedCost = item.LandedCost
                            };

                            // Wire up event to recalculate summary when line total changes
                            grnItem.LineTotalChangedEvent = () => CalculateSummary();
                            
                            GrnItems.Add(grnItem);
                            AppLogger.LogDebug($"Added GRN item", 
                                $"Product: {item.ProductName}, Qty: {item.Quantity}, Cost: {_activeCurrencyService.FormatPrice(item.CostPrice)}", "grn_edit_mode");
                        }
                        
                        AppLogger.LogInfo($"Loaded {GrnItems.Count} GRN items", "Items populated successfully", "grn_edit_mode");
                    }
                    
                    // Calculate totals manually (since CalculateTotals method doesn't exist)
                    TotalItems = GrnItems.Count;
                    TotalQuantity = GrnItems.Sum(item => item.Quantity);
                    TotalAmount = GrnItems.Sum(item => item.LineTotal);
                    
                    // Set edit mode flag
                    IsEditMode = true;
                    EditingGrnId = grnId;
                    
                    StatusMessage = $"GRN {grn.GrnNo} loaded for editing";
                    AppLogger.LogInfo($"Edit mode setup complete", 
                        $"GRN No: {grn.GrnNo}, Items: {GrnItems.Count}, Total: {_activeCurrencyService.FormatPrice(TotalAmount)}", "grn_edit_mode");
                }
                else
                {
                    AppLogger.LogError($"GRN not found", null, $"GRN ID: {grnId}", "grn_edit_mode");
                    StatusMessage = $"GRN with ID {grnId} not found";
                    throw new InvalidOperationException($"GRN with ID {grnId} not found.");
                }
            }
            else
            {
                AppLogger.LogError("GoodsReceivedService not available", null, "Service is null", "grn_edit_mode");
                StatusMessage = "Service not available";
                throw new InvalidOperationException("GoodsReceivedService is not available.");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to load GRN for editing", ex, $"GRN ID: {grnId}", "grn_edit_mode");
            StatusMessage = $"Error loading GRN: {ex.Message}";
            throw;
        }
        finally
        {
            IsLoading = false;
            AppLogger.LogSeparator("LOAD GRN FOR EDIT COMPLETE", "grn_edit_mode");
        }
    }

    /// <summary>
    /// Flag to indicate if the form is in edit mode
    /// </summary>
    [ObservableProperty]
    private bool isEditMode = false;

    /// <summary>
    /// GRN ID being edited (if in edit mode)
    /// </summary>
    [ObservableProperty]
    private long? editingGrnId = null;

    #endregion
}

/// <summary>
/// ViewModel for individual GRN items in the DataGrid
/// </summary>
public partial class GrnItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int id = 0;

    [ObservableProperty]
    private int productId = 0;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private decimal quantity = 0;

    [ObservableProperty]
    private long uomId = 1;

    [ObservableProperty]
    private string uomName = string.Empty;

    [ObservableProperty]
    private decimal costPrice = 0;

    [ObservableProperty]
    private string batchNo = string.Empty;

    private DateTime? _manufactureDate = null;
    public DateTime? ManufactureDate
    {
        get => _manufactureDate;
        set
        {
            if (_manufactureDate != value)
            {
                _manufactureDate = value;
                OnPropertyChanged();
                AppLogger.LogInfo($"ManufactureDate changed for item {ProductId}", 
                    $"Old: {_manufactureDate}, New: {value}", "grn_date_tracking");
            }
        }
    }

    private DateTime? _expiryDate = null;
    public DateTime? ExpiryDate
    {
        get => _expiryDate;
        set
        {
            if (_expiryDate != value)
            {
                _expiryDate = value;
                OnPropertyChanged();
                AppLogger.LogInfo($"ExpiryDate changed for item {ProductId}", 
                    $"Old: {_expiryDate}, New: {value}", "grn_date_tracking");
            }
        }
    }

    [ObservableProperty]
    private decimal? landedCost;

    [ObservableProperty]
    private decimal lineTotal = 0;

    partial void OnQuantityChanged(decimal value)
    {
        CalculateLineTotal();
    }

    partial void OnCostPriceChanged(decimal value)
    {
        CalculateLineTotal();
    }



    partial void OnLineTotalChanged(decimal value)
    {
        AppLogger.LogInfo($"LineTotal changed for {ProductName}", 
            $"Qty: {Quantity}, Price: {CostPrice:C}, Total: {value:C}", "grn_line_total");
        
        // Notify parent to recalculate summary
        LineTotalChangedEvent?.Invoke();
    }

    // Event to notify parent ViewModel when line total changes
    public Action? LineTotalChangedEvent { get; set; }

    private void CalculateLineTotal()
    {
        var newTotal = Quantity * CostPrice;
        AppLogger.LogInfo($"Calculating LineTotal for {ProductName}", 
            $"Qty: {Quantity} * Price: {CostPrice:C} = {newTotal:C}", "grn_calculation");
        LineTotal = newTotal;
    }
}