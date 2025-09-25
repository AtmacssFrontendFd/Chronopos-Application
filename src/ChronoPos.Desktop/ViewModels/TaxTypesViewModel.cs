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
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for managing tax types
/// </summary>
public partial class TaxTypesViewModel : ObservableObject
{
    #region Private Fields

    private readonly ITaxTypeService _taxTypeService;
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
    /// Collection of tax types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> _taxTypes = new();

    /// <summary>
    /// Currently selected tax type
    /// </summary>
    [ObservableProperty]
    private TaxTypeDto? _selectedTaxType;

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
    /// Whether to show only active tax types
    /// </summary>
    [ObservableProperty]
    private bool _showActiveOnly = true;

    /// <summary>
    /// Search text for filtering tax types
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Whether the side panel is visible
    /// </summary>
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    /// <summary>
    /// Current flow direction for UI
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    /// <summary>
    /// Side panel view model
    /// </summary>
    [ObservableProperty]
    private TaxTypeSidePanelViewModel _sidePanelViewModel;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Filtered collection of tax types based on search and active filter
    /// </summary>
    public ObservableCollection<TaxTypeDto> FilteredTaxTypes
    {
        get
        {
            var filtered = TaxTypes.AsEnumerable();

            // Apply active filter
            if (ShowActiveOnly)
            {
                filtered = filtered.Where(t => t.IsActive);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(t => 
                    t.Name.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(t.Description) && t.Description.ToLower().Contains(searchLower)));
            }

            return new ObservableCollection<TaxTypeDto>(filtered);
        }
    }

    /// <summary>
    /// Whether there are any tax types
    /// </summary>
    public bool HasTaxTypes => TaxTypes.Count > 0;

    #endregion

    #region Commands

    /// <summary>
    /// Back navigation action (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services
    /// </summary>
    public TaxTypesViewModel(
        ITaxTypeService taxTypeService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _taxTypeService = taxTypeService ?? throw new ArgumentNullException(nameof(taxTypeService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        // Initialize side panel view model
        _sidePanelViewModel = new TaxTypeSidePanelViewModel(taxTypeService, layoutDirectionService);
        _sidePanelViewModel.OnSave += OnTaxTypeSaved;
        _sidePanelViewModel.OnCancel += OnSidePanelCancelRequested;
        _sidePanelViewModel.OnClose += OnSidePanelCancelRequested;

        // Initialize with current values
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Load tax types
        _ = Task.Run(LoadTaxTypesAsync);
    }

    #endregion

    #region Command Implementations

    /// <summary>
    /// Command to go back to previous screen
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    /// <summary>
    /// Command to refresh tax types
    /// </summary>
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadTaxTypesAsync();
    }

    /// <summary>
    /// Command to add new tax type
    /// </summary>
    [RelayCommand]
    private void AddNew()
    {
        SidePanelViewModel.ResetForNew();
        IsSidePanelVisible = true;
    }

    /// <summary>
    /// Command to edit a tax type
    /// </summary>
    [RelayCommand]
    private void Edit(TaxTypeDto taxType)
    {
        if (taxType != null)
        {
            SidePanelViewModel.LoadTaxType(taxType);
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Command to delete a tax type
    /// </summary>
    [RelayCommand]
    private async Task Delete(TaxTypeDto taxType)
    {
        if (taxType != null)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the tax type '{taxType.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Deleting tax type...";
                    
                    await _taxTypeService.DeleteTaxTypeAsync(taxType.Id);
                    
                    StatusMessage = "Tax type deleted successfully";
                    await LoadTaxTypesAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting tax type: {ex.Message}";
                    MessageBox.Show($"Error deleting tax type: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }

    /// <summary>
    /// Command to toggle active status of a tax type
    /// </summary>
    [RelayCommand]
    private async Task ToggleActive(object parameter)
    {
        if (parameter is not TaxTypeDto taxType) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Updating tax type status...";

            // Toggle the active status
            taxType.IsActive = !taxType.IsActive;
            
            // Update the tax type
            await _taxTypeService.UpdateTaxTypeAsync(taxType);
            
            StatusMessage = "Tax type status updated successfully";
            OnPropertyChanged(nameof(FilteredTaxTypes));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating tax type status: {ex.Message}";
            MessageBox.Show($"Error updating tax type status: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to toggle active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
        OnPropertyChanged(nameof(FilteredTaxTypes));
    }

    /// <summary>
    /// Command to clear search
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load all tax types from the service
    /// </summary>
    private async Task LoadTaxTypesAsync()
    {
        try
        {
            var taxTypes = await _taxTypeService.GetAllTaxTypesAsync();
            TaxTypes = new ObservableCollection<TaxTypeDto>(taxTypes);
            FilterTaxTypes();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tax types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Filter tax types based on search and active status
    /// </summary>
    private void FilterTaxTypes()
    {
        // Since FilteredTaxTypes is a computed property, just notify that it changed
        OnPropertyChanged(nameof(FilteredTaxTypes));
    }

    /// <summary>
    /// Handle tax type saved event from side panel
    /// </summary>
    private async void OnTaxTypeSaved()
    {
        IsSidePanelVisible = false;
        await LoadTaxTypesAsync();
    }

    /// <summary>
    /// Handle side panel cancel event
    /// </summary>
    private void OnSidePanelCancelRequested()
    {
        IsSidePanelVisible = false;
    }

    #endregion

    #region Event Handlers

    partial void OnSearchTextChanged(string value)
    {
        FilterTaxTypes();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterTaxTypes();
    }

    #endregion
}