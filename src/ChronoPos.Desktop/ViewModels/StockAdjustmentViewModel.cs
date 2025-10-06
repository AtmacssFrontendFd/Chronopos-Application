using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Stock Adjustment page
/// </summary>
public partial class StockAdjustmentViewModel : ObservableObject
{
    #region Fields

    private readonly IStockAdjustmentService _stockAdjustmentService;
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of stock adjustments
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentDto> _stockAdjustments = new();

    /// <summary>
    /// Selected stock adjustment
    /// </summary>
    [ObservableProperty]
    private StockAdjustmentDto? _selectedAdjustment;

    /// <summary>
    /// Collection of adjustment reasons for dropdown
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentReasonDto> _adjustmentReasons = new();

    /// <summary>
    /// Collection of store locations for dropdown
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<StockAdjustmentSupportDto.LocationDto> _storeLocations = new();

    /// <summary>
    /// Current page number for pagination
    /// </summary>
    [ObservableProperty]
    private int _currentPage = 1;

    /// <summary>
    /// Total number of pages
    /// </summary>
    [ObservableProperty]
    private int _totalPages = 1;

    /// <summary>
    /// Total count of adjustments
    /// </summary>
    [ObservableProperty]
    private int _totalCount = 0;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [ObservableProperty]
    private int _pageSize = 20;

    /// <summary>
    /// Search term for filtering
    /// </summary>
    [ObservableProperty]
    private string _searchTerm = string.Empty;

    /// <summary>
    /// Selected status filter
    /// </summary>
    [ObservableProperty]
    private string _selectedStatus = "All";

    /// <summary>
    /// Selected store location filter
    /// </summary>
    [ObservableProperty]
    private int? _selectedStoreLocationId;

    /// <summary>
    /// From date filter
    /// </summary>
    [ObservableProperty]
    private DateTime? _fromDate;

    /// <summary>
    /// To date filter
    /// </summary>
    [ObservableProperty]
    private DateTime? _toDate;

    /// <summary>
    /// Is loading data
    /// </summary>
    [ObservableProperty]
    private bool _isLoading = false;

    /// <summary>
    /// Is creating/editing adjustment
    /// </summary>
    [ObservableProperty]
    private bool _isEditing = false;

    /// <summary>
    /// Current adjustment being created/edited
    /// </summary>
    [ObservableProperty]
    private CreateStockAdjustmentDto? _currentAdjustmentDto;

    /// <summary>
    /// Current theme name
    /// </summary>
    [ObservableProperty]
    private string _currentTheme = "Light";

    /// <summary>
    /// Current zoom level
    /// </summary>
    [ObservableProperty]
    private int _currentZoom = 100;

    /// <summary>
    /// Current language
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
    /// Current font family
    /// </summary>
    [ObservableProperty]
    private string _currentFontFamily = "Segoe UI";

    /// <summary>
    /// Current font size
    /// </summary>
    [ObservableProperty]
    private double _currentFontSize = 14;

    /// <summary>
    /// Status options for filtering
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _statusOptions = new() { "All", "Pending", "Completed" };

    #endregion

    #region Commands

    /// <summary>
    /// Command to load stock adjustments
    /// </summary>
    [RelayCommand]
    private async Task LoadAdjustmentsAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _stockAdjustmentService.GetStockAdjustmentsAsync(
                CurrentPage,
                PageSize,
                SelectedStatus == "All" ? null : SelectedStatus,
                SelectedStoreLocationId,
                null, // reasonId
                FromDate,
                ToDate);

            StockAdjustments.Clear();
            foreach (var adjustment in result.Items)
            {
                StockAdjustments.Add(adjustment);
            }

            TotalCount = result.TotalCount;
            TotalPages = result.TotalPages;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading adjustments: {ex.Message}");
            // TODO: Show error message to user
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to create new adjustment
    /// </summary>
    [RelayCommand]
    private void CreateNewAdjustment()
    {
        CurrentAdjustmentDto = new CreateStockAdjustmentDto
        {
            AdjustmentDate = DateTime.Now,
            Items = new List<CreateStockAdjustmentItemDto>()
        };
        IsEditing = true;
    }

    /// <summary>
    /// Command to edit selected adjustment
    /// </summary>
    [RelayCommand]
    private async Task EditAdjustmentAsync()
    {
        if (SelectedAdjustment == null || SelectedAdjustment.Status == "Completed")
            return;

        try
        {
            var fullAdjustment = await _stockAdjustmentService.GetStockAdjustmentByIdAsync(SelectedAdjustment.AdjustmentId);
            if (fullAdjustment != null)
            {
                CurrentAdjustmentDto = new CreateStockAdjustmentDto
                {
                    AdjustmentDate = fullAdjustment.AdjustmentDate,
                    StoreLocationId = fullAdjustment.StoreLocationId,
                    ReasonId = fullAdjustment.ReasonId,
                    Notes = fullAdjustment.Remarks, // Map Remarks to Notes
                    Items = fullAdjustment.Items.Select(i => new CreateStockAdjustmentItemDto
                    {
                        ProductId = i.ProductId,
                        UomId = 1, // Default UOM ID - you may need to adjust this
                        BatchNo = string.Empty, // Default empty batch - you may need to adjust this
                        QuantityBefore = i.QuantityBefore,
                        QuantityAfter = i.QuantityAfter,
                        ReasonLine = i.ReasonLine
                    }).ToList()
                };
                IsEditing = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading adjustment for edit: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to save adjustment
    /// </summary>
    [RelayCommand]
    private async Task SaveAdjustmentAsync()
    {
        if (CurrentAdjustmentDto == null)
            return;

        try
        {
            if (SelectedAdjustment == null)
            {
                // Create new adjustment
                var result = await _stockAdjustmentService.CreateStockAdjustmentAsync(CurrentAdjustmentDto);
                System.Diagnostics.Debug.WriteLine($"Created new adjustment with ID: {result.AdjustmentId}");
            }
            else
            {
                // Update existing adjustment
                var result = await _stockAdjustmentService.UpdateStockAdjustmentAsync(SelectedAdjustment.AdjustmentId, CurrentAdjustmentDto);
                System.Diagnostics.Debug.WriteLine($"Updated adjustment: {result.AdjustmentId}");
            }

            IsEditing = false;
            CurrentAdjustmentDto = null;
            SelectedAdjustment = null;
            await LoadAdjustmentsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving adjustment: {ex.Message}");
            // TODO: Show error message to user
        }
    }

    /// <summary>
    /// Command to cancel editing
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        CurrentAdjustmentDto = null;
        SelectedAdjustment = null;
    }

    /// <summary>
    /// Command to complete adjustment
    /// </summary>
    [RelayCommand]
    private async Task CompleteAdjustmentAsync()
    {
        if (SelectedAdjustment == null || SelectedAdjustment.Status == "Completed")
            return;

        try
        {
            var success = await _stockAdjustmentService.ApproveStockAdjustmentAsync(SelectedAdjustment.AdjustmentId);
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"Approved adjustment: {SelectedAdjustment.AdjustmentNo}");
                await LoadAdjustmentsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error completing adjustment: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to delete adjustment
    /// </summary>
    [RelayCommand]
    private async Task DeleteAdjustmentAsync()
    {
        if (SelectedAdjustment == null || SelectedAdjustment.Status == "Completed")
            return;

        try
        {
            var success = await _stockAdjustmentService.DeleteStockAdjustmentAsync(SelectedAdjustment.AdjustmentId);
            if (success)
            {
                System.Diagnostics.Debug.WriteLine($"Deleted adjustment: {SelectedAdjustment.AdjustmentNo}");
                await LoadAdjustmentsAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting adjustment: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to go to previous page
    /// </summary>
    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadAdjustmentsAsync();
        }
    }

    /// <summary>
    /// Command to go to next page
    /// </summary>
    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadAdjustmentsAsync();
        }
    }

    /// <summary>
    /// Command to apply filters
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1; // Reset to first page when applying filters
        await LoadAdjustmentsAsync();
    }

    /// <summary>
    /// Command to clear filters
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SelectedStatus = "All";
        SelectedStoreLocationId = null;
        FromDate = null;
        ToDate = null;
        SearchTerm = string.Empty;
        CurrentPage = 1;
        await LoadAdjustmentsAsync();
    }

    /// <summary>
    /// Command to go back to stock management
    /// </summary>
    public ICommand? GoBackCommand { get; set; }

    #endregion

    #region Constructor

    public StockAdjustmentViewModel(
        IStockAdjustmentService stockAdjustmentService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _stockAdjustmentService = stockAdjustmentService;
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
        _ = LoadInitialDataAsync();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize all settings with current values
    /// </summary>
    private void InitializeSettings()
    {
        CurrentTheme = _themeService.CurrentTheme.ToString();
        CurrentZoom = (int)_zoomService.CurrentZoomLevel;
        CurrentLanguage = _localizationService.CurrentLanguage.ToString() ?? "English";
        CurrentColorScheme = _colorSchemeService.CurrentPrimaryColor?.Name ?? "Blue";
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        CurrentFontFamily = "Segoe UI"; // Default font family
        CurrentFontSize = GetFontSizeValue(_fontService.CurrentFontSize);
    }

    /// <summary>
    /// Load initial data (reasons, locations, adjustments)
    /// </summary>
    private async Task LoadInitialDataAsync()
    {
        await Task.WhenAll(
            LoadAdjustmentReasonsAsync(),
            LoadStoreLocationsAsync(),
            LoadAdjustmentsAsync()
        );
    }

    /// <summary>
    /// Load adjustment reasons
    /// </summary>
    private async Task LoadAdjustmentReasonsAsync()
    {
        try
        {
            var reasons = await _stockAdjustmentService.GetAdjustmentReasonsAsync();
            AdjustmentReasons.Clear();
            foreach (var reason in reasons)
            {
                AdjustmentReasons.Add(reason);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading adjustment reasons: {ex.Message}");
        }
    }

    /// <summary>
    /// Load store locations
    /// </summary>
    private async Task LoadStoreLocationsAsync()
    {
        try
        {
            var locations = await _stockAdjustmentService.GetStoreLocationsAsync();
            StoreLocations.Clear();
            foreach (var location in locations)
            {
                StoreLocations.Add(location);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading store locations: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to convert FontSize enum to numeric value
    /// </summary>
    private static double GetFontSizeValue(FontSize fontSize)
    {
        return fontSize switch
        {
            FontSize.VerySmall => 10,
            FontSize.Small => 12,
            FontSize.Medium => 14,
            FontSize.Large => 16,
            _ => 14
        };
    }

    #endregion

    #region Event Handlers

    private void OnThemeChanged(Theme newTheme)
    {
        CurrentTheme = newTheme.ToString();
    }

    private void OnZoomChanged(ZoomLevel newZoom)
    {
        CurrentZoom = (int)newZoom;
    }

    private void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        CurrentLanguage = newLanguage.ToString();
    }

    private void OnPrimaryColorChanged(ColorOption newColor)
    {
        CurrentColorScheme = newColor.Name;
    }

    private void OnDirectionChanged(LayoutDirection newDirection)
    {
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private async void OnDatabaseLanguageChanged(object? sender, string newLanguageCode)
    {
        // Reload data with new language if needed
        await LoadInitialDataAsync();
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Cleanup event subscriptions
    /// </summary>
    ~StockAdjustmentViewModel()
    {
        // Unsubscribe from events
        if (_themeService != null)
            _themeService.ThemeChanged -= OnThemeChanged;
        if (_zoomService != null)
            _zoomService.ZoomChanged -= OnZoomChanged;
        if (_localizationService != null)
            _localizationService.LanguageChanged -= OnLanguageChanged;
        if (_colorSchemeService != null)
            _colorSchemeService.PrimaryColorChanged -= OnPrimaryColorChanged;
        if (_layoutDirectionService != null)
            _layoutDirectionService.DirectionChanged -= OnDirectionChanged;
        if (_databaseLocalizationService != null)
            _databaseLocalizationService.LanguageChanged -= OnDatabaseLanguageChanged;
    }

    #endregion
}
