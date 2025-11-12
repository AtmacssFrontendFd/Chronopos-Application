using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
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
    private readonly IBrandService _brandService;
    private readonly IProductImageService _productImageService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly IDiscountService _discountService;
    private readonly IProductUnitService _productUnitService;
    private readonly ISkuGenerationService _skuGenerationService;
    private readonly IProductBatchService _productBatchService;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly IProductModifierGroupService _modifierGroupService;
    private readonly IProductModifierLinkService _modifierLinkService;
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
    private long selectedUnitOfMeasurementId = 1; // Keep for backwards compatibility during transition

    [ObservableProperty]
    private UnitOfMeasurementDto? selectedUnitOfMeasurement;

    // Multi-UOM Support
    [ObservableProperty]
    private ObservableCollection<ProductUnitDto> productUnits = new();

    [ObservableProperty]
    private bool isTaxInclusivePrice = true;

    [ObservableProperty]
    private decimal taxInclusivePriceValue = 0;

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

    // Flag to prevent recursive tax calculations
    private bool _isCalculatingTax = false;

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

    // Remaining quantity tracking
    [ObservableProperty]
    private decimal remainingQuantity = 0;

    [ObservableProperty]
    private string remainingQuantityMessage = string.Empty;

    // Purchase and Selling Units
    [ObservableProperty]
    private long? purchaseUnitId;

    [ObservableProperty]
    private long? sellingUnitId;

    // Product Grouping
    [ObservableProperty]
    private int? productGroupId;

    [ObservableProperty]
    private string? group;

    // Business Rules (Additional)
    [ObservableProperty]
    private bool canReturn = true;

    [ObservableProperty]
    private bool isGrouped = false;

    // Computed Properties for Stock Control
    public bool IsStockFieldsEnabled => IsStockTracked;
    public bool IsSerialNumbersEnabled => IsStockTracked && IsUsingSerialNumbers;

    // Validation Properties
    [ObservableProperty]
    private Dictionary<string, string> stockValidationErrors = new();

    [ObservableProperty]
    private Dictionary<string, string> productUnitValidationErrors = new();

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
    private ObservableCollection<TaxTypeDto> availableTaxTypes = new();

    [ObservableProperty]
    private ObservableCollection<int> selectedTaxTypeIds = new();
    public List<int> SelectedTaxTypeIdsList => SelectedTaxTypeIds.ToList();

    // Dropdown-based tax selection helper state
    [ObservableProperty]
    private TaxTypeDto? selectedTaxType;

    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> selectedTaxTypes = new();

    // Discount properties
    [ObservableProperty]
    private ObservableCollection<DiscountDto> availableDiscounts = new();

    [ObservableProperty]
    private ObservableCollection<int> selectedDiscountIds = new();
    public List<int> SelectedDiscountIdsList => SelectedDiscountIds.ToList();

    // Dropdown-based discount selection helper state
    [ObservableProperty]
    private DiscountDto? selectedDiscount;

    [ObservableProperty]
    private ObservableCollection<DiscountDto> selectedDiscounts = new();

    [ObservableProperty]
    private ObservableCollection<StoreDto> availableStores = new();

    // Brand properties
    [ObservableProperty]
    private ObservableCollection<BrandItemViewModel> availableBrands = new();

    [ObservableProperty]
    private BrandItemViewModel? selectedBrand;

    // Modifier Groups properties
    [ObservableProperty]
    private ObservableCollection<ProductModifierGroupDto> availableModifierGroups = new();

    [ObservableProperty]
    private ProductModifierGroupDto? selectedModifierGroup;

    [ObservableProperty]
    private ObservableCollection<ProductModifierGroupDto> selectedModifierGroups = new();

    // Product Images properties
    [ObservableProperty]
    private ObservableCollection<ProductImageItemViewModel> productImages = new();

    [ObservableProperty]
    private string newImageUrl = string.Empty;

    [ObservableProperty]
    private string newImageAltText = string.Empty;

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

    // Edit Mode Properties
    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private int productId = 0;

    // Product Batch Properties
    [ObservableProperty]
    private ObservableCollection<ProductBatchDto> productBatches = new();

    [ObservableProperty]
    private bool hasProductBatches = false;

    [ObservableProperty]
    private ProductBatchDto? selectedProductBatch = null;

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
    [ObservableProperty] private string productBatchesTitle = "Product Batches";
    [ObservableProperty] private string modifierGroupsTitle = "Modifier Groups";

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
    
    // Multi-UOM Labels
    [ObservableProperty] private string multiUOMLabel = "Units of Measurement";
    [ObservableProperty] private string uomLabel = "UOM";
    [ObservableProperty] private string qtyInUnitLabel = "Qty in Unit";
    [ObservableProperty] private string costOfUnitLabel = "Cost of Unit";
    [ObservableProperty] private string priceOfUnitLabel = "Price of Unit";
    [ObservableProperty] private string discountAllowedLabel = "Discount Allowed";
    [ObservableProperty] private string isBaseLabel = "Is Base";
    [ObservableProperty] private string actionLabel = "Action";
    
    [ObservableProperty] private string isTaxInclusivePriceLabel = "Tax Inclusive Price";
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

    // Missing localization properties
    [ObservableProperty] private string priceIncludingTaxLabel = "Price Including Tax";
    [ObservableProperty] private string taxTypesLabel = "Tax Types";
    [ObservableProperty] private string selectedTaxTypesLabel = "Selected Tax Types";
    [ObservableProperty] private string discountsLabel = "Product Discounts";
    [ObservableProperty] private string selectedDiscountsLabel = "Selected Product Discounts";
    [ObservableProperty] private string barcodeValueLabel = "Barcode Value:";
    [ObservableProperty] private string addNewBarcodeTitle = "Add New Barcode";
    [ObservableProperty] private string productBarcodesTitle = "Product Barcodes";
    [ObservableProperty] private string noBarcodesMessage = "No barcodes added yet";
    [ObservableProperty] private string addBarcodesInstruction = "Add barcode values above or click Generate";
    [ObservableProperty] private string barcodeTypeLabel = "Type:";
    [ObservableProperty] private string primaryImageLabel = "Primary";
    [ObservableProperty] private string addImageLabel = "Add Image";
    [ObservableProperty] private string noImagesMessage = "No images added yet";
    [ObservableProperty] private string addImagesInstruction = "Click the + button above to add your first image";
    [ObservableProperty] private string chooseColorLabel = "Choose Color:";
    [ObservableProperty] private string selectedColorLabel = "Selected:";
    [ObservableProperty] private string addButtonText = "Add";
    [ObservableProperty] private string generateButtonText = "Generate";

    #endregion

    #endregion

    #region Currency Formatted Properties

    /// <summary>
    /// Formatted selling price with active currency symbol and conversion
    /// NOTE: Price is stored in base currency (AED), displayed in active currency
    /// </summary>
    public string FormattedPrice
    {
        get
        {
            var convertedPrice = _activeCurrencyService.ConvertFromBaseCurrency(Price);
            return _activeCurrencyService.FormatPrice(convertedPrice);
        }
    }

    /// <summary>
    /// Formatted cost price with active currency symbol and conversion
    /// NOTE: Cost is stored in base currency (AED), displayed in active currency
    /// </summary>
    public string FormattedCost
    {
        get
        {
            var convertedCost = _activeCurrencyService.ConvertFromBaseCurrency(Cost);
            return _activeCurrencyService.FormatPrice(convertedCost);
        }
    }

    /// <summary>
    /// Formatted last purchase price with active currency symbol and conversion
    /// NOTE: LastPurchasePrice is stored in base currency (AED), displayed in active currency
    /// </summary>
    public string FormattedLastPurchasePrice
    {
        get
        {
            var convertedPrice = _activeCurrencyService.ConvertFromBaseCurrency(LastPurchasePrice);
            return _activeCurrencyService.FormatPrice(convertedPrice);
        }
    }

    /// <summary>
    /// Formatted tax inclusive price with active currency symbol and conversion
    /// NOTE: Tax inclusive price is stored in base currency (AED), displayed in active currency
    /// </summary>
    public string FormattedTaxInclusivePrice
    {
        get
        {
            var convertedPrice = _activeCurrencyService.ConvertFromBaseCurrency(TaxInclusivePriceValue);
            return _activeCurrencyService.FormatPrice(convertedPrice);
        }
    }

    /// <summary>
    /// Current active currency symbol
    /// </summary>
    public string CurrencySymbol => _activeCurrencyService.CurrencySymbol;

    /// <summary>
    /// Current active currency code
    /// </summary>
    public string CurrencyCode => _activeCurrencyService.CurrencyCode;

    /// <summary>
    /// Current active currency name
    /// </summary>
    public string CurrencyName => _activeCurrencyService.CurrencyName;

    /// <summary>
    /// Exchange rate information for display
    /// Note: Currency conversion disabled - shows active currency only
    /// </summary>
    public string ExchangeRateInfo
    {
        get
        {
            return $"Active Currency: {_activeCurrencyService.CurrencyName} ({_activeCurrencyService.CurrencySymbol})";
        }
    }

    #endregion

    #region Barcode Management Classes

    public class BarcodeItemViewModel : ObservableObject
    {
        private string _value = string.Empty;
        private string _barcodeType = "ean";
        private bool _isNew = true;
        private bool _isDeleted = false;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string BarcodeType
        {
            get => _barcodeType;
            set => SetProperty(ref _barcodeType, value);
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

    public class BrandItemViewModel : ObservableObject
    {
        private int _id;
        private string _name = string.Empty;
        private string _nameArabic = string.Empty;
        private string _description = string.Empty;
        private string _logoUrl = string.Empty;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string NameArabic
        {
            get => _nameArabic;
            set => SetProperty(ref _nameArabic, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string LogoUrl
        {
            get => _logoUrl;
            set => SetProperty(ref _logoUrl, value);
        }

        public override string ToString() => Name;
    }

    public class ProductImageItemViewModel : ObservableObject
    {
        private int _id;
        private string _imageUrl = string.Empty;
        private string _altText = string.Empty;
        private int _sortOrder;
        private bool _isPrimary;
        private bool _isNew = true;
        private bool _isDeleted = false;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string AltText
        {
            get => _altText;
            set => SetProperty(ref _altText, value);
        }

        public int SortOrder
        {
            get => _sortOrder;
            set => SetProperty(ref _sortOrder, value);
        }

        public bool IsPrimary
        {
            get => _isPrimary;
            set 
            { 
                SetProperty(ref _isPrimary, value);
                // Notify parent to handle primary image logic
                OnPrimaryChanged?.Invoke(this, value);
            }
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

        public event EventHandler<bool>? OnPrimaryChanged;
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
        CalculateRemainingQuantity();
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

    private void ValidateProductUnits()
    {
        ProductUnitValidationErrors.Clear();

        // ProductUnits are now optional - only validate if there are any
        if (ProductUnits.Any())
        {
            // Validate that exactly one base unit exists
            var baseUnits = ProductUnits.Where(pu => pu.IsBase).ToList();
            if (baseUnits.Count == 0)
            {
                ProductUnitValidationErrors["BaseUnit"] = "One unit must be marked as the base unit";
            }
            else if (baseUnits.Count > 1)
            {
                ProductUnitValidationErrors["BaseUnit"] = "Only one unit can be marked as the base unit";
            }

            // Validate each ProductUnit
            for (int i = 0; i < ProductUnits.Count; i++)
            {
                var unit = ProductUnits[i];
                
                if (unit.QtyInUnit <= 0)
                {
                    ProductUnitValidationErrors[$"QtyInUnit_{i}"] = $"Quantity in unit must be greater than 0 for {GetUnitName(unit.UnitId)}";
                }
                
                if (unit.PriceOfUnit < 0)
                {
                    ProductUnitValidationErrors[$"PriceOfUnit_{i}"] = $"Price cannot be negative for {GetUnitName(unit.UnitId)}";
                }
                
                if (unit.CostOfUnit < 0)
                {
                    ProductUnitValidationErrors[$"CostOfUnit_{i}"] = $"Cost cannot be negative for {GetUnitName(unit.UnitId)}";
                }
            }

            // Check for duplicate units
            var duplicateUnits = ProductUnits.GroupBy(pu => pu.UnitId)
                                           .Where(g => g.Count() > 1)
                                           .Select(g => g.Key);
            
            foreach (var unitId in duplicateUnits)
            {
                ProductUnitValidationErrors[$"Duplicate_{unitId}"] = $"Unit {GetUnitName(unitId)} is defined multiple times";
            }

            // Validate remaining quantity
            ValidateRemainingQuantity();
        }

        OnPropertyChanged(nameof(ProductUnitValidationErrors));
        OnPropertyChanged(nameof(HasProductUnitValidationErrors));
    }

    public bool HasProductUnitValidationErrors => ProductUnitValidationErrors.Any();

    private string GetUnitName(long unitId)
    {
        return UnitsOfMeasurement.FirstOrDefault(u => u.Id == unitId)?.DisplayName ?? "Unknown";
    }

    #endregion

    #region Automatic Calculations

    /// <summary>
    /// Updates cost and price of units for all UOMs based on base cost and price
    /// </summary>
    private void UpdateProductUnitPricing()
    {
        FileLogger.Log($"🔄 UpdateProductUnitPricing called - Processing {ProductUnits.Count} units");
        FileLogger.Log($"   💰 Base Cost: {Cost}, Base Price: {Price}");
        
        foreach (var productUnit in ProductUnits)
        {
            UpdateProductUnitPricing(productUnit);
        }
        
        FileLogger.Log($"✅ Completed updating all product unit pricing");
    }

    /// <summary>
    /// Updates cost and price of unit for a specific UOM based on conversion factor
    /// </summary>
    [RelayCommand]
    public void UpdateProductUnitPricing(ProductUnitDto productUnit)
    {
        var uom = UnitsOfMeasurement.FirstOrDefault(u => u.Id == productUnit.UnitId);
        if (uom == null) 
        {
            FileLogger.Log($"❌ UOM not found for UnitId: {productUnit.UnitId}");
            return;
        }

        var conversionFactor = uom.ConversionFactor ?? 1;
        var oldCost = productUnit.CostOfUnit;
        var oldPrice = productUnit.PriceOfUnit;
        
        FileLogger.Log($"🔧 Updating pricing for UOM: {uom.DisplayName} (Factor: {conversionFactor})");
        FileLogger.Log($"   📊 Before - Cost: {oldCost}, Price: {oldPrice}");
        
        // Cost of unit = Cost price * conversion factor
        if (Cost > 0)
        {
            productUnit.CostOfUnit = Cost * conversionFactor;
            FileLogger.Log($"   💵 Cost calculation: {Cost} * {conversionFactor} = {productUnit.CostOfUnit}");
        }
        
        // Price of unit = Selling price * conversion factor  
        if (Price > 0)
        {
            productUnit.PriceOfUnit = Price * conversionFactor;
            FileLogger.Log($"   💰 Price calculation: {Price} * {conversionFactor} = {productUnit.PriceOfUnit}");
        }
        
        FileLogger.Log($"   📊 After - Cost: {productUnit.CostOfUnit}, Price: {productUnit.PriceOfUnit}");
        
        // Trigger property change notifications
        OnPropertyChanged(nameof(ProductUnits));
    }

    /// <summary>
    /// Calculates remaining quantity based on initial stock and UOM quantities
    /// </summary>
    [RelayCommand]
    public void CalculateRemainingQuantity()
    {
        if (InitialStock <= 0)
        {
            RemainingQuantity = 0;
            RemainingQuantityMessage = string.Empty;
            return;
        }

        decimal usedQuantity = 0;
        
        foreach (var productUnit in ProductUnits)
        {
            var uom = UnitsOfMeasurement.FirstOrDefault(u => u.Id == productUnit.UnitId);
            if (uom == null) continue;
            
            var conversionFactor = uom.ConversionFactor ?? 1;
            usedQuantity += productUnit.QtyInUnit * conversionFactor;
        }

        RemainingQuantity = InitialStock - usedQuantity;
        
        if (InitialStock > 0)
        {
            RemainingQuantityMessage = $"Remaining quantity: {RemainingQuantity:F0} units";
        }
        else
        {
            RemainingQuantityMessage = string.Empty;
        }
    }

    /// <summary>
    /// Validates that remaining quantity is not negative
    /// </summary>
    private bool ValidateRemainingQuantity()
    {
        CalculateRemainingQuantity();
        
        if (RemainingQuantity < 0)
        {
            StockValidationErrors["RemainingQuantity"] = "Total UOM quantities exceed initial stock. Please adjust quantities or increase initial stock.";
            return false;
        }
        else
        {
            StockValidationErrors.Remove("RemainingQuantity");
            return true;
        }
    }

    /// <summary>
    /// Called when a ProductUnit's UnitId changes to recalculate pricing
    /// </summary>
    public async void OnProductUnitUOMChanged(ProductUnitDto productUnit)
    {
        if (productUnit != null)
        {
            // Regenerate SKU when UOM changes
            try
            {
                var uom = UnitsOfMeasurement.FirstOrDefault(u => u.Id == productUnit.UnitId);
                if (uom != null && !string.IsNullOrWhiteSpace(Name))
                {
                    int tempProductId = ProductId > 0 ? ProductId : ProductUnits.IndexOf(productUnit) + 1;
                    string newSku = await _skuGenerationService.GenerateProductUnitSkuAsync(
                        tempProductId,
                        Name,
                        uom.Id,
                        uom.Name,
                        productUnit.QtyInUnit);
                    
                    productUnit.Sku = newSku;
                    FileLogger.Log($"   🔄 Regenerated SKU for UOM change: {newSku}");
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"   ❌ Error regenerating SKU: {ex.Message}");
            }
            
            UpdateProductUnitPricing(productUnit);
            CalculateRemainingQuantity();
            ValidateProductUnits();
        }
    }

    /// <summary>
    /// Called when a ProductUnit's QtyInUnit changes to recalculate remaining quantity
    /// </summary>
    public void OnProductUnitQuantityChanged(ProductUnitDto productUnit)
    {
        if (productUnit != null)
        {
            CalculateRemainingQuantity();
            ValidateProductUnits();
        }
    }

    #endregion

    #region Constructor

    public AddProductViewModel(
        IProductService productService,
        IBrandService brandService,
    IProductImageService productImageService,
    ITaxTypeService taxTypeService,
    IDiscountService discountService,
        IProductUnitService productUnitService,
        ISkuGenerationService skuGenerationService,
        IProductBatchService productBatchService,
        IActiveCurrencyService activeCurrencyService,
        IProductModifierGroupService modifierGroupService,
        IProductModifierLinkService modifierLinkService,
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
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
    _productImageService = productImageService ?? throw new ArgumentNullException(nameof(productImageService));
    _taxTypeService = taxTypeService ?? throw new ArgumentNullException(nameof(taxTypeService));
    _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _productUnitService = productUnitService ?? throw new ArgumentNullException(nameof(productUnitService));
        _skuGenerationService = skuGenerationService ?? throw new ArgumentNullException(nameof(skuGenerationService));
        _productBatchService = productBatchService ?? throw new ArgumentNullException(nameof(productBatchService));
        _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
        _modifierGroupService = modifierGroupService ?? throw new ArgumentNullException(nameof(modifierGroupService));
        _modifierLinkService = modifierLinkService ?? throw new ArgumentNullException(nameof(modifierLinkService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _navigateBack = navigateBack;
        
        // Subscribe to currency change events
        _activeCurrencyService.ActiveCurrencyChanged += OnCurrencyChanged;
        FileLogger.Log($"💱 Subscribed to currency changes. Current: {_activeCurrencyService.CurrencySymbol} {_activeCurrencyService.CurrencyCode}");
        
        // Initialize with default values
        Code = GenerateNextCode();
        Name = "Test Product"; // Debug: Set a default name to see if binding works
        IsTaxInclusivePrice = true;
        IsEnabled = true;
        IsDiscountAllowed = true;
        MaxDiscount = 100;
        SelectedUnitOfMeasurementId = 1; // Default to "Pieces"
        Color = "#FFC107";
        
        FileLogger.Log($"🏗️ AddProductViewModel constructor - NOT adding default ProductUnit");
        
        // Subscribe to ProductUnits collection changes for validation
        ProductUnits.CollectionChanged += (s, e) => ValidateProductUnits();
        
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
            FileLogger.Log("🌐 Ensuring AddProduct translation keywords are in database");
            await AddProductTranslations.EnsureTranslationKeywordsAsync(_databaseLocalizationService);
            FileLogger.Log("✅ AddProduct translation keywords ensured");
            
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
            
            FileLogger.Log("🏷️ Loading brands");
            await LoadBrandsAsync();
            FileLogger.Log($"✅ Loaded {AvailableBrands.Count} brands");
            
            FileLogger.Log("� Loading modifier groups");
            await LoadModifierGroupsAsync();
            FileLogger.Log($"✅ Loaded {AvailableModifierGroups.Count} modifier groups");
            
            FileLogger.Log("�📏 Loading units of measurement");
            await LoadUnitsOfMeasurementAsync();
            FileLogger.Log($"✅ Loaded units of measurement");

            FileLogger.Log("🏦 Loading tax types");
            await LoadTaxTypesAsync();
            await LoadDiscountsAsync();
            FileLogger.Log("✅ Loaded tax types");

            // Initialize calculations after data is loaded
            FileLogger.Log("🧮 Initializing calculations");
            UpdateProductUnitPricing();
            CalculateRemainingQuantity();
            FileLogger.Log("✅ Initial calculations completed");

            StatusMessage = "Ready to create new product";
            FileLogger.Log($"🎯 Final status: {StatusMessage}");
            FileLogger.Log("🎉 AddProductViewModel initialization completed successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ ERROR in AddProductViewModel initialization: {ex.Message}");
            FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
            StatusMessage = $"Error loading data: {ex.Message}";
            var errorDialog = new MessageDialog("Error", $"Failed to load data: {ex.Message}", MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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

    private async Task LoadBrandsAsync()
    {
        try
        {
            // Load brands from database using BrandService
            AvailableBrands.Clear();
            
            // Add "No Brand" option
            AvailableBrands.Add(new BrandItemViewModel
            {
                Id = 0,
                Name = "No Brand",
                NameArabic = "بدون ماركة",
                Description = "No brand selected"
            });
            
            // Load brands from BrandService
            var brands = await _brandService.GetAllAsync();
            foreach (var brand in brands)
            {
                AvailableBrands.Add(new BrandItemViewModel
                {
                    Id = brand.Id,
                    Name = brand.Name,
                    NameArabic = brand.NameArabic ?? string.Empty,
                    Description = brand.Description ?? string.Empty
                });
            }
            
            FileLogger.Log($"✅ Loaded {AvailableBrands.Count} brands (including 'No Brand' option)");
            
            // Set default to "No Brand"
            SelectedBrand = AvailableBrands.FirstOrDefault();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error loading brands: {ex.Message}");
            StatusMessage = $"Error loading brands: {ex.Message}";
            
            // Fallback to default brands on error
            AvailableBrands.Clear();
            AvailableBrands.Add(new BrandItemViewModel
            {
                Id = 0,
                Name = "No Brand",
                NameArabic = "بدون ماركة",
                Description = "No brand selected"
            });
            SelectedBrand = AvailableBrands.FirstOrDefault();
        }
    }

    private async Task LoadModifierGroupsAsync()
    {
        try
        {
            // Load active modifier groups from database
            AvailableModifierGroups.Clear();
            
            var modifierGroups = await _modifierGroupService.GetActiveAsync();
            foreach (var group in modifierGroups)
            {
                AvailableModifierGroups.Add(group);
            }
            
            FileLogger.Log($"✅ Loaded {AvailableModifierGroups.Count} active modifier groups");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error loading modifier groups: {ex.Message}");
            StatusMessage = $"Error loading modifier groups: {ex.Message}";
        }
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

    private async Task LoadTaxTypesAsync()
    {
        try
        {
            var types = await _taxTypeService.GetAllAsync();
            AvailableTaxTypes.Clear();
            foreach (var t in types)
            {
                AvailableTaxTypes.Add(t);
            }
            // Initialize selected list from ids if any
            if (SelectedTaxTypeIds?.Any() == true)
            {
                SelectedTaxTypes.Clear();
                foreach (var id in SelectedTaxTypeIds)
                {
                    var match = AvailableTaxTypes.FirstOrDefault(x => x.Id == id);
                    if (match != null && !SelectedTaxTypes.Any(x => x.Id == match.Id))
                        SelectedTaxTypes.Add(match);
                }
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error loading tax types: {ex.Message}");
        }
    }

    private async Task LoadDiscountsAsync()
    {
        try
        {
            var discounts = await _discountService.GetActiveDiscountsAsync();
            AvailableDiscounts.Clear();
            foreach (var discount in discounts)
            {
                AvailableDiscounts.Add(discount);
            }
            // Initialize selected list from ids if any
            if (SelectedDiscountIds?.Any() == true)
            {
                SelectedDiscounts.Clear();
                foreach (var id in SelectedDiscountIds)
                {
                    var match = AvailableDiscounts.FirstOrDefault(x => x.Id == id);
                    if (match != null && !SelectedDiscounts.Any(x => x.Id == match.Id))
                        SelectedDiscounts.Add(match);
                }
            }
            
            // Apply stackable filtering based on current selection
            FilterAvailableDiscounts();
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error loading discounts: {ex.Message}");
        }
    }

    private string GenerateNextCode()
    {
        // Generate a simple auto-incrementing code
        // In a real application, this would query the database for the next available code
        return $"PROD{DateTime.Now:yyyyMMddHHmmss}";
    }

    public async Task LoadProductForEdit(ProductDto product)
    {
        try
        {
            FileLogger.LogSeparator("LoadProductForEdit");
            FileLogger.Log($"🔄 Loading product for edit: {product.Name} (ID: {product.Id})");
            
            IsEditMode = true;
            ProductId = product.Id;
            
            // Fill form with product data
            Code = product.SKU ?? string.Empty;
            Name = product.Name;
            Description = product.Description ?? string.Empty;
            Price = product.Price;
            Cost = product.CostPrice;
            Markup = product.Markup ?? 0;
            CategoryId = product.CategoryId;
            IsEnabled = product.IsActive;
            ImagePath = product.ImagePath ?? string.Empty;
            Color = product.Color ?? "#FFC107";
            
            // Tax & Pricing attributes (Missing fields!)
            IsTaxInclusivePrice = product.IsTaxInclusivePrice;
            TaxInclusivePriceValue = product.TaxInclusivePriceValue;
            IsDiscountAllowed = product.IsDiscountAllowed;
            MaxDiscount = product.MaxDiscount;
            IsPriceChangeAllowed = product.IsPriceChangeAllowed;
            IsService = product.IsService;
            AgeRestriction = product.AgeRestriction;
            
            // Stock control properties
            IsStockTracked = product.IsStockTracked;
            AllowNegativeStock = product.AllowNegativeStock;
            IsUsingSerialNumbers = product.IsUsingSerialNumbers;
            InitialStock = product.InitialStock;
            MinimumStock = product.MinimumStock;
            MaximumStock = product.MaximumStock;
            ReorderLevel = product.ReorderLevel;
            ReorderQuantity = product.ReorderQuantity;
            AverageCost = product.AverageCost;
            
            // Unit of measurement
            if (product.UnitOfMeasurementId > 0)
            {
                SelectedUnitOfMeasurementId = product.UnitOfMeasurementId;
                SelectedUnitOfMeasurement = UnitsOfMeasurement.FirstOrDefault(u => u.Id == product.UnitOfMeasurementId);
            }
            
            // Purchase and Selling Units
            PurchaseUnitId = product.PurchaseUnitId;
            SellingUnitId = product.SellingUnitId;
            
            // Product Grouping
            ProductGroupId = product.ProductGroupId;
            Group = product.Group;
            
            // Business Rules (Additional)
            CanReturn = product.CanReturn;
            IsGrouped = product.IsGrouped;
            
            // Load barcodes if any
            Barcodes.Clear();
            FileLogger.Log($"🔄 Loading barcodes for product ID: {product.Id}");
            
            if (product.ProductBarcodes?.Any() == true)
            {
                // Load from ProductBarcodes table (multiple barcodes)
                foreach (var productBarcode in product.ProductBarcodes)
                {
                    Barcodes.Add(new BarcodeItemViewModel 
                    { 
                        Value = productBarcode.Barcode,
                        BarcodeType = productBarcode.BarcodeType ?? "ean",
                        IsNew = false 
                    });
                }
                FileLogger.Log($"✅ Loaded {Barcodes.Count} barcodes from ProductBarcodes table");
            }
            else if (!string.IsNullOrEmpty(product.Barcode))
            {
                // Fallback: Load from legacy single barcode field
                Barcodes.Add(new BarcodeItemViewModel 
                { 
                    Value = product.Barcode,
                    BarcodeType = "ean", // Default type for legacy barcodes
                    IsNew = false 
                });
                FileLogger.Log($"✅ Loaded 1 legacy barcode from Product.Barcode field");
            }
            else
            {
                FileLogger.Log($"ℹ️ No barcodes found for product ID: {product.Id}");
            }
            
            // Load brand if any
            if (product.BrandId.HasValue && product.BrandId > 0)
            {
                SelectedBrand = AvailableBrands.FirstOrDefault(b => b.Id == product.BrandId.Value);
            }
            
            // Load product images if any
            ProductImages.Clear();
            try
            {
                var productImages = await _productImageService.GetByProductIdAsync(product.Id);
                foreach (var imageDto in productImages)
                {
                    var imageItemViewModel = new ProductImageItemViewModel
                    {
                        Id = imageDto.Id,
                        ImageUrl = imageDto.ImageUrl,
                        AltText = imageDto.AltText ?? string.Empty,
                        SortOrder = imageDto.SortOrder,
                        IsPrimary = imageDto.IsPrimary,
                        IsNew = false
                    };
                    
                    // Handle primary image logic
                    imageItemViewModel.OnPrimaryChanged += HandlePrimaryImageChanged;
                    ProductImages.Add(imageItemViewModel);
                }
                
                FileLogger.Log($"✅ Loaded {ProductImages.Count} product images");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"⚠️ Error loading product images: {ex.Message}");
                // Don't fail the entire operation if images can't be loaded
            }
            
            // Load selected tax types
            SelectedTaxTypeIds = new ObservableCollection<int>(product.SelectedTaxTypeIds ?? new List<int>());
            // Build SelectedTaxTypes for chip list
            SelectedTaxTypes.Clear();
            foreach (var id in SelectedTaxTypeIds)
            {
                var match = AvailableTaxTypes.FirstOrDefault(x => x.Id == id);
                if (match != null && !SelectedTaxTypes.Any(x => x.Id == match.Id))
                    SelectedTaxTypes.Add(match);
            }
            
            // Load selected discounts
            SelectedDiscountIds = new ObservableCollection<int>(product.SelectedDiscountIds ?? new List<int>());
            // Build SelectedDiscounts for chip list
            SelectedDiscounts.Clear();
            foreach (var id in SelectedDiscountIds)
            {
                var match = AvailableDiscounts.FirstOrDefault(x => x.Id == id);
                if (match != null && !SelectedDiscounts.Any(x => x.Id == match.Id))
                    SelectedDiscounts.Add(match);
            }
            
            // Load ProductUnits for edit mode
            try
            {
                FileLogger.Log($"🔄 Loading ProductUnits for product ID: {product.Id}");
                ProductUnits.Clear();
                
                var existingProductUnits = await _productUnitService.GetByProductIdAsync(product.Id);
                foreach (var productUnit in existingProductUnits)
                {
                    var uom = UnitsOfMeasurement.FirstOrDefault(u => u.Id == productUnit.UnitId);
                    if (uom != null)
                    {
                        ProductUnits.Add(new ProductUnitDto
                        {
                            Id = (int)productUnit.Id,
                            ProductId = (int)productUnit.ProductId,
                            UnitId = productUnit.UnitId,
                            UnitName = uom.Name,
                            UnitAbbreviation = uom.Abbreviation,
                            QtyInUnit = productUnit.QtyInUnit,
                            CostOfUnit = productUnit.CostOfUnit,
                            PriceOfUnit = productUnit.PriceOfUnit,
                            DiscountAllowed = productUnit.DiscountAllowed,
                            IsBase = productUnit.IsBase,
                            Sku = productUnit.Sku
                        });
                    }
                }
                
                FileLogger.Log($"✅ Loaded {ProductUnits.Count} ProductUnits for edit mode");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"⚠️ Error loading ProductUnits: {ex.Message}");
                // Don't fail the entire operation if ProductUnits can't be loaded
            }
            
            // Note: No need to recalculate tax-inclusive price here since we already loaded 
            // the saved value from product.TaxInclusivePriceValue above

            // Load modifier group links for edit mode
            try
            {
                FileLogger.Log($"🔄 Loading modifier group links for product ID: {product.Id}");
                SelectedModifierGroups.Clear();
                
                var productModifierLinks = await _modifierLinkService.GetByProductIdAsync(product.Id);
                foreach (var link in productModifierLinks)
                {
                    var modifierGroup = AvailableModifierGroups.FirstOrDefault(mg => mg.Id == link.ModifierGroupId);
                    if (modifierGroup != null && !SelectedModifierGroups.Any(mg => mg.Id == modifierGroup.Id))
                    {
                        SelectedModifierGroups.Add(modifierGroup);
                    }
                }
                
                FileLogger.Log($"✅ Loaded {SelectedModifierGroups.Count} modifier group links for edit mode");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"⚠️ Error loading modifier group links: {ex.Message}");
                // Don't fail the entire operation if modifier groups can't be loaded
            }

            // Load product batches for edit mode
            await LoadProductBatchesAsync();

            // Update title and button text for edit mode
            await UpdateTitleForEditMode();
            
            StatusMessage = $"Editing product: {product.Name}";
            FileLogger.Log("✅ Product loaded for edit successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading product for edit: {ex.Message}";
            FileLogger.Log($"❌ Error loading product for edit: {ex.Message}");
        }
    }

    private async Task UpdateTitleForEditMode()
    {
        if (IsEditMode)
        {
            AddProductTitle = await GetTranslationAsync("edit_product_title", "Edit Product");
            SaveChangesButtonText = await GetTranslationAsync("save_changes_button", "Save Changes");
        }
        else
        {
            AddProductTitle = await GetTranslationAsync("add_product_title", "Add New Product");
            SaveChangesButtonText = await GetTranslationAsync("save_button", "Save Product");
        }
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
        ModifierGroupsTitle = await GetTranslationAsync("modifier_groups_title", "Modifier Groups");

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
        
        // Multi-UOM Labels
        MultiUOMLabel = await GetTranslationAsync("multi_uom_label", "Units of Measurement");
        UomLabel = await GetTranslationAsync("uom_label", "UOM");
        QtyInUnitLabel = await GetTranslationAsync("qty_in_unit_label", "Qty in Unit");
        CostOfUnitLabel = await GetTranslationAsync("cost_of_unit_label", "Cost of Unit");
        PriceOfUnitLabel = await GetTranslationAsync("price_of_unit_label", "Price of Unit");
        DiscountAllowedLabel = await GetTranslationAsync("discount_allowed_label", "Discount Allowed");
        IsBaseLabel = await GetTranslationAsync("is_base_label", "Is Base");
        ActionLabel = await GetTranslationAsync("action_label", "Action");
        
        IsTaxInclusivePriceLabel = await GetTranslationAsync("tax_inclusive_price_label", "Tax Inclusive Price");
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

        // Missing localization properties
        PriceIncludingTaxLabel = await GetTranslationAsync("price_including_tax_label", "Price Including Tax");
        TaxTypesLabel = await GetTranslationAsync("tax_types_label", "Tax Types");
        SelectedTaxTypesLabel = await GetTranslationAsync("selected_tax_types_label", "Selected Tax Types");
        DiscountsLabel = await GetTranslationAsync("discounts_label", "Product Discounts");
        SelectedDiscountsLabel = await GetTranslationAsync("selected_discounts_label", "Selected Product Discounts");
        BarcodeValueLabel = await GetTranslationAsync("barcode_value_label", "Barcode Value:");
        AddNewBarcodeTitle = await GetTranslationAsync("add_new_barcode_title", "Add New Barcode");
        ProductBarcodesTitle = await GetTranslationAsync("product_barcodes_title", "Product Barcodes");
        NoBarcodesMessage = await GetTranslationAsync("no_barcodes_message", "No barcodes added yet");
        AddBarcodesInstruction = await GetTranslationAsync("add_barcodes_instruction", "Add barcode values above or click Generate");
        BarcodeTypeLabel = await GetTranslationAsync("barcode_type_label", "Type:");
        PrimaryImageLabel = await GetTranslationAsync("primary_image_label", "Primary");
        AddImageLabel = await GetTranslationAsync("add_image_label", "Add Image");
        NoImagesMessage = await GetTranslationAsync("no_images_message", "No images added yet");
        AddImagesInstruction = await GetTranslationAsync("add_images_instruction", "Click the + button above to add your first image");
        ChooseColorLabel = await GetTranslationAsync("choose_color_label", "Choose Color:");
        SelectedColorLabel = await GetTranslationAsync("selected_color_label", "Selected:");
        AddButtonText = await GetTranslationAsync("add_button_text", "Add");
        GenerateButtonText = await GetTranslationAsync("generate_button_text", "Generate");
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
            
            // Missing localized properties
            OnPropertyChanged(nameof(PriceIncludingTaxLabel));
            OnPropertyChanged(nameof(TaxTypesLabel));
            OnPropertyChanged(nameof(SelectedTaxTypesLabel));
            OnPropertyChanged(nameof(DiscountsLabel));
            OnPropertyChanged(nameof(SelectedDiscountsLabel));
            OnPropertyChanged(nameof(BarcodeValueLabel));
            OnPropertyChanged(nameof(AddNewBarcodeTitle));
            OnPropertyChanged(nameof(ProductBarcodesTitle));
            OnPropertyChanged(nameof(NoBarcodesMessage));
            OnPropertyChanged(nameof(AddBarcodesInstruction));
            OnPropertyChanged(nameof(BarcodeTypeLabel));
            OnPropertyChanged(nameof(PrimaryImageLabel));
            OnPropertyChanged(nameof(AddImageLabel));
            OnPropertyChanged(nameof(NoImagesMessage));
            OnPropertyChanged(nameof(AddImagesInstruction));
            OnPropertyChanged(nameof(ChooseColorLabel));
            OnPropertyChanged(nameof(SelectedColorLabel));
            OnPropertyChanged(nameof(AddButtonText));
            OnPropertyChanged(nameof(GenerateButtonText));
            
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
        _activeCurrencyService.ActiveCurrencyChanged -= OnCurrencyChanged;
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
    private void AddSelectedTaxType()
    {
        if (SelectedTaxType == null) return;
        var t = SelectedTaxType;
        // Avoid duplicates
        if (!SelectedTaxTypeIds.Contains(t.Id))
            SelectedTaxTypeIds.Add(t.Id);
        if (!SelectedTaxTypes.Any(x => x.Id == t.Id))
            SelectedTaxTypes.Add(t);
        // Clear dropdown selection for quick add of next
        SelectedTaxType = null;
        StatusMessage = $"Added tax type: {t.Name}";
        CalculateTaxInclusivePrice();
    }

    [RelayCommand]
    private void RemoveTaxType(TaxTypeDto? tax)
    {
        if (tax == null) return;
        // Remove from both collections
        var removed = SelectedTaxTypes.FirstOrDefault(x => x.Id == tax.Id);
        if (removed != null)
            SelectedTaxTypes.Remove(removed);
        var idIndex = SelectedTaxTypeIds.IndexOf(tax.Id);
        if (idIndex >= 0)
            SelectedTaxTypeIds.RemoveAt(idIndex);
        StatusMessage = $"Removed tax type: {tax.Name}";
        CalculateTaxInclusivePrice();
    }

    [RelayCommand]
    private void AddSelectedDiscount()
    {
        if (SelectedDiscount == null) return;
        
        var discountToAdd = SelectedDiscount;
        
        // Check stackable business rules
        var validationResult = ValidateDiscountAddition(discountToAdd);
        if (!validationResult.IsValid)
        {
            StatusMessage = validationResult.ErrorMessage;
            var warningDialog = new MessageDialog("Cannot Add Discount", validationResult.ErrorMessage, MessageDialog.MessageType.Warning);
            warningDialog.ShowDialog();
            return;
        }
        
        // Avoid duplicates
        if (!SelectedDiscountIds.Contains(discountToAdd.Id))
            SelectedDiscountIds.Add(discountToAdd.Id);
        if (!SelectedDiscounts.Any(x => x.Id == discountToAdd.Id))
            SelectedDiscounts.Add(discountToAdd);
            
        // Clear dropdown selection for quick add of next
        SelectedDiscount = null;
        StatusMessage = $"Added discount: {discountToAdd.DiscountName}";
        
        // Update available discounts based on new selection
        FilterAvailableDiscounts();
    }

    [RelayCommand]
    private void RemoveDiscount(DiscountDto? discount)
    {
        if (discount == null) return;
        // Remove from both collections
        var removed = SelectedDiscounts.FirstOrDefault(x => x.Id == discount.Id);
        if (removed != null)
            SelectedDiscounts.Remove(removed);
        var idIndex = SelectedDiscountIds.IndexOf(discount.Id);
        if (idIndex >= 0)
            SelectedDiscountIds.RemoveAt(idIndex);
        StatusMessage = $"Removed discount: {discount.DiscountName}";
        
        // Update available discounts after removal
        FilterAvailableDiscounts();
    }

    /// <summary>
    /// Validates whether a discount can be added based on stackable business rules
    /// </summary>
    private (bool IsValid, string ErrorMessage) ValidateDiscountAddition(DiscountDto discountToAdd)
    {
        // Rule 1: Cannot add duplicate discounts
        if (SelectedDiscounts.Any(d => d.Id == discountToAdd.Id))
        {
            return (false, "This discount is already selected.");
        }

        // Rule 2: If trying to add a non-stackable discount
        if (!discountToAdd.IsStackable)
        {
            // Cannot add if there are already any discounts selected
            if (SelectedDiscounts.Any())
            {
                return (false, "Cannot add non-stackable discount when other discounts are already selected. Remove existing discounts first.");
            }
        }
        
        // Rule 3: If trying to add a stackable discount
        if (discountToAdd.IsStackable)
        {
            // Cannot add if there's already a non-stackable discount selected
            var hasNonStackableDiscount = SelectedDiscounts.Any(d => !d.IsStackable);
            if (hasNonStackableDiscount)
            {
                return (false, "Cannot add stackable discount when a non-stackable discount is already selected. Remove the non-stackable discount first.");
            }
            // Stackable + Stackable is allowed, so continue
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Filters available discounts based on current selection and stackable rules
    /// </summary>
    private async void FilterAvailableDiscounts()
    {
        try
        {
            // Get all active discounts fresh from service
            var allActiveDiscounts = await _discountService.GetActiveDiscountsAsync();
            AvailableDiscounts.Clear();

            // If no discounts selected, show all active discounts
            if (!SelectedDiscounts.Any())
            {
                foreach (var discount in allActiveDiscounts.Where(d => d.IsCurrentlyActive))
                {
                    AvailableDiscounts.Add(discount);
                }
                return;
            }

            // If there's a non-stackable discount selected, show no available discounts
            var hasNonStackableSelected = SelectedDiscounts.Any(d => !d.IsStackable);
            if (hasNonStackableSelected)
            {
                // No discounts can be added
                return;
            }

            // If only stackable discounts are selected, show only other stackable discounts
            // (since stackable + stackable is allowed, but stackable + non-stackable is not allowed)
            foreach (var discount in allActiveDiscounts.Where(d => d.IsCurrentlyActive && d.IsStackable && !SelectedDiscounts.Any(s => s.Id == d.Id)))
            {
                AvailableDiscounts.Add(discount);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error filtering available discounts: {ex.Message}");
        }
    }

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
            BarcodeType = "ean", // Default barcode type
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

    #region Image Management Commands

    [RelayCommand]
    private void AddProductImage()
    {
        FileLogger.Log($"🖼️ AddProductImage called - NewImageUrl: '{NewImageUrl}', NewImageAltText: '{NewImageAltText}'");
        
        if (string.IsNullOrWhiteSpace(NewImageUrl))
        {
            StatusMessage = "Image URL cannot be empty";
            FileLogger.Log("⚠️ AddProductImage aborted: Image URL is empty");
            return;
        }

        var trimmedUrl = NewImageUrl.Trim();
        var altText = string.IsNullOrWhiteSpace(NewImageAltText) ? "Product Image" : NewImageAltText.Trim();

        // Get next sort order
        var nextSortOrder = ProductImages.Any() ? ProductImages.Max(i => i.SortOrder) + 1 : 1;
        
        FileLogger.Log($"📊 Current ProductImages count: {ProductImages.Count}, Next sort order: {nextSortOrder}");

        var imageItem = new ProductImageItemViewModel
        {
            ImageUrl = trimmedUrl,
            AltText = altText,
            SortOrder = nextSortOrder,
            IsPrimary = !ProductImages.Any(), // First image is primary by default
            IsNew = true
        };

        // Handle primary image logic
        imageItem.OnPrimaryChanged += HandlePrimaryImageChanged;

        ProductImages.Add(imageItem);
        FileLogger.Log($"✅ Added image to ProductImages collection - Total count now: {ProductImages.Count}");
        FileLogger.Log($"   🏷️ Image details: URL='{trimmedUrl}', AltText='{altText}', SortOrder={nextSortOrder}, IsPrimary={imageItem.IsPrimary}");
        
        NewImageUrl = string.Empty;
        NewImageAltText = string.Empty;

        StatusMessage = $"Product image added successfully";
    }

    [RelayCommand]
    private void RemoveProductImage(ProductImageItemViewModel? image)
    {
        if (image != null)
        {
            ProductImages.Remove(image);
            
            // If removed image was primary, make first image primary
            if (image.IsPrimary && ProductImages.Any())
            {
                ProductImages.First().IsPrimary = true;
            }

            StatusMessage = "Product image removed";
        }
    }

    [RelayCommand]
    private void SetPrimaryImage(ProductImageItemViewModel? image)
    {
        if (image != null && ProductImages.Contains(image))
        {
            // Clear all primary flags
            foreach (var img in ProductImages)
            {
                img.IsPrimary = false;
            }
            
            // Set selected as primary
            image.IsPrimary = true;
            StatusMessage = "Primary image updated";
        }
    }

    private void HandlePrimaryImageChanged(object? sender, bool isPrimary)
    {
        if (isPrimary && sender is ProductImageItemViewModel selectedImage)
        {
            // Clear all other primary flags
            foreach (var img in ProductImages.Where(i => i != selectedImage))
            {
                img.IsPrimary = false;
            }
        }
    }

    [RelayCommand]
    private void MoveImageUp(ProductImageItemViewModel? image)
    {
        if (image != null && ProductImages.Contains(image))
        {
            var currentIndex = ProductImages.IndexOf(image);
            if (currentIndex > 0)
            {
                // Swap sort orders
                var previousImage = ProductImages[currentIndex - 1];
                var tempSortOrder = image.SortOrder;
                image.SortOrder = previousImage.SortOrder;
                previousImage.SortOrder = tempSortOrder;
                
                // Move in collection
                ProductImages.Move(currentIndex, currentIndex - 1);
                StatusMessage = "Image moved up";
            }
        }
    }

    [RelayCommand]
    private void MoveImageDown(ProductImageItemViewModel? image)
    {
        if (image != null && ProductImages.Contains(image))
        {
            var currentIndex = ProductImages.IndexOf(image);
            if (currentIndex < ProductImages.Count - 1)
            {
                // Swap sort orders
                var nextImage = ProductImages[currentIndex + 1];
                var tempSortOrder = image.SortOrder;
                image.SortOrder = nextImage.SortOrder;
                nextImage.SortOrder = tempSortOrder;
                
                // Move in collection
                ProductImages.Move(currentIndex, currentIndex + 1);
                StatusMessage = "Image moved down";
            }
        }
    }

    #region Multi-UOM Commands

    [RelayCommand]
    private async void AddProductUnit()
    {
        FileLogger.Log($"➕ AddProductUnit called - Current units count: {ProductUnits.Count}");
        
        var firstUom = UnitsOfMeasurement.FirstOrDefault();
        FileLogger.Log($"   🎯 First available UOM: {firstUom?.DisplayName ?? "None"} (ID: {firstUom?.Id ?? 0})");
        
        // Generate SKU for the new product unit
        string generatedSku = "TEMP-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        try
        {
            if (firstUom != null && !string.IsNullOrWhiteSpace(Name))
            {
                // Use a temporary product ID for SKU generation (we'll use current count + 1)
                int tempProductId = ProductId > 0 ? ProductId : ProductUnits.Count + 1;
                generatedSku = await _skuGenerationService.GenerateProductUnitSkuAsync(
                    tempProductId,
                    Name,
                    firstUom.Id,
                    firstUom.Name,
                    1);
                FileLogger.Log($"   🏷️ Generated SKU: {generatedSku}");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"   ❌ Error generating SKU: {ex.Message}");
            // Keep the temporary SKU if generation fails
        }
        
        var newProductUnit = new ProductUnitDto
        {
            Id = 0, // New item
            ProductId = 0, // Will be set when product is saved
            UnitId = firstUom?.Id ?? 1,
            QtyInUnit = 1,
            CostOfUnit = 0,
            PriceOfUnit = 0,
            DiscountAllowed = false,
            IsBase = ProductUnits.Count == 0, // First one is base by default
            Sku = generatedSku
        };
        
        FileLogger.Log($"   📦 Created new ProductUnit - UnitId: {newProductUnit.UnitId}, IsBase: {newProductUnit.IsBase}, SKU: {newProductUnit.Sku}");
        
        // Auto-calculate pricing based on conversion factor
        UpdateProductUnitPricing(newProductUnit);
        
        ProductUnits.Add(newProductUnit);
        FileLogger.Log($"   ✅ Added to collection - New count: {ProductUnits.Count}");
        
        // Update remaining quantity calculation
        CalculateRemainingQuantity();
        
        StatusMessage = "New UOM added";
        FileLogger.Log($"✅ AddProductUnit completed");
    }

    [RelayCommand]
    private void RemoveProductUnit(ProductUnitDto? productUnit)
    {
        if (productUnit != null && ProductUnits.Contains(productUnit))
        {
            ProductUnits.Remove(productUnit);
            
            // If we removed the base unit, set the first remaining as base
            if (productUnit.IsBase && ProductUnits.Any())
            {
                ProductUnits.First().IsBase = true;
            }
            
            // Update remaining quantity calculation
            CalculateRemainingQuantity();
            
            StatusMessage = "UOM removed";
        }
    }

    [RelayCommand]
    private void SetBaseUnit(ProductUnitDto? productUnit)
    {
        if (productUnit != null && ProductUnits.Contains(productUnit))
        {
            // Clear all base flags first
            foreach (var unit in ProductUnits)
            {
                unit.IsBase = false;
            }
            
            // Set the selected unit as base
            productUnit.IsBase = true;
            StatusMessage = $"Base unit set to {UnitsOfMeasurement.FirstOrDefault(u => u.Id == productUnit.UnitId)?.DisplayName ?? "Unknown"}";
        }
    }

    #endregion

    #endregion

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
            FileLogger.Log($"🖼️ ChooseImage called - Current ProductImages count: {ProductImages.Count}");
            
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
                FileLogger.Log($"📁 Selected file: {selectedFile}");
                
                // Validate file size (max 5MB)
                var fileInfo = new FileInfo(selectedFile);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    FileLogger.Log($"⚠️ File too large: {fileInfo.Length} bytes");
                    var warningDialog = new MessageDialog("File Too Large", "Image file size cannot exceed 5MB. Please choose a smaller image.", MessageDialog.MessageType.Warning);
                    warningDialog.ShowDialog();
                    return;
                }

                // Copy file to application images directory
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChronoPos", "Images");
                Directory.CreateDirectory(appDataPath);

                var fileName = $"product_{Code}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(selectedFile)}";
                var destinationPath = Path.Combine(appDataPath, fileName);

                File.Copy(selectedFile, destinationPath, true);
                FileLogger.Log($"📋 File copied to: {destinationPath}");
                
                // Add to ProductImages collection instead of setting ImagePath
                var nextSortOrder = ProductImages.Any() ? ProductImages.Max(i => i.SortOrder) + 1 : 1;
                var imageItem = new ProductImageItemViewModel
                {
                    ImageUrl = destinationPath,
                    AltText = $"Product Image {ProductImages.Count + 1}",
                    SortOrder = nextSortOrder,
                    IsPrimary = !ProductImages.Any(), // First image is primary by default
                    IsNew = true
                };

                // Handle primary image logic
                imageItem.OnPrimaryChanged += HandlePrimaryImageChanged;
                ProductImages.Add(imageItem);
                
                FileLogger.Log($"✅ Added image via file selection - Total count now: {ProductImages.Count}");
                FileLogger.Log($"   🏷️ Image details: URL='{destinationPath}', AltText='{imageItem.AltText}', SortOrder={nextSortOrder}, IsPrimary={imageItem.IsPrimary}");
                
                // Also set ImagePath for backward compatibility
                ImagePath = destinationPath;
                
                StatusMessage = "Product image added successfully";
            }
            else
            {
                FileLogger.Log("❌ File selection cancelled");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error in ChooseImage: {ex.Message}");
            var errorDialog = new MessageDialog("Error", $"Error selecting image: {ex.Message}", MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        ImagePath = string.Empty;
        StatusMessage = "Product image removed";
    }

    [RelayCommand]
    private void AddModifierGroup()
    {
        try
        {
            if (SelectedModifierGroup == null)
            {
                StatusMessage = "Please select a modifier group to add";
                return;
            }

            // Check if already added
            if (SelectedModifierGroups.Any(g => g.Id == SelectedModifierGroup.Id))
            {
                StatusMessage = $"Modifier group '{SelectedModifierGroup.Name}' is already added";
                return;
            }

            SelectedModifierGroups.Add(SelectedModifierGroup);
            StatusMessage = $"Modifier group '{SelectedModifierGroup.Name}' added successfully";
            FileLogger.Log($"✅ Added modifier group: {SelectedModifierGroup.Name}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error adding modifier group: {ex.Message}";
            FileLogger.Log($"❌ Error adding modifier group: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RemoveModifierGroup(ProductModifierGroupDto modifierGroup)
    {
        try
        {
            if (modifierGroup == null)
                return;

            SelectedModifierGroups.Remove(modifierGroup);
            StatusMessage = $"Modifier group '{modifierGroup.Name}' removed";
            FileLogger.Log($"✅ Removed modifier group: {modifierGroup.Name}");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error removing modifier group: {ex.Message}";
            FileLogger.Log($"❌ Error removing modifier group: {ex.Message}");
        }
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

            StatusMessage = IsEditMode ? "Updating product..." : "Saving product...";

            var productDto = CreateProductDto();
            
            ProductDto savedProduct;
            if (IsEditMode)
            {
                // Update existing product
                productDto.Id = ProductId;
                savedProduct = await _productService.UpdateProductAsync(productDto);
                StatusMessage = "Product updated successfully!";
                var successDialog = new MessageDialog("Success", "Product updated successfully!", MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            else
            {
                // Create new product
                savedProduct = await _productService.CreateProductAsync(productDto);
                StatusMessage = "Product saved successfully!";
                var successDialog = new MessageDialog("Success", "Product created successfully!", MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }

            // Save product images if any
            await SaveProductImages(savedProduct.Id);

            // Save product barcodes if any
            await SaveProductBarcodes(savedProduct.Id);

            // Save product modifier links if any
            await SaveProductModifierLinks(savedProduct.Id);

            // Navigate back to product management
            _navigateBack?.Invoke();
        }
        catch (Exception ex)
        {
            var operation = IsEditMode ? "updating" : "saving";
            StatusMessage = $"Error {operation} product: {ex.Message}";
            var errorDialog = new MessageDialog("Error", $"Failed to {operation} product: {ex.Message}", MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        var confirmDialog = new ConfirmationDialog(
            "Confirm Cancel",
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            ConfirmationDialog.DialogType.Warning);

        if (confirmDialog.ShowDialog() == true)
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
        
        // Clear tax and discount selections
        SelectedTaxTypes.Clear();
        SelectedTaxTypeIds.Clear();
        SelectedDiscounts.Clear();
        SelectedDiscountIds.Clear();
        
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
            
            var successDialog = new MessageDialog("Success", "Category saved successfully!", MessageDialog.MessageType.Success);
            successDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving category: {ex.Message}";
            var errorDialog = new MessageDialog("Error", $"Failed to save category: {ex.Message}", MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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

        // Validate ProductUnits
        ValidateProductUnits();
        if (HasProductUnitValidationErrors)
        {
            ValidationErrors.AddRange(ProductUnitValidationErrors.Values);
        }

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
        FileLogger.Log($"📦 CreateProductDto: Creating DTO for product '{Name}' with {Barcodes?.Count ?? 0} barcodes");
        
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
            Barcode = Barcodes?.FirstOrDefault()?.Value, // Temporary for compatibility - will be handled by separate barcodes table
            Price = Price,
            CategoryId = CategoryId ?? 1, // Default category if none selected
            StockQuantity = (int)InitialStock, // Set stock quantity from user input
            IsActive = IsEnabled,
            CostPrice = Cost,
            Markup = calculatedMarkup,
            ImagePath = ImagePath,
            Color = Color,
            BrandId = SelectedBrand?.Id, // Map selected brand
            BrandName = SelectedBrand?.Name ?? string.Empty, // For display purposes
            // Tax & Attributes persisted on Product
            IsTaxInclusivePrice = IsTaxInclusivePrice,
            TaxInclusivePriceValue = TaxInclusivePriceValue,
            IsDiscountAllowed = IsDiscountAllowed,
            MaxDiscount = MaxDiscount,
            IsPriceChangeAllowed = IsPriceChangeAllowed,
            IsService = IsService,
            AgeRestriction = AgeRestriction,
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
            // UOM Properties - Keep for backwards compatibility but will be handled by ProductUnits
            UnitOfMeasurementId = SelectedUnitOfMeasurementId > 0 ? SelectedUnitOfMeasurementId : 1, // Default to "Pieces"
            UnitOfMeasurementName = SelectedUnitOfMeasurement?.Name ?? "Pieces",
            UnitOfMeasurementAbbreviation = SelectedUnitOfMeasurement?.Abbreviation ?? "pcs",
            
            // Multi-UOM Support
            ProductUnits = ProductUnits.Select(pu => new ProductUnitDto
            {
                Id = pu.Id,
                ProductId = 0, // Will be set by the service
                UnitId = pu.UnitId,
                UnitName = UnitsOfMeasurement.FirstOrDefault(u => u.Id == pu.UnitId)?.Name ?? "Unknown",
                UnitAbbreviation = UnitsOfMeasurement.FirstOrDefault(u => u.Id == pu.UnitId)?.Abbreviation ?? "unk",
                QtyInUnit = pu.QtyInUnit,
                CostOfUnit = pu.CostOfUnit,
                PriceOfUnit = pu.PriceOfUnit,
                DiscountAllowed = pu.DiscountAllowed,
                IsBase = pu.IsBase,
                Sku = pu.Sku
            }).ToList(),
            
            // Purchase and Selling Units
            PurchaseUnitId = PurchaseUnitId,
            SellingUnitId = SellingUnitId,
            
            // Product Grouping
            ProductGroupId = ProductGroupId,
            Group = Group,
            
            // Business Rules (Additional)
            CanReturn = CanReturn,
            IsGrouped = IsGrouped,
            
            SelectedStoreId = SelectedStoreId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            
            // Map UI Barcodes collection to ProductBarcodes collection
            ProductBarcodes = Barcodes?.Select(b => new ProductBarcodeDto
            {
                Barcode = b.Value,
                BarcodeType = b.BarcodeType,
                IsNew = true,
                CreatedAt = DateTime.UtcNow
            }).ToList() ?? new List<ProductBarcodeDto>(),

            // Selected tax types
            SelectedTaxTypeIds = SelectedTaxTypeIdsList,
            
            // Selected discounts
            SelectedDiscountIds = SelectedDiscountIdsList
        };

        FileLogger.Log($"📦 CreateProductDto: Created DTO with {productDto.ProductBarcodes.Count} barcodes:");
        foreach (var barcode in productDto.ProductBarcodes)
        {
            FileLogger.Log($"   🏷️ Barcode: {barcode.Barcode} (Type: {barcode.BarcodeType})");
        }

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
        FileLogger.Log($"🏷️ OnPriceChanged called - New Price: {value}, Old Cost: {Cost}");
        
        if (Cost > 0 && value > Cost)
        {
            Markup = ((value - Cost) / Cost) * 100;
            FileLogger.Log($"   📈 Markup calculated: {Markup:F2}%");
        }
        CalculateTaxInclusivePrice();
        
        FileLogger.Log($"   🔄 Calling UpdateProductUnitPricing for all units...");
        UpdateProductUnitPricing(); // Auto-calculate price of units
        
        // Refresh currency formatted properties
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(FormattedTaxInclusivePrice));
        
        ValidateForm();
        FileLogger.Log($"✅ OnPriceChanged completed");
    }

    partial void OnCostChanged(decimal value)
    {
        FileLogger.Log($"💵 OnCostChanged called - New Cost: {value}, Current Price: {Price}");
        
        if (Price > 0 && value > 0 && Price > value)
        {
            Markup = ((Price - value) / value) * 100;
            FileLogger.Log($"   📈 Markup calculated: {Markup:F2}%");
        }
        
        FileLogger.Log($"   🔄 Calling UpdateProductUnitPricing for all units...");
        UpdateProductUnitPricing(); // Auto-calculate cost of units
        
        // Refresh currency formatted properties
        OnPropertyChanged(nameof(FormattedCost));
        
        ValidateForm();
        FileLogger.Log($"✅ OnCostChanged completed");
    }

    partial void OnCodeChanged(string value)
    {
        ValidateForm();
    }

    partial void OnNameChanged(string value)
    {
        ValidateForm();
    }

    /// <summary>
    /// Handles currency change events from IActiveCurrencyService
    /// </summary>
    private void OnCurrencyChanged(object? sender, CurrencyDto newCurrency)
    {
        FileLogger.Log($"💱 Currency changed to: {newCurrency.Symbol} {newCurrency.CurrencyCode} (Rate: {newCurrency.ExchangeRate})");
        
        // Refresh all currency-formatted properties (these will auto-convert using new exchange rate)
        OnPropertyChanged(nameof(FormattedPrice));
        OnPropertyChanged(nameof(FormattedCost));
        OnPropertyChanged(nameof(FormattedLastPurchasePrice));
        OnPropertyChanged(nameof(FormattedTaxInclusivePrice));
        OnPropertyChanged(nameof(CurrencySymbol));
        OnPropertyChanged(nameof(CurrencyCode));
        OnPropertyChanged(nameof(CurrencyName));
        OnPropertyChanged(nameof(ExchangeRateInfo));
        
        // Refresh ProductUnits grid to update formatted prices
        OnPropertyChanged(nameof(ProductUnits));
        
        FileLogger.Log($"✅ Currency UI updated successfully - Prices converted from AED to {newCurrency.CurrencyCode}");
    }

    /// <summary>
    /// Calculates the tax-inclusive price based on selected tax types
    /// </summary>
    private void CalculateTaxInclusivePrice()
    {
        // Prevent recursive calculations
        if (_isCalculatingTax) return;
        
        try
        {
            _isCalculatingTax = true;
            
            decimal basePrice = Price;
            if (basePrice <= 0)
            {
                TaxInclusivePriceValue = 0;
                return;
            }

            if (SelectedTaxTypes == null || !SelectedTaxTypes.Any())
            {
                TaxInclusivePriceValue = basePrice;
                return;
            }

            var applicableTaxes = SelectedTaxTypes
                .Where(t => t.IsActive && t.AppliesToSelling)
                .OrderBy(t => t.CalculationOrder)
                .ToList();

            if (IsTaxInclusivePrice)
            {
                // Tax Inclusive Mode: Price Including Tax stays same, Selling Price gets reduced
                decimal totalTaxPercentage = 0;
                decimal totalTaxAmount = 0;

                foreach (var tax in applicableTaxes)
                {
                    if (tax.IsPercentage)
                    {
                        totalTaxPercentage += tax.Value;
                    }
                    else
                    {
                        totalTaxAmount += tax.Value;
                    }
                }

                // Keep the Price Including Tax as the original entered selling price
                TaxInclusivePriceValue = Math.Round(basePrice, 2);
                
                // Calculate new base selling price: BasePrice = (TotalPrice - FixedTax) / (1 + TaxPercentage/100)
                decimal newSellingPrice = (basePrice - totalTaxAmount) / (1 + totalTaxPercentage / 100m);
                
                // Update the selling price (this will trigger property change notification)
                Price = Math.Round(newSellingPrice, 2);
                
                FileLogger.Log($"Tax Inclusive Mode: Original Selling Price: {basePrice:C}, New Selling Price: {Price:C}, Price Including Tax: {TaxInclusivePriceValue:C}, Total Tax%: {totalTaxPercentage}%, Fixed Tax: {totalTaxAmount:C}");
            }
            else
            {
                // Tax Exclusive Mode: Selling Price stays same, Price Including Tax gets increased
                decimal runningTotal = basePrice;

                foreach (var tax in applicableTaxes)
                {
                    if (tax.IsPercentage)
                    {
                        runningTotal += Math.Round(basePrice * (tax.Value / 100m), 2);
                    }
                    else
                    {
                        runningTotal += tax.Value;
                    }
                }

                TaxInclusivePriceValue = Math.Round(runningTotal, 2);
                
                FileLogger.Log($"Tax Exclusive Mode: Selling Price: {basePrice:C}, Price Including Tax: {TaxInclusivePriceValue:C}");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error calculating tax-inclusive price: {ex.Message}");
            TaxInclusivePriceValue = Price; // Fallback to base price
        }
        finally
        {
            _isCalculatingTax = false;
        }
    }

    /// <summary>
    /// Saves all product images to the database
    /// </summary>
    /// <param name="productId">The ID of the saved product</param>
    private async Task SaveProductImages(int productId)
    {
        try
        {
            FileLogger.Log($"🔄 Starting SaveProductImages for product ID: {productId}");
            FileLogger.Log($"📊 ProductImages collection contains {ProductImages.Count} images:");
            for (int i = 0; i < ProductImages.Count; i++)
            {
                var img = ProductImages[i];
                FileLogger.Log($"   Image {i + 1}: URL='{img.ImageUrl}', Primary={img.IsPrimary}, SortOrder={img.SortOrder}, IsNew={img.IsNew}");
            }
            
            // In edit mode, we need to delete existing images first and recreate them
            // This is a simple approach - for better performance, we could implement
            // a more sophisticated sync logic
            if (IsEditMode)
            {
                FileLogger.Log($"🗑️ Edit mode: Deleting existing images for product ID: {productId}");
                await _productImageService.DeleteByProductIdAsync(productId);
                FileLogger.Log($"✅ Deleted existing images for product ID: {productId}");
            }
            
            int savedCount = 0;
            foreach (var imageItem in ProductImages)
            {
                if (!string.IsNullOrEmpty(imageItem.ImageUrl))
                {
                    FileLogger.Log($"💾 Processing image {savedCount + 1}: {imageItem.ImageUrl}");
                    
                    var createImageDto = new CreateProductImageDto
                    {
                        ProductId = productId,
                        ImageUrl = imageItem.ImageUrl,
                        AltText = imageItem.AltText ?? string.Empty,
                        SortOrder = imageItem.SortOrder,
                        IsPrimary = imageItem.IsPrimary
                    };

                    var savedImage = await _productImageService.CreateAsync(createImageDto);
                    savedCount++;
                    FileLogger.Log($"✅ Saved product image {savedCount}: {imageItem.ImageUrl} (DB ID: {savedImage.Id})");
                }
                else
                {
                    FileLogger.Log($"⚠️ Skipping image with empty URL: Primary={imageItem.IsPrimary}, SortOrder={imageItem.SortOrder}");
                }
            }
            
            FileLogger.Log($"✅ SaveProductImages completed: {savedCount} out of {ProductImages.Count} images saved successfully");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error in SaveProductImages: {ex.Message}");
            FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
            throw; // Re-throw to be handled by the calling method
        }
    }

    /// <summary>
    /// Saves all product barcodes to the database
    /// </summary>
    /// <param name="productId">The ID of the saved product</param>
    private Task SaveProductBarcodes(int productId)
    {
        try
        {
            FileLogger.Log($"🔄 Saving {Barcodes.Count} product barcodes for product ID: {productId}");
            
            // For now, we'll handle barcodes through the ProductService.UpdateProductAsync
            // which clears and recreates the ProductBarcodes collection
            // This is a simple approach - for better performance, we could implement
            // a more sophisticated sync logic similar to images
            
            if (IsEditMode)
            {
                // For edit mode, the barcodes will be handled when we update the main product
                // The UpdateProductAsync method in ProductService already handles barcode updates
                FileLogger.Log($"ℹ️ Barcodes will be updated via ProductService.UpdateProductAsync in edit mode");
            }
            else
            {
                // For create mode, barcodes are already handled in MapToEntityAsync
                FileLogger.Log($"ℹ️ Barcodes already handled via ProductService.CreateProductAsync in create mode");
            }
            
            FileLogger.Log($"✅ Product barcodes handling completed for product ID: {productId}");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error handling product barcodes: {ex.Message}");
            throw; // Re-throw to be handled by the calling method
        }
    }

    private async Task SaveProductModifierLinks(int productId)
    {
        try
        {
            FileLogger.Log($"🔄 Saving {SelectedModifierGroups.Count} product modifier links for product ID: {productId}");
            
            // Delete existing links for this product
            await _modifierLinkService.DeleteByProductIdAsync(productId);
            FileLogger.Log($"🗑️ Deleted existing modifier links for product ID: {productId}");
            
            // Create new links
            foreach (var modifierGroup in SelectedModifierGroups)
            {
                var linkDto = new CreateProductModifierLinkDto
                {
                    ProductId = productId,
                    ModifierGroupId = modifierGroup.Id
                };
                
                await _modifierLinkService.CreateAsync(linkDto);
                FileLogger.Log($"✅ Created modifier link: Product {productId} -> Group {modifierGroup.Id} ({modifierGroup.Name})");
            }
            
            FileLogger.Log($"✅ Product modifier links saved successfully for product ID: {productId}");
        }
        catch (Exception ex)
        {
            FileLogger.Log($"❌ Error saving product modifier links: {ex.Message}");
            throw; // Re-throw to be handled by the calling method
        }
    }

    #region Product Batch Commands

    [RelayCommand]
    private async Task ShowProductBatchesAsync()
    {
        AppLogger.LogSeparator("SHOW PRODUCT BATCHES CLICKED", "product_batches_ui");
        
        try
        {
            AppLogger.LogInfo($"ShowProductBatches command started", 
                $"Product ID: {ProductId}, Current ProductBatches count: {ProductBatches?.Count ?? 0}", "product_batches_ui");

            // Check if ProductBatchService is available
            if (_productBatchService == null)
            {
                AppLogger.LogError($"ProductBatchService is null - cannot load batches", 
                    null, $"Product ID: {ProductId}", "product_batches_ui");
                StatusMessage = "Product Batch service not available";
                return;
            }

            AppLogger.LogInfo($"ProductBatchService is available, starting to load batches", 
                $"Product ID: {ProductId}", "product_batches_ui");

            // Load current batches for the product
            await LoadProductBatchesAsync();
            
            AppLogger.LogInfo($"Product batches loaded successfully", 
                $"Product ID: {ProductId}, Loaded batches count: {ProductBatches?.Count ?? 0}", "product_batches_ui");
            
            StatusMessage = "Product Batches loaded successfully";
            
            // TODO: Implement AddProductBatchDialog or inline batch creation
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Critical error showing product batches", ex, 
                $"Product ID: {ProductId}, Exception Type: {ex.GetType().Name}", "product_batches_ui");
            
            StatusMessage = "Error loading product batches";
        }
        finally
        {
            AppLogger.LogSeparator("SHOW PRODUCT BATCHES COMPLETE", "product_batches_ui");
        }
    }

    [RelayCommand]
    private void EditProductBatch(ProductBatchDto? batch)
    {
        AppLogger.LogInfo($"EditProductBatch command started", 
            $"Batch: {(batch != null ? $"ID={batch.Id}, BatchNo={batch.BatchNo}" : "NULL")}", "product_batches_ui");
        
        try
        {
            if (batch == null)
            {
                AppLogger.LogWarning($"EditProductBatch called with null batch", 
                    $"Product ID: {ProductId}", "product_batches_ui");
                return;
            }
            
            AppLogger.LogInfo($"Starting to edit product batch", 
                $"Batch ID: {batch.Id}, Batch No: {batch.BatchNo}, Product ID: {ProductId}", "product_batches_ui");
            
            StatusMessage = $"Editing batch: {batch.BatchNo}";
            
            // TODO: Implement EditProductBatchDialog
            AppLogger.LogWarning($"EditProductBatchDialog not implemented yet", 
                $"Batch ID: {batch.Id}", "product_batches_ui");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error editing product batch", ex, 
                $"Batch ID: {batch?.Id}, Product ID: {ProductId}", "product_batches_ui");
            StatusMessage = "Error editing batch";
        }
    }

    [RelayCommand]
    private async Task DeleteProductBatchAsync(ProductBatchDto? batch)
    {
        AppLogger.LogInfo($"DeleteProductBatch command started", 
            $"Batch: {(batch != null ? $"ID={batch.Id}, BatchNo={batch.BatchNo}" : "NULL")}", "product_batches_ui");
        
        try
        {
            if (batch == null)
            {
                AppLogger.LogWarning($"DeleteProductBatch called with null batch", 
                    $"Product ID: {ProductId}", "product_batches_ui");
                return;
            }

            AppLogger.LogInfo($"Showing delete confirmation dialog", 
                $"Batch ID: {batch.Id}, Batch No: {batch.BatchNo}", "product_batches_ui");

            var confirmDialog = new ConfirmationDialog(
                "Confirm Delete",
                $"Are you sure you want to delete batch '{batch.BatchNo}'?",
                ConfirmationDialog.DialogType.Danger);

            var dialogResult = confirmDialog.ShowDialog();
            AppLogger.LogInfo($"User response to delete confirmation", 
                $"Batch ID: {batch.Id}, User choice: {(dialogResult == true ? "Yes" : "No")}", "product_batches_ui");

            if (dialogResult == true)
            {
                AppLogger.LogInfo($"Proceeding with batch deletion", 
                    $"Batch ID: {batch.Id}, Batch No: {batch.BatchNo}", "product_batches_ui");

                await _productBatchService.DeleteProductBatchAsync(batch.Id);
                
                AppLogger.LogInfo($"Batch deleted successfully, reloading batches", 
                    $"Deleted Batch ID: {batch.Id}, Product ID: {ProductId}", "product_batches_ui");

                await LoadProductBatchesAsync();
                StatusMessage = $"Batch '{batch.BatchNo}' deleted successfully";
            }
            else
            {
                AppLogger.LogInfo($"User cancelled batch deletion", 
                    $"Batch ID: {batch.Id}", "product_batches_ui");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Critical error deleting product batch", ex, 
                $"Batch ID: {batch?.Id}, Product ID: {ProductId}", "product_batches_ui");
            StatusMessage = "Error deleting batch";
        }
    }

    private async Task LoadProductBatchesAsync()
    {
        AppLogger.LogInfo($"Starting to load product batches", 
            $"Product ID: {ProductId}", "product_batches_loading");
        
        try
        {
            // Check if Product ID is valid
            if (ProductId <= 0) 
            {
                AppLogger.LogWarning($"Invalid Product ID - clearing batches", 
                    $"Product ID: {ProductId}", "product_batches_loading");
                
                ProductBatches.Clear();
                HasProductBatches = false;
                return;
            }

            // Check if ProductBatches collection is initialized
            if (ProductBatches == null)
            {
                AppLogger.LogWarning($"ProductBatches collection is null - initializing", 
                    $"Product ID: {ProductId}", "product_batches_loading");
                ProductBatches = new ObservableCollection<ProductBatchDto>();
            }

            AppLogger.LogDebug($"Calling ProductBatchService.GetProductBatchesByProductIdAsync", 
                $"Product ID: {ProductId}, Service: {(_productBatchService?.GetType().Name ?? "NULL")}", "product_batches_loading");

            // Call the service to get batches
            var batches = await _productBatchService.GetProductBatchesByProductIdAsync(ProductId);
            
            AppLogger.LogInfo($"ProductBatchService returned batches", 
                $"Product ID: {ProductId}, Returned batches count: {batches?.Count ?? 0}", "product_batches_loading");
            
            // Clear and populate the collection on UI thread
            AppLogger.LogDebug($"Clearing ProductBatches collection on UI thread", 
                $"Product ID: {ProductId}, Current count: {ProductBatches.Count}", "product_batches_loading");

            // Ensure UI thread operations
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    ProductBatches.Clear();
                    AppLogger.LogDebug($"ProductBatches collection cleared successfully", 
                        $"Product ID: {ProductId}", "product_batches_loading");
                }
                catch (Exception ex)
                {
                    AppLogger.LogError($"Error clearing ProductBatches collection", ex, 
                        $"Product ID: {ProductId}", "product_batches_loading");
                    throw;
                }
            });
            
            if (batches != null && batches.Any())
            {
                AppLogger.LogDebug($"Adding batches to ProductBatches collection on UI thread", 
                    $"Product ID: {ProductId}, Batches to add: {batches.Count}", "product_batches_loading");
                
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        foreach (var batch in batches)
                        {
                            // Validate batch before adding
                            if (batch == null)
                            {
                                AppLogger.LogWarning($"Skipping null batch", 
                                    $"Product ID: {ProductId}", "product_batches_loading");
                                continue;
                            }

                            if (string.IsNullOrEmpty(batch.BatchNo))
                            {
                                AppLogger.LogWarning($"Batch has empty BatchNo, using default", 
                                    $"Batch ID: {batch.Id}, Product ID: {ProductId}", "product_batches_loading");
                                batch.BatchNo = $"BATCH-{batch.Id}";
                            }

                            AppLogger.LogDebug($"Adding validated batch to collection", 
                                $"Batch ID: {batch.Id}, Batch No: {batch.BatchNo}, Quantity: {batch.Quantity}", "product_batches_loading");
                            
                            ProductBatches.Add(batch);
                        }
                        
                        AppLogger.LogInfo($"All batches added to collection successfully", 
                            $"Product ID: {ProductId}, Final collection count: {ProductBatches.Count}", "product_batches_loading");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.LogError($"Error adding batches to collection", ex, 
                            $"Product ID: {ProductId}, Attempted count: {batches.Count}", "product_batches_loading");
                        throw;
                    }
                });
            }
            
            // Update HasProductBatches on UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                HasProductBatches = ProductBatches.Count > 0;
                AppLogger.LogInfo($"HasProductBatches updated", 
                    $"Product ID: {ProductId}, HasProductBatches: {HasProductBatches}, Count: {ProductBatches.Count}", "product_batches_loading");
            });
            
            AppLogger.LogInfo($"Product batches loaded successfully", 
                $"Product ID: {ProductId}, Final count: {ProductBatches.Count}, HasProductBatches: {HasProductBatches}", "product_batches_loading");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Critical error loading product batches", ex, 
                $"Product ID: {ProductId}, Exception Type: {ex.GetType().Name}, Inner Exception: {ex.InnerException?.Message}", "product_batches_loading");
            
            // Ensure safe state
            try
            {
                ProductBatches?.Clear();
                HasProductBatches = false;
            }
            catch (Exception cleanupEx)
            {
                AppLogger.LogError($"Error during cleanup after batch loading failure", cleanupEx, 
                    $"Product ID: {ProductId}", "product_batches_loading");
            }
        }
    }

    #endregion

    #endregion
}
