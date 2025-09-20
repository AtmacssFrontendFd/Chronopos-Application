using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for comprehensive discount management with full settings integration
/// </summary>
public partial class DiscountViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IDiscountService _discountService;
    private readonly IProductService _productService;
    private readonly object? _categoryService; // ICategoryService when available
    private readonly object? _customerService; // ICustomerService when available
    private readonly object? _storeService; // IStoreService when available
    private readonly Action? _navigateToAddDiscount;
    private readonly Action<DiscountDto>? _navigateToEditDiscount;
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
    private ObservableCollection<DiscountDto> discounts = new();

    [ObservableProperty]
    private ObservableCollection<DiscountDto> filteredDiscounts = new();

    [ObservableProperty]
    private DiscountDto? selectedDiscount;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedSearchType = "Discount Name";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool isDiscountFormVisible = false;

    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private DiscountDto currentDiscount = new();

    // Settings properties
    [ObservableProperty]
    private string _currentTheme = "Light";

    [ObservableProperty]
    private int _currentZoom = 100;

    [ObservableProperty]
    private string _currentLanguage = "English";

    [ObservableProperty]
    private double _currentFontSize = 14;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    // Translated UI Properties
    [ObservableProperty]
    private string _pageTitle = "Discount Management";

    [ObservableProperty]
    private string _backButtonText = "Back";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    [ObservableProperty]
    private string _addNewDiscountButtonText = "Add Discount";

    [ObservableProperty]
    private string _searchPlaceholder = "Search discounts...";

    [ObservableProperty]
    private string _searchTypeDiscountName = "Discount Name";

    [ObservableProperty]
    private string _searchTypeDiscountCode = "Discount Code";

    [ObservableProperty]
    private string _showingDiscountsFormat = "Showing {0} discounts";

    [ObservableProperty]
    private string _itemsCountText = "discounts";

    // Table Column Headers
    [ObservableProperty]
    private string _columnDiscountName = "Discount Name";

    [ObservableProperty]
    private string _columnDiscountCode = "Discount Code";

    [ObservableProperty]
    private string _columnDiscountType = "Type";

    [ObservableProperty]
    private string _columnDiscountValue = "Value";

    [ObservableProperty]
    private string _columnStartDate = "Start Date";

    [ObservableProperty]
    private string _columnEndDate = "End Date";

    [ObservableProperty]
    private string _columnStatus = "Status";

    [ObservableProperty]
    private string _columnActions = "Actions";

    // Action Tooltips
    [ObservableProperty]
    private string _editDiscountTooltip = "Edit Discount";

    [ObservableProperty]
    private string _deleteDiscountTooltip = "Delete Discount";

    [ObservableProperty]
    private string _activateDiscountTooltip = "Activate Discount";

    [ObservableProperty]
    private string _deactivateDiscountTooltip = "Deactivate Discount";

    // Computed Properties
    public bool HasDiscounts => FilteredDiscounts?.Count > 0;
    
    public int TotalDiscounts => Discounts?.Count ?? 0;

    // Sidepanel Properties
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private DiscountSidePanelViewModel? _sidePanelViewModel;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services
    /// </summary>
    public DiscountViewModel(
        IDiscountService discountService,
        IProductService productService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action? navigateToAddDiscount = null,
        Action<DiscountDto>? navigateToEditDiscount = null,
        Action? navigateBack = null)
    {
        _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        
        _navigateToAddDiscount = navigateToAddDiscount;
        _navigateToEditDiscount = navigateToEditDiscount;
        _navigateBack = navigateBack;

        // Initialize current settings
        InitializeCurrentSettings();
        
        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
        
        // Load data
        _ = Task.Run(LoadDiscountsAsync);
        
        // Load translations
        _ = Task.Run(LoadTranslationsAsync);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshData()
    {
        IsLoading = true;
        StatusMessage = "Refreshing discount data...";
        
        try
        {
            await LoadDiscountsAsync();
            StatusMessage = "Discount data refreshed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error refreshing data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NavigateBack()
    {
        _navigateBack?.Invoke();
    }

    [RelayCommand]
    private void AddDiscount()
    {
        ShowSidePanelForNewDiscount();
    }

    [RelayCommand]
    private async void EditDiscount(DiscountDto? discount)
    {
        if (discount != null)
        {
            await ShowSidePanelForEditDiscount(discount);
        }
    }

    [RelayCommand]
    private async Task DeleteDiscount(DiscountDto? discount)
    {
        if (discount == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the discount '{discount.DiscountName}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Deleting discount...";
                
                await _discountService.DeleteAsync(discount.Id);
                await LoadDiscountsAsync();
                
                StatusMessage = "Discount deleted successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting discount: {ex.Message}";
                MessageBox.Show($"Error deleting discount: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task ToggleDiscountStatus(DiscountDto? discount)
    {
        if (discount == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = discount.IsActive ? "Deactivating discount..." : "Activating discount...";
            
            // Get the full discount with selections before updating
            var fullDiscount = await _discountService.GetByIdAsync(discount.Id);
            if (fullDiscount == null)
            {
                StatusMessage = "Error: Discount not found";
                return;
            }
            
            // Update the status
            fullDiscount.IsActive = !fullDiscount.IsActive;
            
            await _discountService.UpdateAsync(fullDiscount.Id, new UpdateDiscountDto
            {
                DiscountName = fullDiscount.DiscountName,
                DiscountNameAr = fullDiscount.DiscountNameAr,
                DiscountDescription = fullDiscount.DiscountDescription,
                DiscountCode = fullDiscount.DiscountCode,
                DiscountType = fullDiscount.DiscountType,
                DiscountValue = fullDiscount.DiscountValue,
                MaxDiscountAmount = fullDiscount.MaxDiscountAmount,
                MinPurchaseAmount = fullDiscount.MinPurchaseAmount,
                StartDate = fullDiscount.StartDate,
                EndDate = fullDiscount.EndDate,
                ApplicableOn = fullDiscount.ApplicableOn,
                SelectedProductIds = fullDiscount.SelectedProductIds ?? new List<int>(),
                SelectedCategoryIds = fullDiscount.SelectedCategoryIds ?? new List<int>(),
                Priority = fullDiscount.Priority,
                IsStackable = fullDiscount.IsStackable,
                IsActive = fullDiscount.IsActive,
                StoreId = fullDiscount.StoreId,
                CurrencyCode = fullDiscount.CurrencyCode
            });
            
            // Update the local discount object for UI refresh
            discount.IsActive = fullDiscount.IsActive;
            
            await LoadDiscountsAsync();
            
            StatusMessage = fullDiscount.IsActive ? "Discount activated successfully" : "Discount deactivated successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating discount status: {ex.Message}";
            MessageBox.Show($"Error updating discount status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void FilterActive()
    {
        var activeDiscounts = Discounts.Where(d => d.IsActive);
        FilteredDiscounts.Clear();
        foreach (var discount in activeDiscounts)
        {
            FilteredDiscounts.Add(discount);
        }
        StatusMessage = $"Showing {FilteredDiscounts.Count} active discounts";
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterDiscounts();
        StatusMessage = $"Showing all {FilteredDiscounts.Count} discounts";
    }

    [RelayCommand]
    private void Refresh()
    {
        _ = Task.Run(RefreshData);
    }

    [RelayCommand]
    private void Back()
    {
        NavigateBack();
    }

    [RelayCommand]
    private void ToggleActive(DiscountDto? discount)
    {
        _ = Task.Run(() => ToggleDiscountStatus(discount));
    }

    [RelayCommand]
    private void PrintCoupon(DiscountDto? discount)
    {
        if (discount == null) return;

        try
        {
            // Create coupon content
            var couponContent = $@"
╔══════════════════════════════════════╗
║              DISCOUNT COUPON         ║
╠══════════════════════════════════════╣
║                                      ║
║  {discount.DiscountName,-34}  ║
║                                      ║
║  Code: {discount.DiscountCode,-27}  ║
║                                      ║
║  Value: {discount.FormattedDiscountValue,-26}  ║
║                                      ║
║  {discount.DiscountDescription,-34}  ║
║                                      ║
║  Valid: {discount.StartDate:MM/dd/yyyy} - {discount.EndDate:MM/dd/yyyy}   ║
║                                      ║
║  Terms and conditions apply          ║
║                                      ║
╚══════════════════════════════════════╝
";

            // Show print dialog with coupon content
            var printDialog = new System.Windows.Controls.PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var document = new System.Windows.Documents.FlowDocument();
                var paragraph = new System.Windows.Documents.Paragraph();
                paragraph.Inlines.Add(new System.Windows.Documents.Run(couponContent)
                {
                    FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                    FontSize = 12
                });
                document.Blocks.Add(paragraph);
                
                var paginator = ((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"Discount Coupon - {discount.DiscountCode}");
                
                StatusMessage = $"Coupon printed for {discount.DiscountName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error printing coupon: {ex.Message}";
            MessageBox.Show($"Error printing coupon: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize current settings from services
    /// </summary>
    private void InitializeCurrentSettings()
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = (int)_zoomService.CurrentZoomPercentage;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString();
        CurrentFontSize = GetCurrentFontSize();
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Get current font size based on zoom and font settings
    /// </summary>
    private double GetCurrentFontSize()
    {
        var baseSize = _fontService.CurrentFontSize switch
        {
            FontSize.Small => 12,
            FontSize.Medium => 14,
            FontSize.Large => 16,
            _ => 14
        };
        
        return baseSize * (_zoomService.CurrentZoomPercentage / 100.0);
    }

    /// <summary>
    /// Load all discounts from the service
    /// </summary>
    private async Task LoadDiscountsAsync()
    {
        try
        {
            IsLoading = true;
            var discountList = await _discountService.GetAllAsync();
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Discounts.Clear();
                foreach (var discount in discountList)
                {
                    Discounts.Add(discount);
                }
                
                FilterDiscounts();
                OnPropertyChanged(nameof(TotalDiscounts));
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading discounts: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Filter discounts based on search text and type
    /// </summary>
    private void FilterDiscounts()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredDiscounts.Clear();
            foreach (var discount in Discounts)
            {
                FilteredDiscounts.Add(discount);
            }
        }
        else
        {
            var filtered = SelectedSearchType switch
            {
                "Discount Name" => Discounts.Where(d => d.DiscountName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),
                "Discount Code" => Discounts.Where(d => d.DiscountCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),
                _ => Discounts.Where(d => d.DiscountName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            };

            FilteredDiscounts.Clear();
            foreach (var discount in filtered)
            {
                FilteredDiscounts.Add(discount);
            }
        }
        
        // Notify computed properties
        OnPropertyChanged(nameof(HasDiscounts));
        OnPropertyChanged(nameof(TotalDiscounts));
    }

    /// <summary>
    /// Show sidepanel for creating a new discount
    /// </summary>
    private void ShowSidePanelForNewDiscount()
    {
        try
        {
            SidePanelViewModel?.Dispose();
            
            // Create the sidepanel ViewModel
            SidePanelViewModel = new DiscountSidePanelViewModel(
                _discountService,
                _productService,
                _categoryService,
                _customerService,
                _storeService,
                _themeService,
                _localizationService,
                _layoutDirectionService,
                _databaseLocalizationService,
                onSaved: OnSidePanelSaved,
                onCancelled: OnSidePanelCancelled
            );
            
            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening discount form: {ex.Message}";
        }
    }

    /// <summary>
    /// Show sidepanel for editing an existing discount
    /// </summary>
    private async Task ShowSidePanelForEditDiscount(DiscountDto discount)
    {
        try
        {
            SidePanelViewModel?.Dispose();
            
            // Get the full discount data with product/category selections
            var fullDiscountData = await _discountService.GetByIdAsync(discount.Id);
            if (fullDiscountData == null)
            {
                MessageBox.Show($"Error: Could not load discount data for ID {discount.Id}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Create the sidepanel ViewModel for editing
            SidePanelViewModel = new DiscountSidePanelViewModel(
                _discountService,
                _productService,
                _categoryService,
                _customerService,
                _storeService,
                _themeService,
                _localizationService,
                _layoutDirectionService,
                _databaseLocalizationService,
                fullDiscountData,
                onSaved: OnSidePanelSaved,
                onCancelled: OnSidePanelCancelled
            );
            
            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening discount form: {ex.Message}";
        }
    }

    /// <summary>
    /// Handle sidepanel saved event
    /// </summary>
    private async void OnSidePanelSaved(bool success)
    {
        if (success)
        {
            await LoadDiscountsAsync();
        }
        
        IsSidePanelVisible = false;
        SidePanelViewModel?.Dispose();
        SidePanelViewModel = null;
    }

    /// <summary>
    /// Handle sidepanel cancelled event
    /// </summary>
    private void OnSidePanelCancelled()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel?.Dispose();
        SidePanelViewModel = null;
    }

    /// <summary>
    /// Load translations from database
    /// </summary>
    private async Task LoadTranslationsAsync()
    {
        try
        {
            PageTitle = await _databaseLocalizationService.GetTranslationAsync("discount.page_title") ?? "Discount Management";
            BackButtonText = await _databaseLocalizationService.GetTranslationAsync("common.back") ?? "Back";
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            AddNewDiscountButtonText = await _databaseLocalizationService.GetTranslationAsync("discount.add_new") ?? "Add Discount";
            SearchPlaceholder = await _databaseLocalizationService.GetTranslationAsync("discount.search_placeholder") ?? "Search discounts...";
            
            // Column headers
            ColumnDiscountName = await _databaseLocalizationService.GetTranslationAsync("discount.column.name") ?? "Discount Name";
            ColumnDiscountCode = await _databaseLocalizationService.GetTranslationAsync("discount.column.code") ?? "Discount Code";
            ColumnDiscountType = await _databaseLocalizationService.GetTranslationAsync("discount.column.type") ?? "Type";
            ColumnDiscountValue = await _databaseLocalizationService.GetTranslationAsync("discount.column.value") ?? "Value";
            ColumnStartDate = await _databaseLocalizationService.GetTranslationAsync("discount.column.start_date") ?? "Start Date";
            ColumnEndDate = await _databaseLocalizationService.GetTranslationAsync("discount.column.end_date") ?? "End Date";
            ColumnStatus = await _databaseLocalizationService.GetTranslationAsync("discount.column.status") ?? "Status";
            ColumnActions = await _databaseLocalizationService.GetTranslationAsync("common.actions") ?? "Actions";
        }
        catch (Exception ex)
        {
            // Log error but don't throw - use default English text
            System.Diagnostics.Debug.WriteLine($"Error loading translations: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle property changes for filtering
    /// </summary>
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedSearchType))
        {
            FilterDiscounts();
        }
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