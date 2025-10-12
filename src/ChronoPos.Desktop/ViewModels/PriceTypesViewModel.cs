using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for managing selling price types
/// </summary>
public partial class PriceTypesViewModel : ObservableObject
{
    #region Private Fields

    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly ICurrentUserService _currentUserService;
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
    /// Collection of selling price types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SellingPriceTypeDto> _priceTypes = new();

    /// <summary>
    /// Currently selected price type
    /// </summary>
    [ObservableProperty]
    private SellingPriceTypeDto? _selectedPriceType;



    /// <summary>
    /// Whether the side panel is visible
    /// </summary>
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    /// <summary>
    /// Loading indicator
    /// </summary>
    [ObservableProperty]
    private bool _isLoading = false;

    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Whether to show only active price types
    /// </summary>
    [ObservableProperty]
    private bool _showActiveOnly = true;

    /// <summary>
    /// Search text for filtering price types
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Current flow direction for UI
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool canCreatePriceType = false;

    [ObservableProperty]
    private bool canEditPriceType = false;

    [ObservableProperty]
    private bool canDeletePriceType = false;

    /// <summary>
    /// Side panel view model
    /// </summary>
    [ObservableProperty]
    private PriceTypeSidePanelViewModel? _sidePanelViewModel;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Filtered collection of price types based on search text and active filter
    /// </summary>
    public IEnumerable<SellingPriceTypeDto> FilteredPriceTypes
    {
        get
        {
            var filtered = PriceTypes.AsEnumerable();

            // Apply active filter
            if (ShowActiveOnly)
            {
                filtered = filtered.Where(pt => pt.Status);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                filtered = filtered.Where(pt => 
                    (pt.TypeName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (pt.ArabicName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    (pt.Description?.ToLowerInvariant().Contains(searchLower) ?? false));
            }

            return filtered;
        }
    }

    /// <summary>
    /// Whether there are any price types
    /// </summary>
    public bool HasPriceTypes => PriceTypes.Any();

    /// <summary>
    /// Total number of price types
    /// </summary>
    public int TotalPriceTypes => PriceTypes.Count;

    #endregion

    #region Property Changed Overrides

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
        OnPropertyChanged(nameof(HasPriceTypes));
    }

    partial void OnPriceTypesChanged(ObservableCollection<SellingPriceTypeDto> value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
        OnPropertyChanged(nameof(HasPriceTypes));
        OnPropertyChanged(nameof(TotalPriceTypes));
    }

    #endregion

    #region Constructor

    public PriceTypesViewModel(
        ISellingPriceTypeService sellingPriceTypeService,
        ICurrentUserService currentUserService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _sellingPriceTypeService = sellingPriceTypeService ?? throw new ArgumentNullException(nameof(sellingPriceTypeService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        InitializePermissions();
        
        // Subscribe to layout direction changes
        _layoutDirectionService.DirectionChanged += OnDirectionChanged;
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Load initial data
        _ = LoadPriceTypesAsync();
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to add a new price type
    /// </summary>
    [RelayCommand]
    private void AddPriceType()
    {
        SidePanelViewModel = new PriceTypeSidePanelViewModel(
            _sellingPriceTypeService,
            OnSidePanelSaved,
            OnSidePanelCancelled);
        
        IsSidePanelVisible = true;
        StatusMessage = "Ready to add new price type";
    }

    /// <summary>
    /// Command to edit a price type
    /// </summary>
    [RelayCommand]
    private void EditPriceType(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            SidePanelViewModel = new PriceTypeSidePanelViewModel(
                _sellingPriceTypeService,
                priceType,
                OnSidePanelSaved,
                OnSidePanelCancelled);
            
            IsSidePanelVisible = true;
            StatusMessage = $"Editing '{priceType.TypeName}'";
        }
    }



    /// <summary>
    /// Command to delete a price type
    /// </summary>
    [RelayCommand]
    private async Task DeletePriceTypeAsync(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{priceType.TypeName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Deleting price type...";

                    await _sellingPriceTypeService.DeleteAsync(priceType.Id, 1); // TODO: Get user ID from context
                    StatusMessage = "Price type deleted successfully";
                    await LoadPriceTypesAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting price type: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }





    /// <summary>
    /// Command to filter active price types
    /// </summary>
    [RelayCommand]
    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        StatusMessage = ShowActiveOnly ? "Showing only active price types" : "Showing all price types";
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    /// <summary>
    /// Command to clear filters
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared";
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Action to navigate back
    /// </summary>
    public Action? GoBackAction { get; set; }

    /// <summary>
    /// Command for back navigation
    /// </summary>
    [RelayCommand]
    private void Back()
    {
        GoBackAction?.Invoke();
    }

    /// <summary>
    /// Command to refresh data
    /// </summary>
    [RelayCommand]
    private async Task RefreshDataAsync()
    {
        await LoadPriceTypesAsync();
    }

    /// <summary>
    /// Command to toggle active status
    /// </summary>
    [RelayCommand]
    private async Task ToggleActiveAsync(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            try
            {
                var updateDto = new UpdateSellingPriceTypeDto
                {
                    Id = priceType.Id,
                    TypeName = priceType.TypeName,
                    ArabicName = priceType.ArabicName,
                    Description = priceType.Description,
                    Status = !priceType.Status,
                    UpdatedBy = 1 // TODO: Get from current user context
                };

                await _sellingPriceTypeService.UpdateAsync(updateDto);
                await LoadPriceTypesAsync();
                StatusMessage = $"Price type {(updateDto.Status ? "activated" : "deactivated")} successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating price type status: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Command to view price type details
    /// </summary>
    [RelayCommand]
    private void ViewPriceTypeDetails(SellingPriceTypeDto? priceType)
    {
        if (priceType != null)
        {
            StatusMessage = $"Viewing details for '{priceType.TypeName}'";
            // TODO: Implement details view
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load price types from the service
    /// </summary>
    private async Task LoadPriceTypesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading price types...";

            // Always load all price types, filtering is handled by FilteredPriceTypes property
            var priceTypes = await _sellingPriceTypeService.GetAllAsync();

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                PriceTypes.Clear();
                foreach (var priceType in priceTypes)
                {
                    PriceTypes.Add(priceType);
                }
            });
            
            // Force refresh of filtered collection
            OnPropertyChanged(nameof(FilteredPriceTypes));

            StatusMessage = $"Loaded {PriceTypes.Count} price types";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading price types: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle layout direction changes
    /// </summary>
    private void OnDirectionChanged(LayoutDirection direction)
    {
        CurrentFlowDirection = direction == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Callback when side panel saves
    /// </summary>
    private async void OnSidePanelSaved(bool success)
    {
        if (success)
        {
            IsSidePanelVisible = false;
            SidePanelViewModel = null;
            await LoadPriceTypesAsync();
            StatusMessage = "Price type saved successfully";
        }
    }

    /// <summary>
    /// Callback when side panel is cancelled
    /// </summary>
    private void OnSidePanelCancelled()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Operation cancelled";
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreatePriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.CREATE);
            CanEditPriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.UPDATE);
            CanDeletePriceType = _currentUserService.HasPermission(ScreenNames.PRICE_TYPES, TypeMatrix.DELETE);
        }
        catch (Exception)
        {
            CanCreatePriceType = false;
            CanEditPriceType = false;
            CanDeletePriceType = false;
        }
    }

    #endregion
}