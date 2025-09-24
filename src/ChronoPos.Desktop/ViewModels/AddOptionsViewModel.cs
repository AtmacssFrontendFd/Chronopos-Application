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
using ChronoPos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Add Options page with all add-on modules
/// Provides 14 different add option modules for various data management tasks
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
    private readonly ISupplierService _supplierService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of add options modules
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<AddOptionsModuleInfo> _addOptionsModules = new();

    /// <summary>
    /// Current page title with localization support
    /// </summary>
    [ObservableProperty]
    private string _pageTitle = "Add Options";

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
    /// Command to navigate to a specific add options module
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
            MessageBox.Show($"Navigating to {moduleType} module", "Navigation", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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
            MessageBox.Show("Going back to Management", "Navigation", 
                MessageBoxButton.OK, MessageBoxImage.Information);
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
        ISupplierService supplierService)
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
        _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));

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

        // Load modules
        _ = Task.Run(LoadModulesAsync);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load all add options modules with their respective data
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

                // Create all 14 add options modules
                var moduleData = new[]
                {
                    new { Type = "Brand", TitleKey = "add_options.brand", CountLabel = "Brands", Count = await GetBrandCountAsync() },
                    new { Type = "Category", TitleKey = "add_options.category", CountLabel = "Categories", Count = await GetCategoryCountAsync() },
                    new { Type = "ProductAttributes", TitleKey = "add_options.product_attributes", CountLabel = "Attributes", Count = await GetProductAttributesCountAsync() },
                    new { Type = "ProductCombinations", TitleKey = "add_options.product_combinations", CountLabel = "Combinations", Count = await GetProductCombinationsCountAsync() },
                    new { Type = "ProductGrouping", TitleKey = "add_options.product_grouping", CountLabel = "Groups", Count = await GetProductGroupingCountAsync() },
                    new { Type = "PriceTypes", TitleKey = "add_options.price_types", CountLabel = "Price Types", Count = await GetPriceTypesCountAsync() },
                    new { Type = "PaymentTypes", TitleKey = "add_options.payment_types", CountLabel = "Payment Types", Count = await GetPaymentTypesCountAsync() },
                    new { Type = "TaxRates", TitleKey = "add_options.tax_rates", CountLabel = "Tax Rates", Count = await GetTaxRatesCountAsync() },
                    new { Type = "Customers", TitleKey = "add_options.customer", CountLabel = "Customers", Count = await GetCustomerCountAsync() },
                    new { Type = "Suppliers", TitleKey = "add_options.suppliers", CountLabel = "Suppliers", Count = await GetSuppliersCountAsync() },
                    new { Type = "UOM", TitleKey = "add_options.uom", CountLabel = "UOMs", Count = await GetUOMCountAsync() },
                    new { Type = "Shop", TitleKey = "add_options.shop", CountLabel = "Shops", Count = await GetShopCountAsync() },
                    new { Type = "CustomerGroups", TitleKey = "add_options.customer_groups", CountLabel = "Groups", Count = await GetCustomerGroupsCountAsync() },
                    new { Type = "Discounts", TitleKey = "add_options.discounts", CountLabel = "Discounts", Count = await GetDiscountsCountAsync() }
                };

                // Add modules to collection
                for (int i = 0; i < moduleData.Length; i++)
                {
                    var data = moduleData[i];
                    
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
                
                // Update page title
                PageTitle = await _databaseLocalizationService.GetTranslationAsync("add_options.page_title") ?? "Add Options";
            });
        }
        catch (Exception ex)
        {
            // Log error (in production, use proper logging)
            System.Diagnostics.Debug.WriteLine($"Error loading add options modules: {ex.Message}");
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
            // Get the Brand service from the service provider if available
            var serviceProvider = System.Windows.Application.Current?.Resources["ServiceProvider"] as IServiceProvider;
            if (serviceProvider != null)
            {
                var brandService = serviceProvider.GetService<IBrandService>();
                if (brandService != null)
                {
                    var brands = await brandService.GetAllAsync();
                    return brands.Count();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting Brand count: {ex.Message}");
        }
        
        // Fallback to mock data
        await Task.Delay(50);
        return 45; // Mock count
    }

    private async Task<int> GetCategoryCountAsync()
    {
        await Task.Delay(50);
        return 28;
    }

    private async Task<int> GetProductAttributesCountAsync()
    {
        await Task.Delay(50);
        return 67;
    }

    private async Task<int> GetProductCombinationsCountAsync()
    {
        await Task.Delay(50);
        return 134;
    }

    private async Task<int> GetProductGroupingCountAsync()
    {
        await Task.Delay(50);
        return 15;
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
            // For now, return a placeholder count
            // TODO: Inject IPaymentTypeService and get real count
            await Task.Delay(50);
            return 6; // Number of seeded payment types
        }
        catch
        {
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
            // Get the UOM service from the service provider if available
            var serviceProvider = System.Windows.Application.Current?.Resources["ServiceProvider"] as IServiceProvider;
            if (serviceProvider != null)
            {
                var uomService = serviceProvider.GetService<IUomService>();
                if (uomService != null)
                {
                    var uoms = await uomService.GetAllAsync();
                    return uoms.Count();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting UOM count: {ex.Message}");
        }
        
        // Fallback to mock data
        await Task.Delay(50);
        return 18;
    }

    private async Task<int> GetShopCountAsync()
    {
        await Task.Delay(50);
        return 3;
    }

    private async Task<int> GetCustomerGroupsCountAsync()
    {
        await Task.Delay(50);
        return 7;
    }

    private async Task<int> GetDiscountsCountAsync()
    {
        await Task.Delay(50);
        return 31;
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
/// Information about an add options module
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