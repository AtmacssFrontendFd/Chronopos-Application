using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Domain.Enums;
using System.Collections.ObjectModel;
using DataAnnotationsValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the discount sidepanel component with tabbed interface
/// </summary>
public partial class DiscountSidePanelViewModel : ObservableValidator, IDisposable
{
    #region Fields
    
    private readonly IDiscountService _discountService;
    private readonly IProductService _productService;
    private readonly object? _categoryService; // ICategoryService when available
    private readonly ICustomerService? _customerService;
    private readonly IStoreService? _storeService;
    private readonly Action<bool>? _onSaved;
    private readonly Action? _onCancelled;
    
    // Settings services
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    
    private bool _isEditMode = false;
    private int _editingDiscountId = 0;
    private DiscountDto? _editingDiscount = null; // Store discount data for restoring selections after load
    
    #endregion

    #region Observable Properties

    // Form State
    [ObservableProperty]
    private string _formTitle = "Add New Discount";

    [ObservableProperty]
    private string _saveButtonText = "Save Discount";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    // General Info Tab
    [ObservableProperty]
    [Required(ErrorMessage = "Discount name is required")]
    [StringLength(150, ErrorMessage = "Discount name must be less than 150 characters")]
    private string _discountName = string.Empty;

    [ObservableProperty]
    [StringLength(150, ErrorMessage = "Arabic name must be less than 150 characters")]
    private string _discountNameAr = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Discount code is required")]
    [StringLength(50, ErrorMessage = "Discount code must be less than 50 characters")]
    private string _discountCode = string.Empty;

    [ObservableProperty]
    [StringLength(150, ErrorMessage = "Description must be less than 150 characters")]
    private string _discountDescription = string.Empty;

    [ObservableProperty]
    private DiscountType _discountType = DiscountType.Percentage;

    [ObservableProperty]
    [Required(ErrorMessage = "Discount value is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
    private decimal _discountValue = 0;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Max discount amount must be 0 or greater")]
    private decimal? _maxDiscountAmount;

    [ObservableProperty]
    private string _currencyCode = "USD";

    // Applicability Tab
    [ObservableProperty]
    private DiscountApplicableOn _applicableOn = DiscountApplicableOn.Shop;

    [ObservableProperty]
    private string _productSearchText = string.Empty;

    [ObservableProperty]
    private string _categorySearchText = string.Empty;

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    [ObservableProperty]
    private object? _selectedProduct;

    [ObservableProperty]
    private object? _selectedCategory;

    [ObservableProperty]
    private object? _selectedCustomer;

    [ObservableProperty]
    private object? _selectedStore;

    // Multi-select properties for products
    [ObservableProperty]
    private bool _selectAllProducts = false;

    [ObservableProperty]
    private ObservableCollection<ProductSelectionViewModel> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductSelectionViewModel> _filteredProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductSelectionViewModel> _selectedProducts = new();

    // Multi-select properties for categories
    [ObservableProperty]
    private bool _selectAllCategories = false;

    [ObservableProperty]
    private ObservableCollection<CategorySelectionViewModel> _availableCategories = new();

    [ObservableProperty]
    private ObservableCollection<CategorySelectionViewModel> _filteredCategories = new();

    [ObservableProperty]
    private ObservableCollection<CategorySelectionViewModel> _selectedCategories = new();

    // Multi-select properties for shops
    [ObservableProperty]
    private string _shopSearchText = string.Empty;

    [ObservableProperty]
    private bool _selectAllShops = false;

    [ObservableProperty]
    private ObservableCollection<ShopSelectionViewModel> _availableShops = new();

    [ObservableProperty]
    private ObservableCollection<ShopSelectionViewModel> _filteredShops = new();

    [ObservableProperty]
    private ObservableCollection<ShopSelectionViewModel> _selectedShops = new();

    // Multi-select properties for customers
    [ObservableProperty]
    private bool _selectAllCustomers = false;

    [ObservableProperty]
    private ObservableCollection<CustomerSelectionViewModel> _availableCustomers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerSelectionViewModel> _filteredCustomers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerSelectionViewModel> _selectedCustomers = new();

    // Validity Tab
    [ObservableProperty]
    [Required(ErrorMessage = "Start date is required")]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    [Required(ErrorMessage = "End date is required")]
    private DateTime _endDate = DateTime.Today.AddMonths(1);

    [ObservableProperty]
    private bool _hasUsageLimit = false;

    [ObservableProperty]
    private int? _usageLimit;

    [ObservableProperty]
    private bool _hasPerCustomerLimit = false;

    [ObservableProperty]
    private int? _perCustomerLimit;

    // Rules Tab
    [ObservableProperty]
    private bool _hasMinPurchaseAmount = false;

    [ObservableProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Min purchase amount must be 0 or greater")]
    private decimal? _minPurchaseAmount;

    [ObservableProperty]
    [Range(0, int.MaxValue, ErrorMessage = "Priority must be 0 or greater")]
    private int _priority = 0;

    [ObservableProperty]
    private bool _isStackable = false;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private bool _excludeSaleItems = false;

    [ObservableProperty]
    private bool _isOneTimeUse = false;

    // Collections
    [ObservableProperty]
    private ObservableCollection<DiscountType> _discountTypes = new();

    [ObservableProperty]
    private ObservableCollection<DiscountApplicableOn> _applicableOnOptions = new();

    [ObservableProperty]
    private ObservableCollection<string> _currencies = new();

    [ObservableProperty]
    private ObservableCollection<object> _products = new();

    [ObservableProperty]
    private ObservableCollection<object> _categories = new();

    [ObservableProperty]
    private ObservableCollection<object> _stores = new();

    // Computed Properties
    public bool IsPercentageDiscount => DiscountType == DiscountType.Percentage;
    public bool IsProductApplicable => ApplicableOn == DiscountApplicableOn.Product;
    public bool IsCategoryApplicable => ApplicableOn == DiscountApplicableOn.Category;
    public bool IsCustomerApplicable => ApplicableOn == DiscountApplicableOn.Customer;
    public bool IsShopApplicable => ApplicableOn == DiscountApplicableOn.Shop;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor for creating a new discount
    /// </summary>
    public DiscountSidePanelViewModel(
        IDiscountService discountService,
        IProductService productService,
        object? categoryService,
        ICustomerService? customerService,
        IStoreService? storeService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ILayoutDirectionService layoutDirectionService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action<bool>? onSaved = null,
        Action? onCancelled = null)
    {
        _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _categoryService = categoryService;
        _customerService = customerService;
        _storeService = storeService;
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        
        _onSaved = onSaved;
        _onCancelled = onCancelled;

        // Initialize settings
        InitializeSettings();
        
        // Initialize collections and data
        InitializeCollections();
        
        // Load data
        _ = Task.Run(LoadDataAsync);
        
        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Constructor for editing an existing discount
    /// </summary>
    public DiscountSidePanelViewModel(
        IDiscountService discountService,
        IProductService productService,
        object? categoryService,
        ICustomerService? customerService,
        IStoreService? storeService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ILayoutDirectionService layoutDirectionService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        DiscountDto discount,
        Action<bool>? onSaved = null,
        Action? onCancelled = null) : this(discountService, productService, categoryService, customerService, storeService, themeService, localizationService, layoutDirectionService, databaseLocalizationService, onSaved, onCancelled)
    {
        FileLogger.LogSeparator();
        FileLogger.Log("=== DISCOUNT EDIT MODE CONSTRUCTOR STARTED ===");
        FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: Discount ID = {discount.Id}");
        FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: Discount Name = '{discount.DiscountName}'");
        FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: ApplicableOn = {discount.ApplicableOn}");
        FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: SelectedProductIds count = {discount.SelectedProductIds?.Count ?? 0}");
        FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: SelectedCategoryIds count = {discount.SelectedCategoryIds?.Count ?? 0}");
        
        if (discount.SelectedProductIds?.Any() == true)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: SelectedProductIds = [{string.Join(", ", discount.SelectedProductIds)}]");
        }
        
        if (discount.SelectedCategoryIds?.Any() == true)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.EditConstructor: SelectedCategoryIds = [{string.Join(", ", discount.SelectedCategoryIds)}]");
        }
        
        _isEditMode = true;
        _editingDiscountId = discount.Id;
        FormTitle = "Edit Discount";
        SaveButtonText = "Update Discount";
        
        FileLogger.Log("DiscountSidePanelViewModel.EditConstructor: Calling LoadDiscountData...");
        LoadDiscountData(discount);
        FileLogger.Log("=== DISCOUNT EDIT MODE CONSTRUCTOR COMPLETED ===");
        FileLogger.LogSeparator();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            if (!ValidateForm())
                return;

            IsLoading = true;
            ValidationMessage = string.Empty;

            if (_isEditMode)
            {
                var updateDto = CreateUpdateDiscountDto();
                await _discountService.UpdateAsync(_editingDiscountId, updateDto);
                ValidationMessage = "Discount updated successfully";
            }
            else
            {
                var createDto = CreateDiscountDto();
                await _discountService.CreateAsync(createDto);
                ValidationMessage = "Discount created successfully";
            }

            _onSaved?.Invoke(true);
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error saving discount: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancelled?.Invoke();
    }

    [RelayCommand]
    private void Close()
    {
        Cancel();
    }

    [RelayCommand]
    private void SetToday()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
    }

    [RelayCommand]
    private void SetThisWeek()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        StartDate = startOfWeek;
        EndDate = startOfWeek.AddDays(6);
    }

    [RelayCommand]
    private void SetThisMonth()
    {
        var today = DateTime.Today;
        StartDate = new DateTime(today.Year, today.Month, 1);
        EndDate = StartDate.AddMonths(1).AddDays(-1);
    }

    [RelayCommand]
    private void SetNextMonth()
    {
        var today = DateTime.Today;
        var nextMonth = today.AddMonths(1);
        StartDate = new DateTime(nextMonth.Year, nextMonth.Month, 1);
        EndDate = StartDate.AddMonths(1).AddDays(-1);
    }

    [RelayCommand]
    private void IncreasePriority()
    {
        Priority++;
    }

    [RelayCommand]
    private void DecreasePriority()
    {
        if (Priority > 0)
            Priority--;
    }

    /// <summary>
    /// Toggle select all products
    /// </summary>
    [RelayCommand]
    private void ToggleSelectAllProducts()
    {
        foreach (var product in AvailableProducts)
        {
            product.IsSelected = SelectAllProducts;
        }
        UpdateSelectedProducts();
    }

    /// <summary>
    /// Toggle select all categories  
    /// </summary>
    [RelayCommand]
    private void ToggleSelectAllCategories()
    {
        foreach (var category in AvailableCategories)
        {
            category.IsSelected = SelectAllCategories;
        }
        UpdateSelectedCategories();
    }

    /// <summary>
    /// Handle product selection change
    /// </summary>
    [RelayCommand]
    private void ProductSelectionChanged(ProductSelectionViewModel product)
    {
        UpdateSelectedProducts();
        UpdateSelectAllProductsState();
    }

    /// <summary>
    /// Handle category selection change
    /// </summary>
    [RelayCommand]
    private void CategorySelectionChanged(CategorySelectionViewModel category)
    {
        UpdateSelectedCategories();
        UpdateSelectAllCategoriesState();
    }

    /// <summary>
    /// Filter categories based on search text
    /// </summary>
    [RelayCommand]
    private void FilterCategories()
    {
        if (string.IsNullOrWhiteSpace(CategorySearchText))
        {
            FilteredCategories.Clear();
            foreach (var category in AvailableCategories)
            {
                FilteredCategories.Add(category);
            }
        }
        else
        {
            FilteredCategories.Clear();
            var searchTerm = CategorySearchText.ToLowerInvariant();
            foreach (var category in AvailableCategories.Where(c => 
                c.Name.ToLowerInvariant().Contains(searchTerm) ||
                c.NameAr.ToLowerInvariant().Contains(searchTerm)))
            {
                FilteredCategories.Add(category);
            }
        }
    }

    /// <summary>
    /// Toggle select all shops
    /// </summary>
    [RelayCommand]
    private void ToggleSelectAllShops()
    {
        foreach (var shop in AvailableShops)
        {
            shop.IsSelected = SelectAllShops;
        }
        UpdateSelectedShops();
    }

    /// <summary>
    /// Toggle select all customers
    /// </summary>
    [RelayCommand]
    private void ToggleSelectAllCustomers()
    {
        foreach (var customer in AvailableCustomers)
        {
            customer.IsSelected = SelectAllCustomers;
        }
        UpdateSelectedCustomers();
    }

    /// <summary>
    /// Handle shop selection change
    /// </summary>
    [RelayCommand]
    private void ShopSelectionChanged(ShopSelectionViewModel shop)
    {
        UpdateSelectedShops();
        UpdateSelectAllShopsState();
    }

    /// <summary>
    /// Filter shops based on search text
    /// </summary>
    [RelayCommand]
    private void FilterShops()
    {
        if (string.IsNullOrWhiteSpace(ShopSearchText))
        {
            FilteredShops.Clear();
            foreach (var shop in AvailableShops)
            {
                FilteredShops.Add(shop);
            }
        }
        else
        {
            FilteredShops.Clear();
            var searchTerm = ShopSearchText.ToLowerInvariant();
            foreach (var shop in AvailableShops.Where(s => 
                s.Name.ToLowerInvariant().Contains(searchTerm) ||
                s.Address.ToLowerInvariant().Contains(searchTerm)))
            {
                FilteredShops.Add(shop);
            }
        }
    }

    /// <summary>
    /// Filter customers based on search text
    /// </summary>
    [RelayCommand]
    private void FilterCustomers()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            FilteredCustomers.Clear();
            foreach (var customer in AvailableCustomers)
            {
                FilteredCustomers.Add(customer);
            }
        }
        else
        {
            FilteredCustomers.Clear();
            var searchTerm = CustomerSearchText.ToLowerInvariant();
            foreach (var customer in AvailableCustomers.Where(c => 
                c.Name.ToLowerInvariant().Contains(searchTerm) ||
                c.Email.ToLowerInvariant().Contains(searchTerm) ||
                c.Phone.ToLowerInvariant().Contains(searchTerm)))
            {
                FilteredCustomers.Add(customer);
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the selected products collection
    /// </summary>
    private void UpdateSelectedProducts()
    {
        FileLogger.Log("DiscountSidePanelViewModel.UpdateSelectedProducts: Starting...");
        FileLogger.Log($"DiscountSidePanelViewModel.UpdateSelectedProducts: AvailableProducts.Count = {AvailableProducts.Count}");
        
        var selectedCount = AvailableProducts.Count(p => p.IsSelected);
        FileLogger.Log($"DiscountSidePanelViewModel.UpdateSelectedProducts: Products marked as selected = {selectedCount}");
        
        SelectedProducts.Clear();
        foreach (var product in AvailableProducts.Where(p => p.IsSelected))
        {
            FileLogger.Log($"DiscountSidePanelViewModel.UpdateSelectedProducts: Adding selected product - ID: {product.Id}, Name: '{product.Name}'");
            SelectedProducts.Add(product);
        }
        
        FileLogger.Log($"DiscountSidePanelViewModel.UpdateSelectedProducts: Final SelectedProducts.Count = {SelectedProducts.Count}");
    }

    /// <summary>
    /// Updates the selected categories collection
    /// </summary>
    private void UpdateSelectedCategories()
    {
        SelectedCategories.Clear();
        foreach (var category in AvailableCategories.Where(c => c.IsSelected))
        {
            SelectedCategories.Add(category);
        }
    }

    /// <summary>
    /// Updates the select all products state
    /// </summary>
    private void UpdateSelectAllProductsState()
    {
        if (AvailableProducts.Count == 0)
        {
            SelectAllProducts = false;
            return;
        }

        var selectedCount = AvailableProducts.Count(p => p.IsSelected);
        SelectAllProducts = selectedCount == AvailableProducts.Count;
    }

    /// <summary>
    /// Updates the select all categories state
    /// </summary>
    private void UpdateSelectAllCategoriesState()
    {
        if (AvailableCategories.Count == 0)
        {
            SelectAllCategories = false;
            return;
        }

        var selectedCount = AvailableCategories.Count(c => c.IsSelected);
        SelectAllCategories = selectedCount == AvailableCategories.Count;
    }

    /// <summary>
    /// Updates the selected shops collection
    /// </summary>
    private void UpdateSelectedShops()
    {
        SelectedShops.Clear();
        foreach (var shop in AvailableShops.Where(s => s.IsSelected))
        {
            SelectedShops.Add(shop);
        }
    }

    /// <summary>
    /// Updates the select all shops state
    /// </summary>
    private void UpdateSelectAllShopsState()
    {
        if (AvailableShops.Count == 0)
        {
            SelectAllShops = false;
            return;
        }

        var selectedCount = AvailableShops.Count(s => s.IsSelected);
        SelectAllShops = selectedCount == AvailableShops.Count;
    }

    /// <summary>
    /// Handle customer selection change
    /// </summary>
    private void CustomerSelectionChanged(CustomerSelectionViewModel customer)
    {
        UpdateSelectedCustomers();
        UpdateSelectAllCustomersState();
    }

    /// <summary>
    /// Updates the selected customers collection
    /// </summary>
    private void UpdateSelectedCustomers()
    {
        SelectedCustomers.Clear();
        foreach (var customer in AvailableCustomers.Where(c => c.IsSelected))
        {
            SelectedCustomers.Add(customer);
        }
    }

    /// <summary>
    /// Updates the select all customers state
    /// </summary>
    private void UpdateSelectAllCustomersState()
    {
        if (AvailableCustomers.Count == 0)
        {
            SelectAllCustomers = false;
            return;
        }

        var selectedCount = AvailableCustomers.Count(c => c.IsSelected);
        SelectAllCustomers = selectedCount == AvailableCustomers.Count;
    }

    /// <summary>
    /// Initialize settings from services
    /// </summary>
    private void InitializeSettings()
    {
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Initialize collections with enum values
    /// </summary>
    private void InitializeCollections()
    {
        // Discount Types
        DiscountTypes.Clear();
        foreach (DiscountType type in Enum.GetValues<DiscountType>())
        {
            DiscountTypes.Add(type);
        }

        // Applicable On Options
        ApplicableOnOptions.Clear();
        foreach (DiscountApplicableOn option in Enum.GetValues<DiscountApplicableOn>())
        {
            ApplicableOnOptions.Add(option);
        }

        // Currencies
        Currencies.Clear();
        Currencies.Add("USD");
        Currencies.Add("EUR");
        Currencies.Add("AED");
        Currencies.Add("SAR");
        Currencies.Add("GBP");
    }

    /// <summary>
    /// Load data from services
    /// </summary>
    private async Task LoadDataAsync()
    {
        try
        {
            FileLogger.LogSeparator();
            FileLogger.Log("=== DISCOUNT SIDE PANEL DATA LOADING SESSION STARTED ===");
            FileLogger.Log("DiscountSidePanelViewModel.LoadDataAsync: Starting complete data load...");
            
            IsLoading = true;

            // Load products, categories, customers, and stores in parallel
            var tasks = new[]
            {
                LoadProductsAsync(),
                LoadCategoriesAsync(),
                LoadCustomersAsync(),
                LoadStoresAsync()
            };

            FileLogger.Log("DiscountSidePanelViewModel.LoadDataAsync: Starting parallel tasks...");
            await Task.WhenAll(tasks);
            FileLogger.Log("DiscountSidePanelViewModel.LoadDataAsync: All parallel tasks completed");
            
            // If we're in edit mode, restore the selected products/categories after data is loaded
            if (_isEditMode && _editingDiscount != null)
            {
                FileLogger.Log("DiscountSidePanelViewModel.LoadDataAsync: Restoring selections for edit mode...");
                RestoreSelectionsFromDiscount(_editingDiscount);
            }
            
            FileLogger.Log("=== DISCOUNT SIDE PANEL DATA LOADING SESSION COMPLETED ===");
            FileLogger.LogSeparator();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Load products for selection
    /// </summary>
    private async Task LoadProductsAsync()
    {
        try
        {
            FileLogger.Log("DiscountSidePanelViewModel.LoadProductsAsync: Starting...");
            
            FileLogger.Log("DiscountSidePanelViewModel.LoadProductsAsync: Calling _productService.GetAllProductsAsync()");
            var products = await _productService.GetAllProductsAsync();
            
            FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: ProductService returned {products?.Count() ?? 0} products");
            
            if (products != null)
            {
                var activeProducts = products.Where(p => p.IsActive).ToList();
                FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: Found {activeProducts.Count} active products");
                
                // Apply stackable filtering based on current discount type
                var filteredProducts = ApplyStackableFiltering(activeProducts);
                FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: After stackable filtering: {filteredProducts.Count} products");
                
                // Marshal collection operations to UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FileLogger.Log("DiscountSidePanelViewModel.LoadProductsAsync: Clearing existing collections on UI thread");
                    AvailableProducts.Clear();
                    FilteredProducts.Clear();
                    
                foreach (var product in filteredProducts)
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: Adding product - ID: {product.Id}, Name: '{product.Name}'");
                    var productVM = new ProductSelectionViewModel(product);
                    
                    // Wire up the PropertyChanged event to track selection changes
                    productVM.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(ProductSelectionViewModel.IsSelected))
                        {
                            ProductSelectionChanged(productVM);
                        }
                    };
                    
                    AvailableProducts.Add(productVM);
                    FilteredProducts.Add(productVM);
                }                    FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: Final counts - AvailableProducts: {AvailableProducts.Count}, FilteredProducts: {FilteredProducts.Count}");
                });
            }
            else
            {
                FileLogger.Log("DiscountSidePanelViewModel.LoadProductsAsync: ProductService returned null");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: ERROR - {ex.Message}");
            FileLogger.Log($"DiscountSidePanelViewModel.LoadProductsAsync: Stack trace - {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies stackable filtering to products based on current discount type and existing discount assignments
    /// </summary>
    private List<ProductDto> ApplyStackableFiltering(List<ProductDto> products)
    {
        try
        {
            FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: Starting with {products.Count} products");
            FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: Current discount IsStackable: {IsStackable}");
            FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: IsEditMode: {_isEditMode}, EditingDiscountId: {_editingDiscountId}");

            // If current discount is stackable, no filtering needed (stackable can be added to any product)
            if (IsStackable)
            {
                FileLogger.Log("DiscountSidePanelViewModel.ApplyStackableFiltering: Current discount is stackable, no filtering needed");
                return products;
            }

            // If current discount is non-stackable, filter out products that already have non-stackable discounts
            // (unless we're editing and it's the same discount)
            var filteredProducts = new List<ProductDto>();
            
            foreach (var product in products)
            {
                bool canAddToProduct = true;
                
                // Check if product has any active discounts
                if (product.ActiveDiscounts?.Any() == true)
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: Product '{product.Name}' has {product.ActiveDiscounts.Count} active discounts");
                    
                    // Check for non-stackable discounts
                    var hasNonStackableDiscounts = product.ActiveDiscounts.Any(d => 
                    {
                        // If we're editing, exclude the current discount from the check
                        bool isCurrentDiscount = _isEditMode && d.Id == _editingDiscountId;
                        bool isNonStackable = !d.IsCurrentlyActive || !IsStackableDiscount(d);
                        
                        FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: Discount '{d.DiscountName}' - IsCurrentDiscount: {isCurrentDiscount}, IsNonStackable: {isNonStackable}");
                        
                        return !isCurrentDiscount && isNonStackable;
                    });
                    
                    if (hasNonStackableDiscounts)
                    {
                        FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: Product '{product.Name}' excluded - has non-stackable discounts");
                        canAddToProduct = false;
                    }
                }
                
                if (canAddToProduct)
                {
                    filteredProducts.Add(product);
                }
            }
            
            FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: Filtered from {products.Count} to {filteredProducts.Count} products");
            return filteredProducts;
        }
        catch (Exception ex)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.ApplyStackableFiltering: ERROR - {ex.Message}");
            // Return all products if filtering fails
            return products;
        }
    }

    /// <summary>
    /// Determines if a discount display DTO represents a stackable discount
    /// </summary>
    private bool IsStackableDiscount(DiscountDisplayDto discount)
    {
        return discount.IsStackable;
    }

    /// <summary>
    /// Load categories for selection
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            FileLogger.Log("DiscountSidePanelViewModel.LoadCategoriesAsync: Starting...");
            
            FileLogger.Log("DiscountSidePanelViewModel.LoadCategoriesAsync: Calling _productService.GetAllCategoriesAsync()");
            var categories = await _productService.GetAllCategoriesAsync();
            
            FileLogger.Log($"DiscountSidePanelViewModel.LoadCategoriesAsync: ProductService returned {categories?.Count() ?? 0} categories");
            
            if (categories != null)
            {
                var activeCategories = categories.Where(c => c.IsActive).ToList();
                FileLogger.Log($"DiscountSidePanelViewModel.LoadCategoriesAsync: Found {activeCategories.Count} active categories");
                
                // Marshal collection operations to UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FileLogger.Log("DiscountSidePanelViewModel.LoadCategoriesAsync: Clearing existing collections on UI thread");
                    AvailableCategories.Clear();
                    FilteredCategories.Clear();
                    
                    foreach (var category in activeCategories)
                    {
                        FileLogger.Log($"DiscountSidePanelViewModel.LoadCategoriesAsync: Adding category - ID: {category.Id}, Name: '{category.Name}'");
                        var categoryVM = new CategorySelectionViewModel(category);
                        
                        // Wire up the PropertyChanged event to track selection changes
                        categoryVM.PropertyChanged += (sender, e) =>
                        {
                            if (e.PropertyName == nameof(CategorySelectionViewModel.IsSelected))
                            {
                                CategorySelectionChanged(categoryVM);
                            }
                        };
                        
                        AvailableCategories.Add(categoryVM);
                        FilteredCategories.Add(categoryVM);
                    }
                    
                    FileLogger.Log($"DiscountSidePanelViewModel.LoadCategoriesAsync: Final counts - AvailableCategories: {AvailableCategories.Count}, FilteredCategories: {FilteredCategories.Count}");
                });
            }
            else
            {
                FileLogger.Log("DiscountSidePanelViewModel.LoadCategoriesAsync: ProductService returned null");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.LoadCategoriesAsync: ERROR - {ex.Message}");
            FileLogger.Log($"DiscountSidePanelViewModel.LoadCategoriesAsync: Stack trace - {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
    }

    /// <summary>
    /// Load customers for selection
    /// </summary>
    private async Task LoadCustomersAsync()
    {
        try
        {
            if (_customerService == null)
            {
                FileLogger.Log("LoadCustomersAsync: _customerService is null");
                return;
            }

            FileLogger.Log("LoadCustomersAsync: Starting to load customers");
            var customers = await _customerService.GetAllAsync();
            FileLogger.Log($"LoadCustomersAsync: Loaded {customers?.Count() ?? 0} customers");

            if (customers != null)
            {
                var customerList = customers.ToList();
                FileLogger.Log($"LoadCustomersAsync: Found {customerList.Count} customers");
                
                // Marshal collection operations to UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FileLogger.Log("LoadCustomersAsync: Clearing existing collections on UI thread");
                    AvailableCustomers.Clear();
                    FilteredCustomers.Clear();
                    
                    foreach (var customer in customerList)
                    {
                        FileLogger.Log($"LoadCustomersAsync: Adding customer - ID: {customer.Id}, Name: '{customer.CustomerFullName}'");
                        var customerVM = new CustomerSelectionViewModel(customer);
                        
                        // Wire up selection change event
                        customerVM.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(customerVM.IsSelected))
                            {
                                CustomerSelectionChanged(customerVM);
                            }
                        };
                        
                        AvailableCustomers.Add(customerVM);
                        FilteredCustomers.Add(customerVM);
                    }
                    
                    FileLogger.Log($"LoadCustomersAsync: Added {AvailableCustomers.Count} customers to AvailableCustomers");
                    FileLogger.Log($"LoadCustomersAsync: Added {FilteredCustomers.Count} customers to FilteredCustomers");
                });
            }
            else
            {
                FileLogger.Log("LoadCustomersAsync: customers is null");
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"Error loading customers: {ex.Message}");
            FileLogger.Log($"Error loading customers stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
        }
    }

    /// <summary>
    /// Load stores for selection
    /// </summary>
    private async Task LoadStoresAsync()
    {
        try
        {
            FileLogger.Log("DiscountSidePanelViewModel.LoadStoresAsync: Starting...");
            
            if (_storeService == null)
            {
                FileLogger.Log("DiscountSidePanelViewModel.LoadStoresAsync: StoreService is null");
                return;
            }

            var stores = await _storeService.GetActiveStoresAsync();
            FileLogger.Log($"DiscountSidePanelViewModel.LoadStoresAsync: Retrieved {stores?.Count() ?? 0} stores");

            if (stores != null && stores.Any())
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AvailableShops.Clear();
                    FilteredShops.Clear();

                    foreach (var store in stores)
                    {
                        var shopVM = new ShopSelectionViewModel(store);
                        shopVM.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(ShopSelectionViewModel.IsSelected))
                            {
                                ShopSelectionChanged(shopVM);
                            }
                        };
                        AvailableShops.Add(shopVM);
                        FilteredShops.Add(shopVM);
                    }

                    FileLogger.Log($"DiscountSidePanelViewModel.LoadStoresAsync: Loaded {AvailableShops.Count} shops");
                });
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.LoadStoresAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error loading stores: {ex.Message}");
        }
    }

    /// <summary>
    /// Filter products based on search text
    /// </summary>
    private void FilterProducts()
    {
        FilteredProducts.Clear();
        if (string.IsNullOrWhiteSpace(ProductSearchText))
        {
            foreach (var product in AvailableProducts)
            {
                FilteredProducts.Add(product);
            }
        }
        else
        {
            var searchTerm = ProductSearchText.ToLowerInvariant();
            foreach (var product in AvailableProducts.Where(p => 
                p.Name.ToLowerInvariant().Contains(searchTerm) ||
                p.Code.ToLowerInvariant().Contains(searchTerm)))
            {
                FilteredProducts.Add(product);
            }
        }
    }

    /// <summary>
    /// <summary>
    /// Load discount data for editing
    /// </summary>
    private void LoadDiscountData(DiscountDto discount)
    {
        FileLogger.Log("DiscountSidePanelViewModel.LoadDiscountData: Starting...");
        
        DiscountName = discount.DiscountName;
        DiscountNameAr = discount.DiscountNameAr ?? string.Empty;
        DiscountCode = discount.DiscountCode;
        DiscountDescription = discount.DiscountDescription ?? string.Empty;
        DiscountType = discount.DiscountType;
        DiscountValue = discount.DiscountValue;
        MaxDiscountAmount = discount.MaxDiscountAmount;
        CurrencyCode = discount.CurrencyCode;
        ApplicableOn = discount.ApplicableOn;
        StartDate = discount.StartDate;
        EndDate = discount.EndDate;
        MinPurchaseAmount = discount.MinPurchaseAmount;
        Priority = discount.Priority;
        IsStackable = discount.IsStackable;
        IsActive = discount.IsActive;
        
        // Set flags based on values
        HasMinPurchaseAmount = discount.MinPurchaseAmount.HasValue;
        
        FileLogger.Log($"DiscountSidePanelViewModel.LoadDiscountData: Loaded basic properties - ApplicableOn = {ApplicableOn}");
        
        // Store discount data for restoring selections after data loads
        _editingDiscount = discount;
        
        FileLogger.Log("DiscountSidePanelViewModel.LoadDiscountData: Stored _editingDiscount for later selection restoration");
        FileLogger.Log("DiscountSidePanelViewModel.LoadDiscountData: Completed - selections will be restored after LoadDataAsync");
        
        // NOTE: We don't call SetSelectedProductsFromDiscount here anymore
        // because the products/categories haven't been loaded yet.
        // The selection will be restored in LoadDataAsync after data loading completes.
    }

    /// <summary>
    /// Restore selected products and categories from discount data after data loading completes
    /// </summary>
    private void RestoreSelectionsFromDiscount(DiscountDto discount)
    {
        FileLogger.LogSeparator();
        FileLogger.Log("=== DISCOUNT EDIT MODE SELECTION RESTORATION STARTED ===");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: ApplicableOn = {discount.ApplicableOn}");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: SelectedProductIds count = {discount.SelectedProductIds?.Count ?? 0}");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: SelectedCategoryIds count = {discount.SelectedCategoryIds?.Count ?? 0}");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: AvailableProducts.Count = {AvailableProducts.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: AvailableCategories.Count = {AvailableCategories.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: AvailableShops.Count = {AvailableShops.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: StoreId = {discount.StoreId}");
        
        if (discount.ApplicableOn == DiscountApplicableOn.Product)
        {
            FileLogger.Log("DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: Restoring PRODUCT selections...");
            SetSelectedProductsFromDiscount(discount);
        }
        else if (discount.ApplicableOn == DiscountApplicableOn.Category)
        {
            FileLogger.Log("DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: Restoring CATEGORY selections...");
            SetSelectedCategoriesFromDiscount(discount);
        }
        else if (discount.ApplicableOn == DiscountApplicableOn.Shop)
        {
            FileLogger.Log("DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: Restoring SHOP selections...");
            SetSelectedShopsFromDiscount(discount);
        }
        else if (discount.ApplicableOn == DiscountApplicableOn.Customer)
        {
            FileLogger.Log("DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: Restoring CUSTOMER selections...");
            SetSelectedCustomersFromDiscount(discount);
        }
        else
        {
            FileLogger.Log($"DiscountSidePanelViewModel.RestoreSelectionsFromDiscount: Unknown ApplicableOn value: {discount.ApplicableOn}");
        }
        
        FileLogger.Log("=== DISCOUNT EDIT MODE SELECTION RESTORATION COMPLETED ===");
        FileLogger.LogSeparator();
    }

    /// <summary>
    /// Set selected products from discount data
    /// </summary>
    private void SetSelectedProductsFromDiscount(DiscountDto discount)
    {
        FileLogger.Log("DiscountSidePanelViewModel.SetSelectedProductsFromDiscount: Starting...");
        FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedProductsFromDiscount: AvailableProducts.Count = {AvailableProducts.Count}");
        
        if (discount.SelectedProductIds?.Any() == true)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedProductsFromDiscount: Found {discount.SelectedProductIds.Count} product IDs to select: [{string.Join(", ", discount.SelectedProductIds)}]");
            
            foreach (var productId in discount.SelectedProductIds)
            {
                var product = AvailableProducts.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedProductsFromDiscount: Setting IsSelected=true for product ID {productId} ('{product.Name}')");
                    product.IsSelected = true;
                }
                else
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedProductsFromDiscount: WARNING - Product ID {productId} not found in AvailableProducts");
                }
            }
            UpdateSelectedProducts();
            UpdateSelectAllProductsState();
        }
        else
        {
            FileLogger.Log("DiscountSidePanelViewModel.SetSelectedProductsFromDiscount: No selected product IDs found in discount");
        }
    }

    /// <summary>
    /// Set selected categories from discount data
    /// </summary>
    private void SetSelectedCategoriesFromDiscount(DiscountDto discount)
    {
        FileLogger.Log("DiscountSidePanelViewModel.SetSelectedCategoriesFromDiscount: Starting...");
        FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCategoriesFromDiscount: AvailableCategories.Count = {AvailableCategories.Count}");
        
        if (discount.SelectedCategoryIds?.Any() == true)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCategoriesFromDiscount: Found {discount.SelectedCategoryIds.Count} category IDs to select: [{string.Join(", ", discount.SelectedCategoryIds)}]");
            
            foreach (var categoryId in discount.SelectedCategoryIds)
            {
                var category = AvailableCategories.FirstOrDefault(c => c.Id == categoryId);
                if (category != null)
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCategoriesFromDiscount: Setting IsSelected=true for category ID {categoryId} ('{category.Name}')");
                    category.IsSelected = true;
                }
                else
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCategoriesFromDiscount: WARNING - Category ID {categoryId} not found in AvailableCategories");
                }
            }
            UpdateSelectedCategories();
            UpdateSelectAllCategoriesState();
        }
        else
        {
            FileLogger.Log("DiscountSidePanelViewModel.SetSelectedCategoriesFromDiscount: No selected category IDs found in discount");
        }
    }

    /// <summary>
    /// Set selected shops from discount data
    /// </summary>
    private void SetSelectedShopsFromDiscount(DiscountDto discount)
    {
        FileLogger.Log("DiscountSidePanelViewModel.SetSelectedShopsFromDiscount: Starting...");
        FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedShopsFromDiscount: AvailableShops.Count = {AvailableShops.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedShopsFromDiscount: StoreId = {discount.StoreId}");
        
        if (discount.StoreId.HasValue)
        {
            var shop = AvailableShops.FirstOrDefault(s => s.Id == discount.StoreId.Value);
            if (shop != null)
            {
                FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedShopsFromDiscount: Setting IsSelected=true for shop ID {discount.StoreId.Value} ('{shop.Name}')");
                shop.IsSelected = true;
                UpdateSelectedShops();
                UpdateSelectAllShopsState();
            }
            else
            {
                FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedShopsFromDiscount: WARNING - Shop ID {discount.StoreId.Value} not found in AvailableShops");
            }
        }
        else
        {
            FileLogger.Log("DiscountSidePanelViewModel.SetSelectedShopsFromDiscount: No StoreId found in discount");
        }
    }

    /// <summary>
    /// Set selected customers from discount data
    /// </summary>
    private void SetSelectedCustomersFromDiscount(DiscountDto discount)
    {
        FileLogger.Log("DiscountSidePanelViewModel.SetSelectedCustomersFromDiscount: Starting...");
        FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCustomersFromDiscount: AvailableCustomers.Count = {AvailableCustomers.Count}");
        
        if (discount.SelectedCustomerIds?.Any() == true)
        {
            FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCustomersFromDiscount: Found {discount.SelectedCustomerIds.Count} customer IDs to select: [{string.Join(", ", discount.SelectedCustomerIds)}]");
            
            foreach (var customerId in discount.SelectedCustomerIds)
            {
                var customer = AvailableCustomers.FirstOrDefault(c => c.Id == customerId);
                if (customer != null)
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCustomersFromDiscount: Setting IsSelected=true for customer ID {customerId} ('{customer.Name}')");
                    customer.IsSelected = true;
                }
                else
                {
                    FileLogger.Log($"DiscountSidePanelViewModel.SetSelectedCustomersFromDiscount: WARNING - Customer ID {customerId} not found in AvailableCustomers");
                }
            }
            UpdateSelectedCustomers();
            UpdateSelectAllCustomersState();
        }
        else
        {
            FileLogger.Log("DiscountSidePanelViewModel.SetSelectedCustomersFromDiscount: No selected customer IDs found in discount");
        }
    }

    /// <summary>
    /// Validate form data
    /// </summary>
    private bool ValidateForm()
    {
        var validationResults = new List<DataAnnotationsValidationResult>();
        var context = new ValidationContext(this);
        
        if (!Validator.TryValidateObject(this, context, validationResults, true))
        {
            ValidationMessage = validationResults.First().ErrorMessage ?? "Validation failed";
            return false;
        }

        // Additional custom validations
        if (EndDate <= StartDate)
        {
            ValidationMessage = "End date must be after start date";
            return false;
        }

        if (DiscountType == DiscountType.Percentage && DiscountValue > 100)
        {
            ValidationMessage = "Percentage discount cannot exceed 100%";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Create DTO for new discount
    /// </summary>
    private CreateDiscountDto CreateDiscountDto()
    {
        FileLogger.Log("DiscountSidePanelViewModel.CreateDiscountDto: Starting...");
        FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: ApplicableOn = {ApplicableOn}");
        FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: AvailableProducts.Count = {AvailableProducts.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: SelectedProducts.Count = {SelectedProducts.Count}");
        
        var selectedProductIds = SelectedProducts.Select(p => p.Id).ToList();
        var selectedCategoryIds = SelectedCategories.Select(c => c.Id).ToList();
        var selectedCustomerIds = SelectedCustomers.Select(c => c.Id).ToList();
        
        FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: selectedProductIds.Count = {selectedProductIds.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: selectedCategoryIds.Count = {selectedCategoryIds.Count}");
        FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: selectedCustomerIds.Count = {selectedCustomerIds.Count}");
        
        if (selectedProductIds.Any())
        {
            FileLogger.Log($"DiscountSidePanelViewModel.CreateDiscountDto: Selected product IDs: [{string.Join(", ", selectedProductIds)}]");
        }
        
        return new CreateDiscountDto
        {
            DiscountName = DiscountName,
            DiscountNameAr = string.IsNullOrWhiteSpace(DiscountNameAr) ? null : DiscountNameAr,
            DiscountDescription = string.IsNullOrWhiteSpace(DiscountDescription) ? null : DiscountDescription,
            DiscountCode = DiscountCode,
            DiscountType = DiscountType,
            DiscountValue = DiscountValue,
            MaxDiscountAmount = DiscountType == DiscountType.Percentage ? MaxDiscountAmount : null,
            MinPurchaseAmount = HasMinPurchaseAmount ? MinPurchaseAmount : null,
            StartDate = StartDate,
            EndDate = EndDate,
            ApplicableOn = ApplicableOn,
            SelectedProductIds = selectedProductIds,
            SelectedCategoryIds = selectedCategoryIds,
            SelectedCustomerIds = selectedCustomerIds,
            Priority = Priority,
            IsStackable = IsStackable,
            IsActive = IsActive,
            StoreId = ApplicableOn == DiscountApplicableOn.Shop && SelectedShops.Any() ? SelectedShops.First().Id : null,
            CurrencyCode = CurrencyCode,
            CreatedBy = 1 // TODO: Get current user ID
        };
    }

    /// <summary>
    /// Create DTO for updating discount
    /// </summary>
    private UpdateDiscountDto CreateUpdateDiscountDto()
    {
        return new UpdateDiscountDto
        {
            DiscountName = DiscountName,
            DiscountNameAr = string.IsNullOrWhiteSpace(DiscountNameAr) ? null : DiscountNameAr,
            DiscountDescription = string.IsNullOrWhiteSpace(DiscountDescription) ? null : DiscountDescription,
            DiscountCode = DiscountCode,
            DiscountType = DiscountType,
            DiscountValue = DiscountValue,
            MaxDiscountAmount = DiscountType == DiscountType.Percentage ? MaxDiscountAmount : null,
            MinPurchaseAmount = HasMinPurchaseAmount ? MinPurchaseAmount : null,
            StartDate = StartDate,
            EndDate = EndDate,
            ApplicableOn = ApplicableOn,
            SelectedProductIds = SelectedProducts.Select(p => p.Id).ToList(),
            SelectedCategoryIds = SelectedCategories.Select(c => c.Id).ToList(),
            SelectedCustomerIds = SelectedCustomers.Select(c => c.Id).ToList(),
            Priority = Priority,
            IsStackable = IsStackable,
            IsActive = IsActive,
            StoreId = ApplicableOn == DiscountApplicableOn.Shop && SelectedShops.Any() ? SelectedShops.First().Id : null,
            CurrencyCode = CurrencyCode,
            UpdatedBy = 1 // TODO: Get current user ID
        };
    }

    /// <summary>
    /// Handle property changes
    /// </summary>
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DiscountType):
                OnPropertyChanged(nameof(IsPercentageDiscount));
                break;
            case nameof(ApplicableOn):
                OnPropertyChanged(nameof(IsProductApplicable));
                OnPropertyChanged(nameof(IsCategoryApplicable));
                OnPropertyChanged(nameof(IsCustomerApplicable));
                OnPropertyChanged(nameof(IsShopApplicable));
                break;
            case nameof(ProductSearchText):
                FilterProducts();
                break;
            case nameof(CategorySearchText):
                FilterCategories();
                break;
            case nameof(CustomerSearchText):
                FilterCustomers();
                break;
            case nameof(ShopSearchText):
                FilterShops();
                break;
        }
    }

    #endregion

    #region Auto-Suggestion Logic
    
    /// <summary>
    /// Handles auto-suggestion of discount code when discount name changes
    /// </summary>
    partial void OnDiscountNameChanged(string value)
    {
        // Only auto-suggest if we're in Add mode and the discount code is empty or still auto-generated
        if (_isEditMode || string.IsNullOrWhiteSpace(value)) return;
        
        // Generate suggested code: DiscountName + CurrentYear
        var currentYear = DateTime.Now.Year.ToString();
        var sanitizedName = SanitizeDiscountName(value);
        var suggestedCode = $"{sanitizedName}{currentYear}";
        
        // Only update if the current code is empty or looks like an old auto-generated code
        if (string.IsNullOrWhiteSpace(DiscountCode) || IsAutoGeneratedCode(DiscountCode))
        {
            DiscountCode = suggestedCode;
        }
    }
    
    /// <summary>
    /// Sanitizes discount name for use in discount code
    /// </summary>
    private string SanitizeDiscountName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;
        
        // Remove spaces, special characters, keep only alphanumeric
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray());
        
        // Capitalize first letter of each word and limit length
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = string.Join("", words.Select(w => 
            char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1).ToLower() : "")));
        
        // Limit to reasonable length (20 chars max before year)
        return result.Length > 20 ? result.Substring(0, 20) : result;
    }
    
    /// <summary>
    /// Checks if a discount code looks like it was auto-generated
    /// </summary>
    private bool IsAutoGeneratedCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        
        // Check if it ends with a 4-digit year (2020-2030 range)
        if (code.Length >= 4)
        {
            var lastFour = code.Substring(code.Length - 4);
            if (int.TryParse(lastFour, out int year) && year >= 2020 && year <= 2030)
            {
                return true;
            }
        }
        
        return false;
    }
    
    #endregion

    #region Dispose

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// ViewModel for product selection with checkbox
/// </summary>
public partial class ProductSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _code = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    public ProductSelectionViewModel(ProductDto product)
    {
        Id = product.Id;
        Name = product.Name;
        Code = product.SKU ?? string.Empty;
        Price = product.Price;
        CategoryName = product.CategoryName;
    }
}

/// <summary>
/// ViewModel for category selection with checkbox
/// </summary>
public partial class CategorySelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _nameAr = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private int _productCount;

    public CategorySelectionViewModel(CategoryDto category)
    {
        Id = category.Id;
        Name = category.Name;
        NameAr = category.NameArabic ?? string.Empty;
        ProductCount = category.ProductCount;
    }
}

/// <summary>
/// ViewModel for shop/store selection with checkbox
/// </summary>
public partial class ShopSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isActive;

    public ShopSelectionViewModel(StoreDto store)
    {
        Id = store.Id;
        Name = store.Name;
        Address = store.Address ?? string.Empty;
        IsActive = store.IsActive;
    }
}
/// <summary>
/// ViewModel for customer selection in multi-select list
/// </summary>
public partial class CustomerSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public CustomerSelectionViewModel(CustomerDto customer)
    {
        Id = customer.Id;
        Name = customer.CustomerFullName;
        Email = customer.OfficialEmail ?? string.Empty;
        Phone = customer.MobileNo ?? string.Empty;
    }
}