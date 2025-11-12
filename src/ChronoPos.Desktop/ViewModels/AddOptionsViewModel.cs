using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Views.Dialogs;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Others page with all add-on modules
/// Provides 14 different modules for various data management tasks
/// </summary>
public partial class AddOptionsViewModel : ObservableObject
{
    #region Private Fields

    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;
    private readonly ISellingPriceTypeService? _sellingPriceTypeService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly ICustomerService _customerService;
    private readonly ICustomerGroupService _customerGroupService;
    private readonly ISupplierService _supplierService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICategoryService _categoryService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductCombinationItemService _productCombinationItemService;
    private readonly IDiscountService _discountService;
    private readonly IProductGroupService _productGroupService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IBrandService _brandService;
    private readonly IUomService _uomService;
    private readonly IStoreService _storeService;
    private readonly ICurrencyService _currencyService;
    private readonly IProductModifierService _productModifierService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of modules in Others section
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AddOptionsModuleInfo> _addOptionsModules = new();

    /// <summary>
    /// Current page title with localization support
    /// </summary>
    [ObservableProperty]
    private string _pageTitle = "Others";

    /// <summary>
    /// Current theme (Light/Dark)
    /// </summary>
    [ObservableProperty]
    private string _currentTheme = "Light";

    /// <summary>
    /// Current zoom level percentage
    /// </summary>
    [ObservableProperty]
    private double _currentZoom = 100.0;

    /// <summary>
    /// Current language for UI
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
    /// Refresh button text with localization support
    /// </summary>
    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    /// <summary>
    /// Visibility flag for Brand module
    /// </summary>
    [ObservableProperty]
    private bool _isBrandVisible = true;

    /// <summary>
    /// Visibility flag for Category module
    /// </summary>
    [ObservableProperty]
    private bool _isCategoryVisible = true;

    /// <summary>
    /// Visibility flag for Discounts module
    /// </summary>
    [ObservableProperty]
    private bool _isDiscountsVisible = true;

    /// <summary>
    /// Visibility flag for UOM module
    /// </summary>
    [ObservableProperty]
    private bool _isUomVisible = true;

    /// <summary>
    /// Visibility flag for Product Attributes module
    /// </summary>
    [ObservableProperty]
    private bool _isProductAttributesVisible = true;

    /// <summary>
    /// Visibility flag for Product Modifiers module
    /// </summary>
    [ObservableProperty]
    private bool _isProductModifiersVisible = true;

    /// <summary>
    /// Visibility flag for Product Combinations module
    /// </summary>
    [ObservableProperty]
    private bool _isProductCombinationsVisible = true;

    /// <summary>
    /// Visibility flag for Product Groups module
    /// </summary>
    [ObservableProperty]
    private bool _isProductGroupsVisible = true;

    /// <summary>
    /// Visibility flag for Price Types module
    /// </summary>
    [ObservableProperty]
    private bool _isPriceTypesVisible = true;

    /// <summary>
    /// Visibility flag for Payment Types module
    /// </summary>
    [ObservableProperty]
    private bool _isPaymentTypesVisible = true;

    /// <summary>
    /// Visibility flag for Tax Rates module
    /// </summary>
    [ObservableProperty]
    private bool _isTaxRatesVisible = true;

    /// <summary>
    /// Visibility flag for Customers module
    /// </summary>
    [ObservableProperty]
    private bool _isCustomersVisible = true;

    /// <summary>
    /// Visibility flag for Customer Groups module
    /// </summary>
    [ObservableProperty]
    private bool _isCustomerGroupsVisible = true;

    /// <summary>
    /// Visibility flag for Suppliers module
    /// </summary>
    [ObservableProperty]
    private bool _isSuppliersVisible = true;

    /// <summary>
    /// Visibility flag for Shop/Store module
    /// </summary>
    [ObservableProperty]
    private bool _isShopVisible = true;

    /// <summary>
    /// Visibility flag for Warehouses module
    /// </summary>
    [ObservableProperty]
    private bool _isWarehousesVisible = true;

    /// <summary>
    /// Visibility flag for Currency module
    /// </summary>
    [ObservableProperty]
    private bool _isCurrencyVisible = true;

    #endregion

    #region Commands

    /// <summary>
    /// Navigation action for module navigation (set by parent)
    /// </summary>
    public Action<string>? NavigateToModuleAction { get; set; }

    /// <summary>
    /// Back navigation action (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    /// <summary>
    /// Command to navigate to a specific module in Others section
    /// </summary>
    [RelayCommand]
    private void NavigateToModule(string moduleType)
    {
        if (NavigateToModuleAction != null)
        {
            NavigateToModuleAction(moduleType);
        }
        else
        {
            // Fallback for debug
            new MessageDialog("Navigation", $"Navigating to {moduleType} module", MessageDialog.MessageType.Info).ShowDialog();
        }
    }

    /// <summary>
    /// Command to go back to previous screen
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        if (GoBackAction != null)
        {
            GoBackAction();
        }
        else
        {
            // Fallback for debug
            new MessageDialog("Navigation", "Going back to Management", MessageDialog.MessageType.Info).ShowDialog();
        }
    }

    /// <summary>
    /// Command to refresh modules and reload data
    /// </summary>
    [RelayCommand]
    private async Task RefreshModules()
    {
        await LoadModulesAsync();
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services for full theme and localization support
    /// </summary>
    public AddOptionsViewModel(
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService,
        ITaxTypeService taxTypeService,
        ICustomerService customerService,
        ICustomerGroupService customerGroupService,
        ISupplierService supplierService,
        ICurrentUserService currentUserService,
        ICategoryService categoryService,
        IProductAttributeService productAttributeService,
        IProductCombinationItemService productCombinationItemService,
        IDiscountService discountService,
        IProductGroupService productGroupService,
        IPaymentTypeService paymentTypeService,
        IBrandService brandService,
        IUomService uomService,
        IStoreService storeService,
        ICurrencyService currencyService,
        IProductModifierService productModifierService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _taxTypeService = taxTypeService ?? throw new ArgumentNullException(nameof(taxTypeService));
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _customerGroupService = customerGroupService ?? throw new ArgumentNullException(nameof(customerGroupService));
        _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _productAttributeService = productAttributeService ?? throw new ArgumentNullException(nameof(productAttributeService));
        _productCombinationItemService = productCombinationItemService ?? throw new ArgumentNullException(nameof(productCombinationItemService));
        _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _productGroupService = productGroupService ?? throw new ArgumentNullException(nameof(productGroupService));
        _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _productModifierService = productModifierService ?? throw new ArgumentNullException(nameof(productModifierService));

        // Subscribe to service events (commented out until proper event signatures are confirmed)
        // _themeService.ThemeChanged += OnThemeChanged;
        // _zoomService.ZoomChanged += OnZoomChanged;
        // _colorSchemeService.ColorSchemeChanged += OnColorSchemeChanged;
        // _layoutDirectionService.DirectionChanged += OnDirectionChanged;

        // Initialize with current values
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = _zoomService.CurrentZoomPercentage;
        CurrentColorScheme = _colorSchemeService.CurrentPrimaryColor.Name;
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Initialize module visibility based on user permissions
        InitializeModuleVisibility();

        // Load modules
        _ = Task.Run(LoadModulesAsync);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize module visibility based on user permissions
    /// </summary>
    private void InitializeModuleVisibility()
    {
        try
        {
            // Check permissions for each module and set visibility flags
            IsBrandVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.BRAND);
            IsCategoryVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.CATEGORY);
            IsDiscountsVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.DISCOUNTS);
            IsUomVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.UOM);
            IsProductAttributesVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.PRODUCT_ATTRIBUTES);
            IsProductModifiersVisible = true; // TODO: Add ScreenNames.PRODUCT_MODIFIERS when permission constant is added
            IsProductCombinationsVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.PRODUCT_COMBINATIONS);
            IsProductGroupsVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.PRODUCT_GROUPING);
            IsPriceTypesVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.PRICE_TYPES);
            IsPaymentTypesVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.PAYMENT_TYPES);
            IsTaxRatesVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.TAX_RATES);
            IsCustomersVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.CUSTOMERS);
            IsCustomerGroupsVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.CUSTOMER_GROUPS);
            IsSuppliersVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.SUPPLIERS);
            IsShopVisible = _currentUserService.HasAnyScreenPermission(ScreenNames.SHOP);
            // Note: Warehouses doesn't have a screen constant, so keeping it visible by default
            IsWarehousesVisible = true;
        }
        catch (Exception ex)
        {
            // If permission check fails, default to showing all modules (fail-open for better UX)
            // The actual permission checks at module level will still protect the screens
            System.Diagnostics.Debug.WriteLine($"Error checking module visibility: {ex.Message}");
            
            // Set all to visible as fallback
            IsBrandVisible = true;
            IsCategoryVisible = true;
            IsDiscountsVisible = true;
            IsUomVisible = true;
            IsProductAttributesVisible = true;
            IsProductModifiersVisible = true;
            IsProductCombinationsVisible = true;
            IsProductGroupsVisible = true;
            IsPriceTypesVisible = true;
            IsPaymentTypesVisible = true;
            IsTaxRatesVisible = true;
            IsCustomersVisible = true;
            IsCustomerGroupsVisible = true;
            IsSuppliersVisible = true;
            IsShopVisible = true;
            IsWarehousesVisible = true;
        }
    }

    /// <summary>
    /// Load all modules in Others section with their respective data
    /// </summary>
    private async Task LoadModulesAsync()
    {
        try
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                // Clear existing modules
                AddOptionsModules.Clear();

                // Get the primary color brush for all icons
                var primaryColorBrush = GetPrimaryColorBrush();
                var buttonBackgroundBrush = GetButtonBackgroundBrush();

                // Create all 11 modules with visibility flags (removed Customer, CustomerGroups, and Suppliers)
                var moduleData = new[]
                {
                    new { Type = "Brand", TitleKey = "add_options.brand", CountLabel = "Brands", Count = await GetBrandCountAsync(), IsVisible = IsBrandVisible },
                    new { Type = "Category", TitleKey = "add_options.category", CountLabel = "Categories", Count = await GetCategoryCountAsync(), IsVisible = IsCategoryVisible },
                    new { Type = "ProductAttributes", TitleKey = "add_options.product_attributes", CountLabel = "Attributes", Count = await GetProductAttributesCountAsync(), IsVisible = IsProductAttributesVisible },
                    new { Type = "ProductModifiers", TitleKey = "add_options.product_modifiers", CountLabel = "Modifiers", Count = await GetProductModifiersCountAsync(), IsVisible = IsProductModifiersVisible },
                    new { Type = "ProductCombinations", TitleKey = "add_options.product_combinations", CountLabel = "Combinations", Count = await GetProductCombinationsCountAsync(), IsVisible = IsProductCombinationsVisible },
                    new { Type = "ProductGrouping", TitleKey = "add_options.product_grouping", CountLabel = "Groups", Count = await GetProductGroupingCountAsync(), IsVisible = IsProductGroupsVisible },
                    new { Type = "PriceTypes", TitleKey = "add_options.price_types", CountLabel = "Price Types", Count = await GetPriceTypesCountAsync(), IsVisible = IsPriceTypesVisible },
                    new { Type = "PaymentTypes", TitleKey = "add_options.payment_types", CountLabel = "Payment Types", Count = await GetPaymentTypesCountAsync(), IsVisible = IsPaymentTypesVisible },
                    new { Type = "TaxRates", TitleKey = "add_options.tax_rates", CountLabel = "Tax Rates", Count = await GetTaxRatesCountAsync(), IsVisible = IsTaxRatesVisible },
                    new { Type = "UOM", TitleKey = "add_options.uom", CountLabel = "UOMs", Count = await GetUOMCountAsync(), IsVisible = IsUomVisible },
                    new { Type = "Shop", TitleKey = "add_options.shop", CountLabel = "Shops", Count = await GetShopCountAsync(), IsVisible = IsShopVisible },
                    new { Type = "Discounts", TitleKey = "add_options.discounts", CountLabel = "Discounts", Count = await GetDiscountsCountAsync(), IsVisible = IsDiscountsVisible },
                    new { Type = "Currency", TitleKey = "add_options.currency", CountLabel = "Currencies", Count = await GetCurrencyCountAsync(), IsVisible = IsCurrencyVisible }
                };

                // Add modules to collection - Only add visible modules
                for (int i = 0; i < moduleData.Length; i++)
                {
                    var data = moduleData[i];
                    
                    // Only add module if user has permission to see it
                    if (data.IsVisible)
                    {
                        AddOptionsModules.Add(new AddOptionsModuleInfo
                        {
                            ModuleType = data.Type,
                            Title = await _databaseLocalizationService.GetTranslationAsync(data.TitleKey) ?? data.Type,
                            ItemCount = data.Count,
                            ItemCountLabel = data.CountLabel,
                            IconBackground = primaryColorBrush,
                            ButtonBackground = buttonBackgroundBrush
                        });
                    }
                }
                
                // Update page title and refresh button text
                PageTitle = await _databaseLocalizationService.GetTranslationAsync("add_options.page_title") ?? "Others";
                RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("add_options.refresh_button") ?? "Refresh";
            });
        }
        catch (Exception ex)
        {
            // Log error (in production, use proper logging)
            System.Diagnostics.Debug.WriteLine($"Error loading modules: {ex.Message}");
        }
    }

    /// <summary>
    /// Get module colors based on current color scheme
    /// </summary>
    private Brush GetPrimaryColorBrush()
    {
        var primaryColor = _colorSchemeService.CurrentPrimaryColor;
        return new SolidColorBrush(primaryColor.Color);
    }

    /// <summary>
    /// Get button background brush based on current theme
    /// </summary>
    private Brush GetButtonBackgroundBrush()
    {
        // Return transparent for now, will use theme-based background
        return new SolidColorBrush(Colors.Transparent);
    }

    #endregion

    #region Data Count Methods (Mock implementations - replace with actual data service calls)

    private async Task<int> GetBrandCountAsync()
    {
        try
        {
            var brands = await _brandService.GetAllAsync();
            return brands.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Brand count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetCategoryCountAsync()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return categories.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Category count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetProductAttributesCountAsync()
    {
        try
        {
            var attributes = await _productAttributeService.GetAllAttributesAsync();
            return attributes.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting ProductAttributes count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetProductModifiersCountAsync()
    {
        try
        {
            var modifiers = await _productModifierService.GetAllAsync();
            return modifiers.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting ProductModifier count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetProductCombinationsCountAsync()
    {
        try
        {
            var combinations = await _productCombinationItemService.GetAllCombinationItemsAsync();
            return combinations.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting ProductCombinations count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetProductGroupingCountAsync()
    {
        try
        {
            return await _productGroupService.GetCountAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting ProductGrouping count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetPriceTypesCountAsync()
    {
        try 
        {
            if (_sellingPriceTypeService != null)
            {
                return await _sellingPriceTypeService.GetCountAsync();
            }
            
            // Fallback to mock count
            await Task.Delay(50);
            return 5; // Number of seed price types
        }
        catch
        {
            return 0;
        }
    }

    private async Task<int> GetPaymentTypesCountAsync()
    {
        try
        {
            return await _paymentTypeService.GetCountAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting PaymentTypes count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetTaxRatesCountAsync()
    {
        try
        {
            var taxTypes = await _taxTypeService.GetAllTaxTypesAsync();
            return taxTypes.Count();
        }
        catch
        {
            return 0; // Return 0 if there's an error loading tax types
        }
    }

    private async Task<int> GetCustomerCountAsync()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return customers.Count();
        }
        catch
        {
            return 0; // Return 0 if there's an error loading customers
        }
    }

    private async Task<int> GetSuppliersCountAsync()
    {
        try
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            return suppliers.Count();
        }
        catch (Exception)
        {
            return 0; // Return 0 if there's an error
        }
    }

    private async Task<int> GetUOMCountAsync()
    {
        try
        {
            var uoms = await _uomService.GetAllAsync();
            return uoms.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting UOM count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetShopCountAsync()
    {
        try
        {
            var stores = await _storeService.GetAllAsync();
            return stores.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Store count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetCustomerGroupsCountAsync()
    {
        try
        {
            return await _customerGroupService.GetCountAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Customer Groups count: {ex.Message}");
        }
        
        // Fallback to mock data
        await Task.Delay(50);
        return 0;
    }

    private async Task<int> GetDiscountsCountAsync()
    {
        try
        {
            var discounts = await _discountService.GetAllAsync();
            return discounts.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Discounts count: {ex.Message}");
            return 0;
        }
    }

    private async Task<int> GetCurrencyCountAsync()
    {
        try
        {
            var currencies = await _currencyService.GetAllAsync();
            return currencies.Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Currency count: {ex.Message}");
            return 0;
        }
    }

    #endregion

    #region Event Handlers (commented out until proper event signatures are confirmed)

    /*
    private void OnThemeChanged(object? sender, EventArgs e)
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
    }

    private void OnZoomChanged(object? sender, EventArgs e)
    {
        CurrentZoom = _zoomService.CurrentZoomPercentage;
    }

    private void OnColorSchemeChanged(object? sender, EventArgs e)
    {
        CurrentColorScheme = _colorSchemeService.CurrentPrimaryColor.Name;
        // Reload modules to update colors
        _ = Task.Run(LoadModulesAsync);
    }

    private void OnDirectionChanged(object? sender, EventArgs e)
    {
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }
    */

    #endregion
}

/// <summary>
/// Information about a module in the Others section
/// </summary>
public class AddOptionsModuleInfo : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private string _moduleType = string.Empty;
    private int _itemCount = 0;
    private string _itemCountLabel = string.Empty;
    private Brush _iconBackground = new SolidColorBrush(Colors.DodgerBlue);
    private Brush _buttonBackground = new SolidColorBrush(Colors.White);

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}