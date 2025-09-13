using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;
using System.Globalization;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for adding new products with comprehensive form validation and full settings integration
/// </summary>
public partial class AddProductViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IProductService _productService;
    private readonly Action? _navigateBack;
    
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

    [ObservableProperty]
    private string code = string.Empty;

    [ObservableProperty]
    private int plu = 0;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal price = 0;

    [ObservableProperty]
    private decimal cost = 0;

    [ObservableProperty]
    private decimal lastPurchasePrice = 0;

    [ObservableProperty]
    private decimal markup = 0;

    [ObservableProperty]
    private int? categoryId;

    [ObservableProperty]
    private int selectedUnitOfMeasurementId = 1; // Default to "Pieces"

    [ObservableProperty]
    private UnitOfMeasurementDto? selectedUnitOfMeasurement;

    [ObservableProperty]
    private bool isTaxInclusivePrice = true;

    [ObservableProperty]
    private decimal excise = 0;

    [ObservableProperty]
    private bool isDiscountAllowed = true;

    [ObservableProperty]
    private decimal maxDiscount = 100;

    [ObservableProperty]
    private bool isPriceChangeAllowed = true;

    [ObservableProperty]
    private bool isUsingSerialNumbers = false;

    [ObservableProperty]
    private bool isManufactureRequired = false;

    [ObservableProperty]
    private bool isService = false;

    [ObservableProperty]
    private bool isUsingDefaultQuantity = true;

    // Stock Control Properties (Enhanced)
    [ObservableProperty]
    private bool isStockTracked = true;

    [ObservableProperty]
    private bool allowNegativeStock = false;

    [ObservableProperty]
    private decimal initialStock = 0;

    [ObservableProperty]
    private decimal minimumStock = 0;

    [ObservableProperty]
    private decimal maximumStock = 0;

    [ObservableProperty]
    private decimal reorderLevel = 0;

    [ObservableProperty]
    private decimal reorderQuantity = 0;

    [ObservableProperty]
    private decimal averageCost = 0;

    [ObservableProperty]
    private int selectedStoreId = 1; // Default to store 1

    // Computed Properties for Stock Control
    public bool IsStockFieldsEnabled => IsStockTracked;
    public bool IsSerialNumbersEnabled => IsStockTracked && IsUsingSerialNumbers;

    // Validation Properties
    [ObservableProperty]
    private Dictionary<string, string> stockValidationErrors = new();

    [ObservableProperty]
    private int? ageRestriction;

    [ObservableProperty]
    private string color = "#FFC107";

    [ObservableProperty]
    private string imagePath = string.Empty;

    [ObservableProperty]
    private bool isEnabled = true;

    [ObservableProperty]
    private ObservableCollection<BarcodeItemViewModel> barcodes = new();

    [ObservableProperty]
    private ObservableCollection<string> comments = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> unitsOfMeasurement = new();

    [ObservableProperty]
    private ObservableCollection<string> availableTaxes = new();

    [ObservableProperty]
    private ObservableCollection<StoreDto> availableStores = new();

    [ObservableProperty]
    private string newBarcode = string.Empty;

    [ObservableProperty]
    private string newComment = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready to create new product";

    [ObservableProperty]
    private bool hasValidationErrors = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string barcodeValidationMessage = string.Empty;

    [ObservableProperty]
    private bool hasBarcodeValidationError = false;

    // Navigation Properties for Sidebar
    [ObservableProperty]
    private string currentSection = "ProductInfo";

    // Category panel properties
    [ObservableProperty]
    private bool isCategoryPanelOpen = false;

    [ObservableProperty]
    private CategoryDto currentCategory = new();

    [ObservableProperty]
    private bool isCategoryEditMode = false;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> parentCategories = new();

    #region Settings Properties
    [ObservableProperty]
    private int _currentZoom = 100;

    [ObservableProperty]
    private string _currentLanguage = "English";

    [ObservableProperty]
    private double _currentFontSize = 14;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;
    #endregion

    #region Translation Properties

    // Page titles and navigation
    [ObservableProperty] private string addProductTitle = "Add Product";
    [ObservableProperty] private string productInfoTitle = "Product Information";
    [ObservableProperty] private string pricingTitle = "Pricing";
    [ObservableProperty] private string stockControlTitle = "Stock Control";
    [ObservableProperty] private string advancedSettingsTitle = "Advanced Settings";
    [ObservableProperty] private string categoriesTitle = "Categories";
    [ObservableProperty] private string picturesTitle = "Pictures";
    [ObservableProperty] private string attributesTitle = "Attributes";
    [ObservableProperty] private string unitPricesTitle = "Unit Prices";

    // Form labels and inputs
    [ObservableProperty] private string codeLabel = "Code";
    [ObservableProperty] private string pluLabel = "PLU";
    [ObservableProperty] private string nameLabel = "Name";
    [ObservableProperty] private string descriptionLabel = "Description";
    [ObservableProperty] private string priceLabel = "Price";
    [ObservableProperty] private string costLabel = "Cost";
    [ObservableProperty] private string lastPurchasePriceLabel = "Last Purchase Price";
    [ObservableProperty] private string markupLabel = "Markup";
    [ObservableProperty] private string categoryLabel = "Category";
    [ObservableProperty] private string brandLabel = "Brand";
    [ObservableProperty] private string purchaseUnitLabel = "Purchase Unit";
    [ObservableProperty] private string sellingUnitLabel = "Selling Unit";
    [ObservableProperty] private string unitOfMeasurementLabel = "Unit of Measurement";
    [ObservableProperty] private string isTaxInclusivePriceLabel = "Tax Inclusive Price";
    [ObservableProperty] private string exciseLabel = "Excise";
    [ObservableProperty] private string isDiscountAllowedLabel = "Discount Allowed";
    [ObservableProperty] private string maxDiscountLabel = "Max Discount";
    [ObservableProperty] private string isPriceChangeAllowedLabel = "Price Change Allowed";
    [ObservableProperty] private string isUsingSerialNumbersLabel = "Using Serial Numbers";
    [ObservableProperty] private string isManufactureRequiredLabel = "Manufacture Required";
    [ObservableProperty] private string isServiceLabel = "Service";
    [ObservableProperty] private string isUsingDefaultQuantityLabel = "Using Default Quantity";
    
    // Additional form labels that were missing
    [ObservableProperty] private string groupLabel = "Group";
    [ObservableProperty] private string reorderLevelFormLabel = "Reorder Level";
    [ObservableProperty] private string canReturnLabel = "Can Return";
    [ObservableProperty] private string isGroupedLabel = "Is Grouped";
    [ObservableProperty] private string sellingPriceLabel = "Selling Price";
    [ObservableProperty] private string costPriceLabel = "Cost Price";
    [ObservableProperty] private string markupPercentLabel = "Markup %";
    [ObservableProperty] private string taxInclusivePriceLabel = "Tax Inclusive Price";
    [ObservableProperty] private string chooseImageLabel = "Choose Image";
    [ObservableProperty] private string removeImageLabel = "Remove Image";
    [ObservableProperty] private string noImageSelectedLabel = "No Image Selected";
    [ObservableProperty] private string clickChooseImageLabel = "Click 'Choose Image' to add a product picture";
    [ObservableProperty] private string trackStockLabel = "Track Stock for this Product";
    [ObservableProperty] private string storeLabel = "Store";
    [ObservableProperty] private string trackStockForProductLabel = "Track Stock for this Product";
    [ObservableProperty] private string allowDiscountsLabel = "Allow Discounts";
    [ObservableProperty] private string allowPriceChangesLabel = "Allow Price Changes";
    [ObservableProperty] private string useSerialNumbersLabel = "Use Serial Numbers";
    [ObservableProperty] private string isServiceLabel2 = "Is Service";
    [ObservableProperty] private string ageRestrictionYearsLabel = "Age Restriction (years)";
    [ObservableProperty] private string productColorLabel = "Product Color";
    [ObservableProperty] private string stockControlUnitPricesLabel = "Stock Control & Unit Prices";
    [ObservableProperty] private string allowNegativeStockLabel2 = "Allow Negative Stock";
    [ObservableProperty] private string useSerialNumbersLabel2 = "Use Serial Numbers";

    // Stock control labels
    [ObservableProperty] private string isStockTrackedLabel = "Stock Tracked";
    [ObservableProperty] private string allowNegativeStockLabel = "Allow Negative Stock";
    [ObservableProperty] private string initialStockLabel = "Initial Stock";
    [ObservableProperty] private string minimumStockLabel = "Minimum Stock";
    [ObservableProperty] private string maximumStockLabel = "Maximum Stock";
    [ObservableProperty] private string reorderLevelLabel = "Reorder Level";
    [ObservableProperty] private string reorderQuantityLabel = "Reorder Quantity";
    [ObservableProperty] private string averageCostLabel = "Average Cost";
    [ObservableProperty] private string selectedStoreLabel = "Store";

    // Advanced settings labels
    [ObservableProperty] private string ageRestrictionLabel = "Age Restriction";
    [ObservableProperty] private string colorLabel = "Color";
    [ObservableProperty] private string imagePathLabel = "Image Path";
    [ObservableProperty] private string isEnabledLabel = "Enabled";

    // Barcode section
    [ObservableProperty] private string barcodesTitle = "Barcodes";
    [ObservableProperty] private string newBarcodeLabel = "New Barcode";
    [ObservableProperty] private string addBarcodeButton = "Add Barcode";
    [ObservableProperty] private string generateBarcodeButton = "Generate Barcode";
    [ObservableProperty] private string removeBarcodeButton = "Remove";

    // Comments section
    [ObservableProperty] private string commentsTitle = "Comments";
    [ObservableProperty] private string newCommentLabel = "New Comment";
    [ObservableProperty] private string addCommentButton = "Add Comment";
    [ObservableProperty] private string removeCommentButton = "Remove";

    // Action buttons
    [ObservableProperty] private string saveButton = "Save";
    [ObservableProperty] private string saveAndAddAnotherButton = "Save & Add Another";
    [ObservableProperty] private string cancelButton = "Cancel";
    [ObservableProperty] private string resetButton = "Reset";
    [ObservableProperty] private string browseImageButton = "Browse";
    
    // Button text properties for UI binding
    [ObservableProperty] private string backButtonText = "Back";
    [ObservableProperty] private string cancelButtonText = "Cancel";
    [ObservableProperty] private string saveChangesButtonText = "Save Changes";
    [ObservableProperty] private string saveCategoryButtonText = "Save Category";
    [ObservableProperty] private string currentUserDisplayText = "Administrator";

    // Category panel
    [ObservableProperty] private string addCategoryTitle = "Add Category";
    [ObservableProperty] private string addNewCategoryTitle = "Add New Category";
    [ObservableProperty] private string editCategoryTitle = "Edit Category";
    [ObservableProperty] private string categoryNameLabel = "Category Name";
    [ObservableProperty] private string categoryNameArabicLabel = "Category Name (Arabic)";
    [ObservableProperty] private string parentCategoryLabel = "Parent Category";
    [ObservableProperty] private string categoryDescriptionLabel = "Description";
    [ObservableProperty] private string displayOrderLabel = "Display Order";
    [ObservableProperty] private string displayOrderHelp = "Lower numbers appear first in the list";
    [ObservableProperty] private string activeCategoryLabel = "Active Category";
    [ObservableProperty] private string saveCategoryButton = "Save Category";
    [ObservableProperty] private string cancelCategoryButton = "Cancel";
    [ObservableProperty] private string deleteCategoryButton = "Delete";
    [ObservableProperty] private string closePanelTooltip = "Close Panel";
    
    // Category Info Panel
    [ObservableProperty] private string categoryInfoTitle = "Category Information";
    [ObservableProperty] private string categoryInfoNameRequired = "• Category name is required and cannot be empty";
    [ObservableProperty] private string categoryInfoArabicOptional = "• Arabic name is optional but recommended for bilingual support";
    [ObservableProperty] private string categoryInfoParentHierarchy = "• Select a parent to create subcategories and organize products hierarchically";
    [ObservableProperty] private string categoryInfoDisplayOrder = "• Display order controls the sequence in lists and menus";

    // Validation and status messages
    [ObservableProperty] private string validationErrorsTitle = "Validation Errors";
    [ObservableProperty] private string loadingMessage = "Loading...";
    [ObservableProperty] private string savingMessage = "Saving...";
    [ObservableProperty] private string readyMessage = "Ready to create new product";
    [ObservableProperty] private string successMessage = "Product saved successfully";
    [ObservableProperty] private string errorMessage = "Error";

    // Placeholders
    [ObservableProperty] private string codePlaceholder = "Enter product code";
    [ObservableProperty] private string pluPlaceholder = "Enter PLU number";
    [ObservableProperty] private string namePlaceholder = "Enter product name";
    [ObservableProperty] private string descriptionPlaceholder = "Enter product description";
    [ObservableProperty] private string pricePlaceholder = "0.00";
    [ObservableProperty] private string costPlaceholder = "0.00";
    [ObservableProperty] private string markupPlaceholder = "0.00";
    [ObservableProperty] private string excisePlaceholder = "0.00";
    [ObservableProperty] private string maxDiscountPlaceholder = "100";
    [ObservableProperty] private string stockPlaceholder = "0";
    [ObservableProperty] private string ageRestrictionPlaceholder = "18";
    [ObservableProperty] private string barcodePlaceholder = "Enter barcode";
    [ObservableProperty] private string commentPlaceholder = "Enter comment";

    // Tooltips
    [ObservableProperty] private string codeTooltip = "Unique product identifier";
    [ObservableProperty] private string pluTooltip = "Price Look-Up number";
    [ObservableProperty] private string nameTooltip = "Product display name";
    [ObservableProperty] private string priceTooltip = "Selling price";
    [ObservableProperty] private string costTooltip = "Cost price";
    [ObservableProperty] private string stockTooltip = "Current stock quantity";
    [ObservableProperty] private string discountTooltip = "Maximum discount percentage";
    [ObservableProperty] private string barcodeTooltip = "Product barcode for scanning";

    // Dropdown options
    [ObservableProperty] private string selectCategoryOption = "Select Category";
    [ObservableProperty] private string selectUnitOption = "Select Unit";
    [ObservableProperty] private string selectStoreOption = "Select Store";
    [ObservableProperty] private string noCategoryOption = "No Category";
    [ObservableProperty] private string noParentCategoryOption = "No Parent Category";

    // File dialog
    [ObservableProperty] private string selectImageTitle = "Select Product Image";
    [ObservableProperty] private string imageFileFilter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

    // Confirmation dialogs
    [ObservableProperty] private string confirmDeleteTitle = "Confirm Delete";
    [ObservableProperty] private string confirmDeleteMessage = "Are you sure you want to delete this item?";
    [ObservableProperty] private string confirmResetTitle = "Confirm Reset";
    [ObservableProperty] private string confirmResetMessage = "Are you sure you want to reset all fields?";
    [ObservableProperty] private string yesButton = "Yes";
    [ObservableProperty] private string noButton = "No";

    #endregion

    #endregion

    #region Barcode Management Classes

    public class BarcodeItemViewModel : ObservableObject
    {
        private string _value = string.Empty;
        private bool _isNew = true;
        private bool _isDeleted = false;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public bool IsDeleted
        {
            get => _isDeleted;
            set => SetProperty(ref _isDeleted, value);
        }

        public object? Id { get; set; }
    }

    #endregion

    #region Validation Classes

    public class ValidationResult
    {
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool IsValid => !Errors.Any();
    }

    #endregion

    #region Validation Properties

    public List<string> ValidationErrors { get; private set; } = new();

    #endregion

    #region Stock Validation Methods

    partial void OnIsStockTrackedChanged(bool value)
    {
        ValidateStockLevels();
        OnPropertyChanged(nameof(IsStockFieldsEnabled));
    }

    partial void OnInitialStockChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnMinimumStockChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnMaximumStockChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnReorderLevelChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnIsUsingSerialNumbersChanged(bool value)
    {
        ValidateStockLevels();
        OnPropertyChanged(nameof(IsSerialNumbersEnabled));
        
        if (value && InitialStock > 0)
        {
            InitialStock = 0;
            StatusMessage = "Initial stock set to 0 for serial number tracking";
        }
    }

    private void ValidateStockLevels()
    {
        StockValidationErrors.Clear();
        
        if (IsStockTracked)
        {
            // Validate minimum vs maximum
            if (MaximumStock > 0 && MinimumStock > MaximumStock)
            {
                StockValidationErrors["MinimumStock"] = "Minimum stock cannot exceed maximum stock";
            }
            
            // Validate reorder level
            if (ReorderLevel > 0 && MinimumStock > 0 && ReorderLevel < MinimumStock)
            {
                StockValidationErrors["ReorderLevel"] = "Reorder level should be at or above minimum stock";
            }
            
            // Validate initial stock
            if (InitialStock < 0)
            {
                StockValidationErrors["InitialStock"] = "Initial stock cannot be negative";
            }
            
            // Validate costs
            if (InitialStock > 0 && AverageCost <= 0)
            {
                StockValidationErrors["AverageCost"] = "Average cost should be specified when setting initial stock";
            }
            
            // Validate reorder quantity
            if (ReorderLevel > 0 && ReorderQuantity <= 0)
            {
                StockValidationErrors["ReorderQuantity"] = "Reorder quantity should be specified when reorder level is set";
            }
            
            // Serial number validation
            if (IsUsingSerialNumbers && InitialStock > 0)
            {
                StockValidationErrors["SerialNumbers"] = "Serial numbers must be entered individually. Initial stock will be set to 0.";
            }
        }
        
        OnPropertyChanged(nameof(StockValidationErrors));
        OnPropertyChanged(nameof(HasStockValidationErrors));
    }

    public bool HasStockValidationErrors => StockValidationErrors.Any();

    #endregion

    #region Constructor

    public AddProductViewModel(
        IProductService productService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action? navigateBack = null)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _navigateBack = navigateBack;
        
        // Initialize with default values
        Code = GenerateNextCode();
        Name = "Test Product"; // Debug: Set a default name to see if binding works
        IsTaxInclusivePrice = true;
        IsEnabled = true;
        IsDiscountAllowed = true;
        MaxDiscount = 100;
        SelectedUnitOfMeasurementId = 1; // Default to "Pieces"
        Color = "#FFC107";
        
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            FileLogger.LogSeparator("AddProductViewModel Initialization");
            FileLogger.Log("🚀 Starting AddProductViewModel initialization");
            
            IsLoading = true;
            StatusMessage = "Loading data...";
            FileLogger.Log($"📊 Initial status: {StatusMessage}");

            // Load translations (translations are now seeded at application startup)
            FileLogger.Log("🌐 Loading translations");
            await LoadTranslationsAsync();
            FileLogger.Log("✅ Translations loaded");
            
            // Subscribe to settings changes
            FileLogger.Log("📡 Subscribing to settings changes");
            SubscribeToSettingsChanges();
            FileLogger.Log("✅ Settings subscriptions done");
            
            // Initialize current settings values
            FileLogger.Log("⚙️ Updating current settings");
            UpdateCurrentSettings();
            FileLogger.Log("✅ Current settings updated");

            FileLogger.Log("📦 Loading categories");
            await LoadCategoriesAsync();
            FileLogger.Log($"✅ Loaded {Categories.Count} categories");
            
            FileLogger.Log("🏪 Loading stores");
            await LoadStoresAsync();
            FileLogger.Log($"✅ Loaded stores");
            
            FileLogger.Log("📏 Loading units of measurement");
            await LoadUnitsOfMeasurementAsync();
            FileLogger.Log($"✅ Loaded units of measurement");

            StatusMessage = "Ready to create new product";
            FileLogger.Log($"🎯 Final status: {StatusMessage}");
            FileLogger.Log("🎉 AddProductViewModel initialization completed successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ ERROR in AddProductViewModel initialization: {ex.Message}");
            FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
            StatusMessage = $"Error loading data: {ex.Message}";
            MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            FileLogger.LogSeparator("AddProductViewModel Initialization Complete");
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var categoryList = await _productService.GetAllCategoriesAsync();
        Categories.Clear();
        ParentCategories.Clear();
        
        // Add "No Parent" option for parent categories
        ParentCategories.Add(new CategoryDto { Id = 0, Name = "No Parent Category", Description = "Top Level Category" });
        
        foreach (var category in categoryList)
        {
            Categories.Add(category);
            ParentCategories.Add(category);
        }
    }

    private async Task LoadStoresAsync()
    {
        // For now, add default stores. In a real app, this would come from a store service
        AvailableStores.Clear();
        AvailableStores.Add(new StoreDto { Id = 1, Name = "Main Store", IsDefault = true, IsActive = true });
        AvailableStores.Add(new StoreDto { Id = 2, Name = "Branch Store", IsDefault = false, IsActive = true });
        
        // Set default store
        SelectedStoreId = AvailableStores.FirstOrDefault(s => s.IsDefault)?.Id ?? 1;
        
        await Task.CompletedTask; // Placeholder for async operation
    }

    private async Task LoadUnitsOfMeasurementAsync()
    {
        var uomList = await _productService.GetAllUnitsOfMeasurementAsync();
        UnitsOfMeasurement.Clear();
        
        foreach (var uom in uomList)
        {
            UnitsOfMeasurement.Add(uom);
        }
        
        // Set default UOM to "Pieces" if available
        SelectedUnitOfMeasurement = UnitsOfMeasurement.FirstOrDefault(u => u.Id == 1) ?? UnitsOfMeasurement.FirstOrDefault();
        if (SelectedUnitOfMeasurement != null)
        {
            SelectedUnitOfMeasurementId = SelectedUnitOfMeasurement.Id;
        }
    }

    private string GenerateNextCode()
    {
        // Generate a simple auto-incrementing code
        // In a real application, this would query the database for the next available code
        return $"PROD{DateTime.Now:yyyyMMddHHmmss}";
    }

    #endregion

    #region Translation and Settings Management

    private async Task LoadTranslationsAsync()
    {
        try
        {
            FileLogger.LogSeparator("LoadTranslationsAsync");
            FileLogger.Log("🌐 Starting translation loading");
            
            // Get current language from database service
            var currentLangCode = _databaseLocalizationService.GetCurrentLanguageCode();
            FileLogger.Log($"🗣️ Current language code: {currentLangCode}");
            
            // Apply translations to properties using the database localization service
            FileLogger.Log("📝 Applying translations to properties");
            await ApplyTranslationsAsync();
            FileLogger.Log("✅ Translations applied successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ ERROR in LoadTranslationsAsync: {ex.Message}");
            FileLogger.Log($"❌ Inner exception: {ex.InnerException?.Message}");
            FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
            // Log error and use default English values
            System.Diagnostics.Debug.WriteLine($"Error loading translations: {ex.Message}");
        }
        finally
        {
            FileLogger.LogSeparator("LoadTranslationsAsync Complete");
        }
    }

    private async Task ApplyTranslationsAsync()
    {
        // Page titles and navigation
        AddProductTitle = await GetTranslationAsync("add_product_title", "Add New Product");
        ProductInfoTitle = await GetTranslationAsync("basic_info_section", "Basic Information");
        PricingTitle = await GetTranslationAsync("pricing_section", "Pricing & Cost");
        StockControlTitle = await GetTranslationAsync("stock_section", "Stock Management");
        AdvancedSettingsTitle = await GetTranslationAsync("advanced_section", "Advanced Settings");
        CategoriesTitle = await GetTranslationAsync("Categories", "Categories");
        PicturesTitle = await GetTranslationAsync("Pictures", "Pictures");
        AttributesTitle = await GetTranslationAsync("Attributes", "Attributes");
        UnitPricesTitle = await GetTranslationAsync("UnitPrices", "Unit Prices");
        BarcodesTitle = await GetTranslationAsync("barcodes_section", "Barcodes & SKU");

        // Form labels - using the correct translation keys
        CodeLabel = await GetTranslationAsync("product_code_label", "Product Code");
        PluLabel = await GetTranslationAsync("plu_label", "PLU");
        NameLabel = await GetTranslationAsync("product_name_label", "Product Name");
        DescriptionLabel = await GetTranslationAsync("description_label", "Description");
        PriceLabel = await GetTranslationAsync("price_label", "Price");
        CostLabel = await GetTranslationAsync("cost_label", "Cost");
        LastPurchasePriceLabel = await GetTranslationAsync("last_purchase_price_label", "Last Purchase Price");
        MarkupLabel = await GetTranslationAsync("markup_label", "Markup");
        CategoryLabel = await GetTranslationAsync("category_label", "Category");
        BrandLabel = await GetTranslationAsync("brand_label", "Brand");
        PurchaseUnitLabel = await GetTranslationAsync("purchase_unit_label", "Purchase Unit");
        SellingUnitLabel = await GetTranslationAsync("selling_unit_label", "Selling Unit");
        UnitOfMeasurementLabel = await GetTranslationAsync("unit_of_measurement_label", "Unit of Measurement");
        IsTaxInclusivePriceLabel = await GetTranslationAsync("tax_inclusive_price_label", "Tax Inclusive Price");
        ExciseLabel = await GetTranslationAsync("excise_label", "Excise");
        IsDiscountAllowedLabel = await GetTranslationAsync("discount_allowed_label", "Discount Allowed");
        MaxDiscountLabel = await GetTranslationAsync("max_discount_label", "Maximum Discount %");
        IsPriceChangeAllowedLabel = await GetTranslationAsync("price_change_allowed_label", "Price Change Allowed");
        IsUsingSerialNumbersLabel = await GetTranslationAsync("serial_numbers_label", "Use Serial Numbers");
        IsManufactureRequiredLabel = await GetTranslationAsync("ManufactureRequired", "Manufacture Required");
        IsServiceLabel = await GetTranslationAsync("service_item_label", "Service Item");
        IsUsingDefaultQuantityLabel = await GetTranslationAsync("UsingDefaultQuantity", "Using Default Quantity");

        // Additional form labels
        GroupLabel = await GetTranslationAsync("group_label", "Group");
        ReorderLevelFormLabel = await GetTranslationAsync("reorder_level_label", "Reorder Level");
        CanReturnLabel = await GetTranslationAsync("can_return_label", "Can Return");
        IsGroupedLabel = await GetTranslationAsync("is_grouped_label", "Is Grouped");
        SellingPriceLabel = await GetTranslationAsync("selling_price_label", "Selling Price");
        CostPriceLabel = await GetTranslationAsync("cost_price_label", "Cost Price");
        MarkupPercentLabel = await GetTranslationAsync("markup_percent_label", "Markup %");
        TaxInclusivePriceLabel = await GetTranslationAsync("tax_inclusive_price_label", "Tax Inclusive Price");
        ChooseImageLabel = await GetTranslationAsync("choose_image_label", "Choose Image");
        RemoveImageLabel = await GetTranslationAsync("remove_image_label", "Remove Image");
        NoImageSelectedLabel = await GetTranslationAsync("no_image_selected_label", "No Image Selected");
        ClickChooseImageLabel = await GetTranslationAsync("click_choose_image_label", "Click 'Choose Image' to add a product picture");
        TrackStockLabel = await GetTranslationAsync("track_stock_label", "Track Stock");
        StoreLabel = await GetTranslationAsync("store_label", "Store");
        TrackStockForProductLabel = await GetTranslationAsync("track_stock_for_product_label", "Track Stock for this Product");
        AllowDiscountsLabel = await GetTranslationAsync("allow_discounts_label", "Allow Discounts");
        AllowPriceChangesLabel = await GetTranslationAsync("allow_price_changes_label", "Allow Price Changes");
        UseSerialNumbersLabel = await GetTranslationAsync("use_serial_numbers_label", "Use Serial Numbers");
        IsServiceLabel2 = await GetTranslationAsync("is_service_label", "Is Service");
        AgeRestrictionYearsLabel = await GetTranslationAsync("age_restriction_years_label", "Age Restriction (years)");
        ProductColorLabel = await GetTranslationAsync("product_color_label", "Product Color");
        StockControlUnitPricesLabel = await GetTranslationAsync("stock_control_unit_prices_label", "Stock Control & Unit Prices");
        AllowNegativeStockLabel2 = await GetTranslationAsync("allow_negative_stock_label", "Allow Negative Stock");
        UseSerialNumbersLabel2 = await GetTranslationAsync("use_serial_numbers_label", "Use Serial Numbers");

        // Stock control labels
        IsStockTrackedLabel = await GetTranslationAsync("stock_tracked_label", "Stock Tracked");
        AllowNegativeStockLabel = await GetTranslationAsync("allow_negative_stock_label", "Allow Negative Stock");
        InitialStockLabel = await GetTranslationAsync("initial_stock_label", "Initial Stock");
        MinimumStockLabel = await GetTranslationAsync("minimum_stock_label", "Minimum Stock");
        MaximumStockLabel = await GetTranslationAsync("maximum_stock_label", "Maximum Stock");
        ReorderLevelLabel = await GetTranslationAsync("reorder_level_label", "Reorder Level");
        ReorderQuantityLabel = await GetTranslationAsync("reorder_quantity_label", "Reorder Quantity");
        AverageCostLabel = await GetTranslationAsync("average_cost_label", "Average Cost");
        SelectedStoreLabel = await GetTranslationAsync("store_label", "Store");

        // Advanced settings
        AgeRestrictionLabel = await GetTranslationAsync("age_restriction_label", "Age Restriction");
        ColorLabel = await GetTranslationAsync("color_label", "Color");
        ImagePathLabel = await GetTranslationAsync("image_path_label", "Image Path");
        IsEnabledLabel = await GetTranslationAsync("enabled_label", "Product Enabled");

        // Barcode section
        NewBarcodeLabel = await GetTranslationAsync("barcode_label", "Barcode");
        AddBarcodeButton = await GetTranslationAsync("add_barcode_button", "Add Barcode");
        GenerateBarcodeButton = await GetTranslationAsync("generate_barcode_button", "Generate Barcode");
        RemoveBarcodeButton = await GetTranslationAsync("remove_barcode_button", "Remove");

        // Comments section
        CommentsTitle = await GetTranslationAsync("Comments", "Comments");
        NewCommentLabel = await GetTranslationAsync("NewComment", "New Comment");
        AddCommentButton = await GetTranslationAsync("AddComment", "Add Comment");
        RemoveCommentButton = await GetTranslationAsync("RemoveComment", "Remove");

        // Action buttons
        SaveButton = await GetTranslationAsync("save_button", "Save Product");
        SaveAndAddAnotherButton = await GetTranslationAsync("SaveAndAddAnother", "Save & Add Another");
        CancelButton = await GetTranslationAsync("cancel_button", "Cancel");
        ResetButton = await GetTranslationAsync("reset_button", "Reset Form");
        BrowseImageButton = await GetTranslationAsync("Browse", "Browse");
        
        // Button text properties for UI binding
        BackButtonText = await GetTranslationAsync("back_button", "← Back");
        CancelButtonText = await GetTranslationAsync("cancel_button", "Cancel");
        SaveChangesButtonText = await GetTranslationAsync("save_button", "Save Product");
        SaveCategoryButtonText = await GetTranslationAsync("save_category_button", "Save Category");
        CurrentUserDisplayText = await GetTranslationAsync("Administrator", "Administrator");

        // Category panel
        AddCategoryTitle = await GetTranslationAsync("add_category_title", "Add New Category");
        AddNewCategoryTitle = await GetTranslationAsync("add_category_title", "Add New Category");
        EditCategoryTitle = await GetTranslationAsync("EditCategory", "Edit Category");
        CategoryNameLabel = await GetTranslationAsync("category_name_label", "Category Name");
        CategoryNameArabicLabel = await GetTranslationAsync("category_name_arabic_label", "Category Name (Arabic)");
        ParentCategoryLabel = await GetTranslationAsync("ParentCategory", "Parent Category");
        CategoryDescriptionLabel = await GetTranslationAsync("CategoryDescription", "Description");
        DisplayOrderLabel = await GetTranslationAsync("DisplayOrder", "Display Order");
        DisplayOrderHelp = await GetTranslationAsync("DisplayOrderHelp", "Lower numbers appear first in the list");
        ActiveCategoryLabel = await GetTranslationAsync("ActiveCategory", "Active Category");
        SaveCategoryButton = await GetTranslationAsync("SaveCategory", "Save Category");
        CancelCategoryButton = await GetTranslationAsync("CancelCategory", "Cancel");
        DeleteCategoryButton = await GetTranslationAsync("DeleteCategory", "Delete");
        ClosePanelTooltip = await GetTranslationAsync("ClosePanelTooltip", "Close Panel");
        
        // Category Info Panel
        CategoryInfoTitle = await GetTranslationAsync("CategoryInfoTitle", "Category Information");
        CategoryInfoNameRequired = await GetTranslationAsync("CategoryInfoNameRequired", "• Category name is required and cannot be empty");
        CategoryInfoArabicOptional = await GetTranslationAsync("CategoryInfoArabicOptional", "• Arabic name is optional but recommended for bilingual support");
        CategoryInfoParentHierarchy = await GetTranslationAsync("CategoryInfoParentHierarchy", "• Select a parent to create subcategories and organize products hierarchically");
        CategoryInfoDisplayOrder = await GetTranslationAsync("CategoryInfoDisplayOrder", "• Display order controls the sequence in lists and menus");

        // Status messages
        ValidationErrorsTitle = await GetTranslationAsync("ValidationErrors", "Validation Errors");
        LoadingMessage = await GetTranslationAsync("Loading", "Loading...");
        SavingMessage = await GetTranslationAsync("Saving", "Saving...");
        ReadyMessage = await GetTranslationAsync("ReadyToCreateProduct", "Ready to create new product");
        SuccessMessage = await GetTranslationAsync("ProductSavedSuccessfully", "Product saved successfully");
        ErrorMessage = await GetTranslationAsync("Error", "Error");

        // Placeholders
        CodePlaceholder = await GetTranslationAsync("EnterProductCode", "Enter product code");
        PluPlaceholder = await GetTranslationAsync("EnterPLUNumber", "Enter PLU number");
        NamePlaceholder = await GetTranslationAsync("EnterProductName", "Enter product name");
        DescriptionPlaceholder = await GetTranslationAsync("EnterProductDescription", "Enter product description");
        PricePlaceholder = await GetTranslationAsync("PricePlaceholder", "0.00");
        CostPlaceholder = await GetTranslationAsync("CostPlaceholder", "0.00");
        MarkupPlaceholder = await GetTranslationAsync("MarkupPlaceholder", "0.00");
        ExcisePlaceholder = await GetTranslationAsync("ExcisePlaceholder", "0.00");
        MaxDiscountPlaceholder = await GetTranslationAsync("MaxDiscountPlaceholder", "100");
        StockPlaceholder = await GetTranslationAsync("StockPlaceholder", "0");
        AgeRestrictionPlaceholder = await GetTranslationAsync("AgeRestrictionPlaceholder", "18");
        BarcodePlaceholder = await GetTranslationAsync("EnterBarcode", "Enter barcode");
        CommentPlaceholder = await GetTranslationAsync("EnterComment", "Enter comment");

        // Tooltips
        CodeTooltip = await GetTranslationAsync("CodeTooltip", "Unique product identifier");
        PluTooltip = await GetTranslationAsync("PLUTooltip", "Price Look-Up number");
        NameTooltip = await GetTranslationAsync("NameTooltip", "Product display name");
        PriceTooltip = await GetTranslationAsync("PriceTooltip", "Selling price");
        CostTooltip = await GetTranslationAsync("CostTooltip", "Cost price");
        StockTooltip = await GetTranslationAsync("StockTooltip", "Current stock quantity");
        DiscountTooltip = await GetTranslationAsync("DiscountTooltip", "Maximum discount percentage");
        BarcodeTooltip = await GetTranslationAsync("BarcodeTooltip", "Product barcode for scanning");

        // Dropdown options
        SelectCategoryOption = await GetTranslationAsync("SelectCategory", "Select Category");
        SelectUnitOption = await GetTranslationAsync("SelectUnit", "Select Unit");
        SelectStoreOption = await GetTranslationAsync("SelectStore", "Select Store");
        NoCategoryOption = await GetTranslationAsync("NoCategory", "No Category");
        NoParentCategoryOption = await GetTranslationAsync("NoParentCategory", "No Parent Category");

        // File dialog
        SelectImageTitle = await GetTranslationAsync("SelectProductImage", "Select Product Image");
        ImageFileFilter = await GetTranslationAsync("ImageFileFilter", "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif");

        // Confirmation dialogs
        ConfirmDeleteTitle = await GetTranslationAsync("ConfirmDelete", "Confirm Delete");
        ConfirmDeleteMessage = await GetTranslationAsync("ConfirmDeleteMessage", "Are you sure you want to delete this item?");
        ConfirmResetTitle = await GetTranslationAsync("ConfirmReset", "Confirm Reset");
        ConfirmResetMessage = await GetTranslationAsync("ConfirmResetMessage", "Are you sure you want to reset all fields?");
        YesButton = await GetTranslationAsync("Yes", "Yes");
        NoButton = await GetTranslationAsync("No", "No");
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

    private void SubscribeToSettingsChanges()
    {
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;

        // Subscribe to layout direction changes
        _layoutDirectionService.DirectionChanged += OnLayoutDirectionChanged;

        // Subscribe to font changes
        _fontService.FontChanged += OnFontChanged;

        // Subscribe to other settings changes
        _themeService.ThemeChanged += OnThemeChanged;
        _zoomService.ZoomChanged += OnZoomChanged;
    }

    private void UpdateCurrentSettings()
    {
        CurrentZoom = (int)_zoomService.CurrentZoomLevel;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString();
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        CurrentFontSize = _fontService.CurrentFontSize switch
        {
            FontSize.VerySmall => 10.0,
            FontSize.Small => 12.0,
            FontSize.Medium => 14.0,
            FontSize.Large => 16.0,
            _ => 14.0
        };
    }

    private async void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        CurrentLanguage = newLanguage.ToString();
        await LoadTranslationsAsync();
    }

    private async void OnDatabaseLanguageChanged(object? sender, string languageCode)
    {
        FileLogger.LogSeparator("Database Language Changed");
        FileLogger.Log($"🔄 Database language changed to: {languageCode}");
        
        try
        {
            FileLogger.Log("🔄 Reloading translations...");
            await LoadTranslationsAsync();
            
            // Force UI update by notifying all translation properties
            FileLogger.Log("🔄 Forcing UI update for all translated properties");
            OnPropertyChanged(nameof(AddProductTitle));
            OnPropertyChanged(nameof(ProductInfoTitle));
            OnPropertyChanged(nameof(PricingTitle));
            OnPropertyChanged(nameof(BackButtonText));
            OnPropertyChanged(nameof(CancelButtonText));
            OnPropertyChanged(nameof(SaveChangesButtonText));
            OnPropertyChanged(nameof(AddNewCategoryTitle));
            OnPropertyChanged(nameof(CategoryNameLabel));
            OnPropertyChanged(nameof(CodeLabel));
            OnPropertyChanged(nameof(NameLabel));
            OnPropertyChanged(nameof(CategoryLabel));
            OnPropertyChanged(nameof(BrandLabel));
            OnPropertyChanged(nameof(PurchaseUnitLabel));
            OnPropertyChanged(nameof(SellingUnitLabel));
            OnPropertyChanged(nameof(BarcodesTitle));
            OnPropertyChanged(nameof(UnitPricesTitle));
            FileLogger.Log($"✅ Language change completed successfully for language: {languageCode}");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error during language change: {ex.Message}");
            FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
        }
        finally
        {
            FileLogger.LogSeparator("Database Language Changed Complete");
        }
    }

    private void OnLayoutDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void OnFontChanged(FontSize newFontSize)
    {
        // Convert enum to approximate double value for display
        CurrentFontSize = newFontSize switch
        {
            FontSize.VerySmall => 10.0,
            FontSize.Small => 12.0,
            FontSize.Medium => 14.0,
            FontSize.Large => 16.0,
            _ => 14.0
        };
    }

    private void OnThemeChanged(Theme newTheme)
    {
        // Trigger UI update for theme changes
        OnPropertyChanged(nameof(AddProductTitle));
    }

    private void OnZoomChanged(ZoomLevel newZoomLevel)
    {
        CurrentZoom = (int)newZoomLevel;
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
        _layoutDirectionService.DirectionChanged -= OnLayoutDirectionChanged;
        _fontService.FontChanged -= OnFontChanged;
        _themeService.ThemeChanged -= OnThemeChanged;
        _zoomService.ZoomChanged -= OnZoomChanged;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void AddBarcode()
    {
        if (!CanAddBarcode())
        {
            return;
        }

        var trimmedBarcode = NewBarcode.Trim();
        var validation = ValidateBarcode(trimmedBarcode);
        
        if (!validation.IsValid)
        {
            BarcodeValidationMessage = string.Join(Environment.NewLine, validation.Errors);
            HasBarcodeValidationError = true;
            return;
        }

        var barcodeItem = new BarcodeItemViewModel 
        { 
            Value = trimmedBarcode,
            IsNew = true 
        };
        
        Barcodes.Add(barcodeItem);
        NewBarcode = string.Empty;
        BarcodeValidationMessage = string.Empty;
        HasBarcodeValidationError = false;
        
        StatusMessage = $"Barcode '{trimmedBarcode}' added successfully";
    }

    [RelayCommand]
    private void RemoveBarcode(BarcodeItemViewModel? barcode)
    {
        if (barcode != null)
        {
            Barcodes.Remove(barcode);
            StatusMessage = $"Barcode '{barcode.Value}' removed";
        }
    }

    [RelayCommand]
    private void GenerateBarcode()
    {
        try
        {
            // Generate different types of barcodes based on preference
            var generatedBarcode = GenerateUniqueBarcode();
            
            if (!string.IsNullOrEmpty(generatedBarcode))
            {
                // Check if it already exists
                if (!Barcodes.Any(b => b.Value.Equals(generatedBarcode, StringComparison.OrdinalIgnoreCase)))
                {
                    // Set the generated barcode in the input field so user can see it before adding
                    NewBarcode = generatedBarcode;
                    StatusMessage = $"Barcode '{generatedBarcode}' generated - click Add to include it";
                }
                else
                {
                    // Try again with a different pattern
                    GenerateBarcode();
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error generating barcode: {ex.Message}";
        }
    }

    private bool CanAddBarcode()
    {
        if (string.IsNullOrWhiteSpace(NewBarcode))
        {
            BarcodeValidationMessage = "Barcode cannot be empty";
            HasBarcodeValidationError = true;
            return false;
        }

        var trimmedBarcode = NewBarcode.Trim();
        if (Barcodes.Any(b => b.Value.Equals(trimmedBarcode, StringComparison.OrdinalIgnoreCase)))
        {
            BarcodeValidationMessage = "Barcode already exists in the list";
            HasBarcodeValidationError = true;
            return false;
        }

        BarcodeValidationMessage = string.Empty;
        HasBarcodeValidationError = false;
        return true;
    }

    private ValidationResult ValidateBarcode(string barcodeValue)
    {
        var result = new ValidationResult();
        
        // Check if empty
        if (string.IsNullOrWhiteSpace(barcodeValue))
        {
            result.Errors.Add("Barcode cannot be empty");
            return result;
        }

        // Check length (typical barcode lengths)
        if (barcodeValue.Length < 4 || barcodeValue.Length > 50)
        {
            result.Errors.Add("Barcode length should be between 4-50 characters");
        }

        // Check for valid characters (alphanumeric, hyphens, spaces)
        if (!IsValidBarcodeFormat(barcodeValue))
        {
            result.Errors.Add("Barcode contains invalid characters. Only letters, numbers, hyphens, and spaces are allowed");
        }

        return result;
    }

    private bool IsValidBarcodeFormat(string barcode)
    {
        // Allow alphanumeric characters, hyphens, and spaces
        return Regex.IsMatch(barcode, @"^[a-zA-Z0-9\-\s]+$");
    }

    [RelayCommand]
    private void AddComment()
    {
        if (!string.IsNullOrWhiteSpace(NewComment))
        {
            Comments.Add(NewComment);
            NewComment = string.Empty;
            StatusMessage = "Comment added successfully";
        }
    }

    [RelayCommand]
    private void RemoveComment(string comment)
    {
        if (!string.IsNullOrEmpty(comment))
        {
            Comments.Remove(comment);
            StatusMessage = "Comment removed";
        }
    }

    [RelayCommand]
    private void ChooseImage()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFile = openFileDialog.FileName;
                
                // Validate file size (max 5MB)
                var fileInfo = new FileInfo(selectedFile);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("Image file size cannot exceed 5MB. Please choose a smaller image.", 
                        "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Copy file to application images directory
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChronoPos", "Images");
                Directory.CreateDirectory(appDataPath);

                var fileName = $"product_{Code}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(selectedFile)}";
                var destinationPath = Path.Combine(appDataPath, fileName);

                File.Copy(selectedFile, destinationPath, true);
                ImagePath = destinationPath;
                
                StatusMessage = "Product image updated successfully";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        ImagePath = string.Empty;
        StatusMessage = "Product image removed";
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Validating product data...";

            if (!ValidateForm())
            {
                StatusMessage = "Please fix validation errors before saving";
                return;
            }

            StatusMessage = "Saving product...";

            var productDto = CreateProductDto();
            var savedProduct = await _productService.CreateProductAsync(productDto);

            StatusMessage = "Product saved successfully!";
            MessageBox.Show("Product created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Navigate back to product management
            _navigateBack?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving product: {ex.Message}";
            MessageBox.Show($"Failed to save product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        var result = MessageBox.Show("Are you sure you want to cancel? All unsaved changes will be lost.", 
            "Confirm Cancel", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Navigate back to product management
            _navigateBack?.Invoke();
        }
    }

    [RelayCommand]
    private void ResetForm()
    {
        Code = GenerateNextCode();
        Name = string.Empty;
        Description = string.Empty;
        Price = 0;
        Cost = 0;
        Markup = 0;
        CategoryId = null;
        SelectedUnitOfMeasurementId = 1; // Reset to "Pieces"
        SelectedUnitOfMeasurement = UnitsOfMeasurement.FirstOrDefault(u => u.Id == 1) ?? UnitsOfMeasurement.FirstOrDefault();
        IsTaxInclusivePrice = true;
        Excise = 0;
        IsDiscountAllowed = true;
        MaxDiscount = 100;
        IsPriceChangeAllowed = true;
        IsUsingSerialNumbers = false;
        IsService = false;
        AgeRestriction = null;
        Color = "#FFC107";
        ImagePath = string.Empty;
        IsEnabled = true;
        
        Barcodes.Clear();
        Comments.Clear();
        NewBarcode = string.Empty;
        NewComment = string.Empty;
        
        ValidationErrors.Clear();
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
        BarcodeValidationMessage = string.Empty;
        HasBarcodeValidationError = false;
        
        StatusMessage = "Form reset - ready for new product";
    }

    #endregion

    #region Category Panel Commands

    [RelayCommand]
    private void OpenAddCategoryPanel()
    {
        CurrentCategory = new CategoryDto { IsActive = true, DisplayOrder = 0 };
        IsCategoryEditMode = false;
        IsCategoryPanelOpen = true;
    }

    [RelayCommand]
    private void CloseCategoryPanel()
    {
        IsCategoryPanelOpen = false;
        CurrentCategory = new CategoryDto();
    }

    [RelayCommand]
    private async Task SaveCategory()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Saving category...";

            if (!ValidateCategoryForm())
            {
                StatusMessage = "Please fix category validation errors";
                return;
            }

            CategoryDto savedCategory;
            if (IsCategoryEditMode)
            {
                savedCategory = await _productService.UpdateCategoryAsync(CurrentCategory);
                StatusMessage = "Category updated successfully";
            }
            else
            {
                savedCategory = await _productService.CreateCategoryAsync(CurrentCategory);
                StatusMessage = "Category created successfully";
            }

            // Refresh categories list
            await LoadCategoriesAsync();
            
            // Select the newly created/updated category
            CategoryId = savedCategory.Id;

            // Close the panel
            IsCategoryPanelOpen = false;
            
            MessageBox.Show("Category saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving category: {ex.Message}";
            MessageBox.Show($"Failed to save category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool ValidateCategoryForm()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(CurrentCategory.Name))
            errors.Add("Category name is required");
        
        if (CurrentCategory.Name.Length > 100)
            errors.Add("Category name cannot exceed 100 characters");

        if (!string.IsNullOrWhiteSpace(CurrentCategory.Description) && CurrentCategory.Description.Length > 500)
            errors.Add("Category description cannot exceed 500 characters");

        if (!string.IsNullOrWhiteSpace(CurrentCategory.NameArabic) && CurrentCategory.NameArabic.Length > 100)
            errors.Add("Category name (Arabic) cannot exceed 100 characters");

        if (CurrentCategory.DisplayOrder < 0)
            errors.Add("Display order cannot be negative");

        if (errors.Any())
        {
            ValidationMessage = string.Join(Environment.NewLine, errors);
            HasValidationErrors = true;
            return false;
        }

        HasValidationErrors = false;
        return true;
    }

    #endregion

    #region Validation

    private bool ValidateForm()
    {
        ValidationErrors.Clear();

        // Required field validation
        if (string.IsNullOrWhiteSpace(Code))
            ValidationErrors.Add("Product code is required");
        else if (Code.Length > 50)
            ValidationErrors.Add("Product code cannot exceed 50 characters");

        if (string.IsNullOrWhiteSpace(Name))
            ValidationErrors.Add("Product name is required");
        else if (Name.Length > 100)
            ValidationErrors.Add("Product name cannot exceed 100 characters");

        if (Price < 0)
            ValidationErrors.Add("Price cannot be negative");

        if (Cost < 0)
            ValidationErrors.Add("Cost cannot be negative");

        if (MaxDiscount < 0 || MaxDiscount > 100)
            ValidationErrors.Add("Max discount must be between 0 and 100");

        if (AgeRestriction.HasValue && (AgeRestriction < 0 || AgeRestriction > 150))
            ValidationErrors.Add("Age restriction must be between 0 and 150");

        // Business rule validation
        if (Cost > 0 && Price > 0 && Price < Cost)
            ValidationErrors.Add("Warning: Selling price is lower than cost price");

        // Update validation status
        HasValidationErrors = ValidationErrors.Any();
        ValidationMessage = HasValidationErrors 
            ? string.Join(Environment.NewLine, ValidationErrors)
            : "All validations passed";

        return !HasValidationErrors;
    }

    #endregion

    #region Helper Methods

    private ProductDto CreateProductDto()
    {
        // Calculate markup if both cost and price are provided
        var calculatedMarkup = Markup;
        if (Cost > 0 && Price > Cost)
        {
            calculatedMarkup = ((Price - Cost) / Cost) * 100;
        }

        var productDto = new ProductDto
        {
            Name = Name,
            Description = Description ?? string.Empty,
            SKU = Code,
            Price = Price,
            CategoryId = CategoryId ?? 1, // Default category if none selected
            StockQuantity = 0, // New products start with 0 stock
            IsActive = IsEnabled,
            CostPrice = Cost,
            Markup = calculatedMarkup,
            ImagePath = ImagePath,
            Color = Color,
            // Stock Control Properties
            IsStockTracked = IsStockTracked,
            AllowNegativeStock = AllowNegativeStock,
            IsUsingSerialNumbers = IsUsingSerialNumbers,
            InitialStock = IsUsingSerialNumbers ? 0 : InitialStock, // Serial number products start with 0
            MinimumStock = MinimumStock,
            MaximumStock = MaximumStock,
            ReorderLevel = ReorderLevel,
            ReorderQuantity = ReorderQuantity,
            AverageCost = AverageCost,
            LastCost = AverageCost, // Set last cost same as average cost initially
            // UOM Properties
            UnitOfMeasurementId = SelectedUnitOfMeasurementId > 0 ? SelectedUnitOfMeasurementId : 1, // Default to "Pieces"
            UnitOfMeasurementName = SelectedUnitOfMeasurement?.Name ?? "Pieces",
            UnitOfMeasurementAbbreviation = SelectedUnitOfMeasurement?.Abbreviation ?? "pcs",
            SelectedStoreId = SelectedStoreId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return productDto;
    }

    private string GenerateUniqueBarcode()
    {
        var random = new Random();
        var barcodeTypes = new string[] { "EAN13", "CODE128", "CUSTOM" };
        var selectedType = barcodeTypes[random.Next(barcodeTypes.Length)];

        return selectedType switch
        {
            "EAN13" => GenerateEAN13Barcode(),
            "CODE128" => GenerateCode128Barcode(),
            "CUSTOM" => GenerateCustomBarcode(),
            _ => GenerateCustomBarcode()
        };
    }

    private string GenerateEAN13Barcode()
    {
        // Generate EAN-13 format: 13 digits
        var random = new Random();
        var digits = new int[12]; // First 12 digits, 13th is check digit
        
        // Country code (2-3 digits) - using 123 as example
        digits[0] = 1;
        digits[1] = 2;
        digits[2] = 3;
        
        // Manufacturer code and product code (9 digits)
        for (int i = 3; i < 12; i++)
        {
            digits[i] = random.Next(0, 10);
        }
        
        // Calculate check digit
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += digits[i] * (i % 2 == 0 ? 1 : 3);
        }
        int checkDigit = (10 - (sum % 10)) % 10;
        
        return string.Join("", digits) + checkDigit;
    }

    private string GenerateCode128Barcode()
    {
        // Generate CODE-128 format: Alphanumeric
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var length = random.Next(8, 16); // Variable length
        
        var barcode = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            barcode.Append(chars[random.Next(chars.Length)]);
        }
        
        return barcode.ToString();
    }

    private string GenerateCustomBarcode()
    {
        // Generate custom format based on product info
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var productPrefix = !string.IsNullOrEmpty(Name) ? Name.Substring(0, Math.Min(3, Name.Length)).ToUpper() : "PRD";
        var random = new Random().Next(100, 999);
        
        return $"{productPrefix}{timestamp}{random}";
    }

    #endregion

    #region Property Change Handlers

    partial void OnPriceChanged(decimal value)
    {
        if (Cost > 0 && value > Cost)
        {
            Markup = ((value - Cost) / Cost) * 100;
        }
        ValidateForm();
    }

    partial void OnCostChanged(decimal value)
    {
        if (Price > 0 && value > 0 && Price > value)
        {
            Markup = ((Price - value) / value) * 100;
        }
        ValidateForm();
    }

    partial void OnCodeChanged(string value)
    {
        ValidateForm();
    }

    partial void OnNameChanged(string value)
    {
        ValidateForm();
    }

    #endregion
}
