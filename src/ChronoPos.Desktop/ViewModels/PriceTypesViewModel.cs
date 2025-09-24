using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
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
    /// Price type being edited/created
    /// </summary>
    [ObservableProperty]
    private SellingPriceTypeDto _currentPriceType = new();

    /// <summary>
    /// Whether we're in edit mode or create mode
    /// </summary>
    [ObservableProperty]
    private bool _isEditMode = false;

    /// <summary>
    /// Whether the side panel is open
    /// </summary>
    [ObservableProperty]
    private bool _isSidePanelOpen = false;

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
    private bool _showOnlyActive = true;

    /// <summary>
    /// Search text for filtering price types
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Text for the active filter button
    /// </summary>
    [ObservableProperty]
    private string _activeFilterButtonText = "Show All";

    /// <summary>
    /// Current flow direction for UI
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

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
            if (ShowOnlyActive)
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

    #endregion

    #region Property Changed Overrides

    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    partial void OnShowOnlyActiveChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    partial void OnPriceTypesChanged(ObservableCollection<SellingPriceTypeDto> value)
    {
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    #endregion

    #region Constructor

    public PriceTypesViewModel(
        ISellingPriceTypeService sellingPriceTypeService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _sellingPriceTypeService = sellingPriceTypeService ?? throw new ArgumentNullException(nameof(sellingPriceTypeService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

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
        CurrentPriceType = new SellingPriceTypeDto
        {
            TypeName = string.Empty,
            ArabicName = string.Empty,
            Description = string.Empty,
            Status = true
        };
        IsEditMode = false;
        IsSidePanelOpen = true;
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
            CurrentPriceType = new SellingPriceTypeDto
            {
                Id = priceType.Id,
                TypeName = priceType.TypeName,
                ArabicName = priceType.ArabicName,
                Description = priceType.Description,
                Status = priceType.Status
            };
            IsEditMode = true;
            IsSidePanelOpen = true;
        }
    }

    /// <summary>
    /// Command to save price type
    /// </summary>
    [RelayCommand]
    private async Task SavePriceTypeAsync()
    {
        try
        {
            StatusMessage = "Save button clicked...";
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(CurrentPriceType.TypeName))
            {
                StatusMessage = "Type Name is required";
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentPriceType.ArabicName))
            {
                StatusMessage = "Arabic Name is required";
                return;
            }

            IsLoading = true;
            StatusMessage = "Saving price type...";

            if (IsEditMode)
            {
                var updateDto = new UpdateSellingPriceTypeDto
                {
                    Id = CurrentPriceType.Id,
                    TypeName = CurrentPriceType.TypeName.Trim(),
                    ArabicName = CurrentPriceType.ArabicName.Trim(),
                    Description = CurrentPriceType.Description?.Trim(),
                    Status = CurrentPriceType.Status,
                    UpdatedBy = 1 // TODO: Get from current user context
                };

                await _sellingPriceTypeService.UpdateAsync(updateDto);
                StatusMessage = "Price type updated successfully";
            }
            else
            {
                var createDto = new CreateSellingPriceTypeDto
                {
                    TypeName = CurrentPriceType.TypeName.Trim(),
                    ArabicName = CurrentPriceType.ArabicName.Trim(),
                    Description = CurrentPriceType.Description?.Trim(),
                    Status = CurrentPriceType.Status,
                    CreatedBy = 1 // TODO: Get from current user context
                };

                await _sellingPriceTypeService.CreateAsync(createDto);
                StatusMessage = "Price type created successfully";
            }

            // Close the side panel and refresh the list
            IsEditMode = false;
            IsSidePanelOpen = false;
            CurrentPriceType = new SellingPriceTypeDto();
            await LoadPriceTypesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving price type: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
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
    /// Command to cancel editing
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsSidePanelOpen = false;
        CurrentPriceType = new SellingPriceTypeDto();
    }

    /// <summary>
    /// Command to refresh the price types list
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPriceTypesAsync();
    }

    /// <summary>
    /// Command to toggle active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowOnlyActive = !ShowOnlyActive;
        ActiveFilterButtonText = ShowOnlyActive ? "Show All" : "Active Only";
        StatusMessage = ShowOnlyActive ? "Showing only active price types" : "Showing all price types";
        OnPropertyChanged(nameof(FilteredPriceTypes));
    }

    /// <summary>
    /// Command to clear search
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        StatusMessage = "Search cleared";
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
    private void GoBack()
    {
        GoBackAction?.Invoke();
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

    #endregion
}