using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using ChronoPos.Desktop.Services;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for comprehensive UOM management with full settings integration
/// </summary>
public partial class UomViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IUomService _uomService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action? _navigateToAddUom;
    private readonly Action<UnitOfMeasurementDto>? _navigateToEditUom;
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
    private ObservableCollection<UnitOfMeasurementDto> uoms = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> filteredUoms = new();

    [ObservableProperty]
    private UnitOfMeasurementDto? selectedUom;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedSearchType = "Name";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready";

    [ObservableProperty]
    private bool isUomFormVisible = false;

    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private UnitOfMeasurementDto currentUom = new();

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
    private string _pageTitle = "Unit of Measurement Management";

    [ObservableProperty]
    private string _backButtonText = "Back";

    [ObservableProperty]
    private string _refreshButtonText = "Refresh";

    [ObservableProperty]
    private string _addNewUomButtonText = "Add UOM";

    [ObservableProperty]
    private string _searchPlaceholder = "Search units of measurement...";

    [ObservableProperty]
    private string _searchTypeName = "Name";

    [ObservableProperty]
    private string _searchTypeSymbol = "Symbol";

    [ObservableProperty]
    private string _showingUomsFormat = "Showing {0} units";

    [ObservableProperty]
    private string _itemsCountText = "units";

    // Permission Properties
    [ObservableProperty]
    private bool canCreateUom = false;

    [ObservableProperty]
    private bool canEditUom = false;

    [ObservableProperty]
    private bool canDeleteUom = false;

    // Table Column Headers
    [ObservableProperty]
    private string _columnName = "Name";

    [ObservableProperty]
    private string _columnSymbol = "Symbol";

    [ObservableProperty]
    private string _columnType = "Type";

    [ObservableProperty]
    private string _columnCategory = "Category";

    [ObservableProperty]
    private string _columnBaseUom = "Base UOM";

    [ObservableProperty]
    private string _columnConversion = "Conversion";

    [ObservableProperty]
    private string _columnCreated = "Created";

    [ObservableProperty]
    private string _columnStatus = "Status";

    [ObservableProperty]
    private string _columnActions = "Actions";

    // Action Tooltips
    [ObservableProperty]
    private string _editUomTooltip = "Edit UOM";

    [ObservableProperty]
    private string _deleteUomTooltip = "Delete UOM";

    [ObservableProperty]
    private string _activateUomTooltip = "Activate UOM";

    [ObservableProperty]
    private string _deactivateUomTooltip = "Deactivate UOM";

    [ObservableProperty]
    private string _viewConversionsTooltip = "View Conversions";

    // Computed Properties
    public bool HasUoms => FilteredUoms?.Count > 0;
    
    public int TotalUoms => Uoms?.Count ?? 0;

    // Sidepanel Properties
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private UomSidePanelViewModel? _sidePanelViewModel;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services
    /// </summary>
    public UomViewModel(
        IUomService uomService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        ICurrentUserService currentUserService,
        Action? navigateToAddUom = null,
        Action<UnitOfMeasurementDto>? navigateToEditUom = null,
        Action? navigateBack = null)
    {
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        
        _navigateToAddUom = navigateToAddUom;
        _navigateToEditUom = navigateToEditUom;
        _navigateBack = navigateBack;

        // Initialize permissions
        InitializePermissions();

        // Initialize current settings
        InitializeCurrentSettings();
        
        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;
        
        // Load data
        _ = Task.Run(LoadUomsAsync);
        
        // Load translations
        _ = Task.Run(LoadTranslationsAsync);
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshData()
    {
        IsLoading = true;
        StatusMessage = "Refreshing UOM data...";
        
        try
        {
            await LoadUomsAsync();
            StatusMessage = "UOM data refreshed successfully";
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
    private void AddUom()
    {
        ShowSidePanelForNewUom();
    }

    [RelayCommand]
    private async Task EditUom(UnitOfMeasurementDto? uom)
    {
        if (uom != null)
        {
            await ShowSidePanelForEditUom(uom);
        }
    }

    [RelayCommand]
    private async Task DeleteUom(UnitOfMeasurementDto? uom)
    {
        if (uom == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the unit '{uom.Name}'?\n\nWarning: This may affect products that use this unit.",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Deleting UOM...";
                
                await _uomService.DeleteAsync(uom.Id);
                await LoadUomsAsync();
                
                StatusMessage = "UOM deleted successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting UOM: {ex.Message}";
                MessageBox.Show($"Error deleting UOM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task ToggleUomStatus(UnitOfMeasurementDto? uom)
    {
        if (uom == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = uom.IsActive ? "Deactivating UOM..." : "Activating UOM...";
            
            // Get the full UOM before updating
            var fullUom = await _uomService.GetByIdAsync(uom.Id);
            if (fullUom == null)
            {
                StatusMessage = "Error: UOM not found";
                return;
            }
            
            // Update the status
            fullUom.IsActive = !fullUom.IsActive;
            
            await _uomService.UpdateAsync(fullUom.Id, new UpdateUomDto
            {
                Name = fullUom.Name,
                Abbreviation = fullUom.Abbreviation,
                Type = fullUom.Type,
                CategoryTitle = fullUom.CategoryTitle,
                BaseUomId = fullUom.BaseUomId,
                ConversionFactor = fullUom.ConversionFactor,
                IsActive = fullUom.IsActive
            });
            
            // Update the local UOM object for UI refresh
            uom.IsActive = fullUom.IsActive;
            
            await LoadUomsAsync();
            
            StatusMessage = fullUom.IsActive ? "UOM activated successfully" : "UOM deactivated successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating UOM status: {ex.Message}";
            MessageBox.Show($"Error updating UOM status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void FilterBaseUnits()
    {
        var baseUnits = Uoms.Where(u => u.Type == "Base");
        FilteredUoms.Clear();
        foreach (var uom in baseUnits)
        {
            FilteredUoms.Add(uom);
        }
        StatusMessage = $"Showing {FilteredUoms.Count} base units";
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        FilterUoms();
        StatusMessage = $"Showing all {FilteredUoms.Count} units";
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
    private void ToggleActive(UnitOfMeasurementDto? uom)
    {
        _ = Task.Run(() => ToggleUomStatus(uom));
    }

    [RelayCommand]
    private void ViewConversions(UnitOfMeasurementDto? uom)
    {
        if (uom == null) return;

        try
        {
            // Show conversion information
            var message = $"Unit: {uom.Name} ({uom.Abbreviation})\n" +
                         $"Type: {uom.Type}\n" +
                         $"Category: {uom.CategoryTitle}\n";

            if (uom.Type == "Derived" && !string.IsNullOrEmpty(uom.BaseUomName))
            {
                message += $"Base Unit: {uom.BaseUomName}\n" +
                          $"Conversion Factor: {uom.ConversionFactor?.ToString("F4") ?? "N/A"}";
            }
            else
            {
                message += "This is a base unit - no conversion needed.";
            }

            MessageBox.Show(message, $"Conversion Info - {uom.Name}", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error viewing conversions: {ex.Message}";
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
    /// Load all UOMs from the service
    /// </summary>
    private async Task LoadUomsAsync()
    {
        try
        {
            IsLoading = true;
            var uomList = await _uomService.GetAllAsync();
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Uoms.Clear();
                foreach (var uom in uomList)
                {
                    Uoms.Add(uom);
                }
                
                FilterUoms();
                OnPropertyChanged(nameof(TotalUoms));
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading UOMs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Filter UOMs based on search text and type
    /// </summary>
    private void FilterUoms()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredUoms.Clear();
            foreach (var uom in Uoms)
            {
                FilteredUoms.Add(uom);
            }
        }
        else
        {
            var filtered = SelectedSearchType switch
            {
                "Name" => Uoms.Where(u => u.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)),
                "Symbol" => Uoms.Where(u => u.Abbreviation?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true),
                _ => Uoms.Where(u => u.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            };

            FilteredUoms.Clear();
            foreach (var uom in filtered)
            {
                FilteredUoms.Add(uom);
            }
        }
        
        // Notify computed properties
        OnPropertyChanged(nameof(HasUoms));
        OnPropertyChanged(nameof(TotalUoms));
    }

    /// <summary>
    /// Show sidepanel for creating a new UOM
    /// </summary>
    private void ShowSidePanelForNewUom()
    {
        try
        {
            SidePanelViewModel?.Dispose();
            
            // Create the sidepanel ViewModel
            SidePanelViewModel = new UomSidePanelViewModel(
                _uomService,
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
            StatusMessage = $"Error opening UOM form: {ex.Message}";
        }
    }

    /// <summary>
    /// Show sidepanel for editing an existing UOM
    /// </summary>
    private async Task ShowSidePanelForEditUom(UnitOfMeasurementDto uom)
    {
        try
        {
            SidePanelViewModel?.Dispose();
            
            // Get the full UOM data
            var fullUomData = await _uomService.GetByIdAsync(uom.Id);
            if (fullUomData == null)
            {
                MessageBox.Show($"Error: Could not load UOM data for ID {uom.Id}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Create the sidepanel ViewModel for editing
            SidePanelViewModel = new UomSidePanelViewModel(
                _uomService,
                _themeService,
                _localizationService,
                _layoutDirectionService,
                _databaseLocalizationService,
                fullUomData,
                onSaved: OnSidePanelSaved,
                onCancelled: OnSidePanelCancelled
            );
            
            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening UOM form: {ex.Message}";
        }
    }

    /// <summary>
    /// Handle sidepanel saved event
    /// </summary>
    private async void OnSidePanelSaved(bool success)
    {
        if (success)
        {
            await LoadUomsAsync();
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
            PageTitle = await _databaseLocalizationService.GetTranslationAsync("uom.page_title") ?? "Unit of Measurement Management";
            BackButtonText = await _databaseLocalizationService.GetTranslationAsync("common.back") ?? "Back";
            RefreshButtonText = await _databaseLocalizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            AddNewUomButtonText = await _databaseLocalizationService.GetTranslationAsync("uom.add_new") ?? "Add UOM";
            SearchPlaceholder = await _databaseLocalizationService.GetTranslationAsync("uom.search_placeholder") ?? "Search units of measurement...";
            
            // Column headers
            ColumnName = await _databaseLocalizationService.GetTranslationAsync("uom.column.name") ?? "Name";
            ColumnSymbol = await _databaseLocalizationService.GetTranslationAsync("uom.column.symbol") ?? "Symbol";
            ColumnType = await _databaseLocalizationService.GetTranslationAsync("uom.column.type") ?? "Type";
            ColumnCategory = await _databaseLocalizationService.GetTranslationAsync("uom.column.category") ?? "Category";
            ColumnBaseUom = await _databaseLocalizationService.GetTranslationAsync("uom.column.base_uom") ?? "Base UOM";
            ColumnConversion = await _databaseLocalizationService.GetTranslationAsync("uom.column.conversion") ?? "Conversion";
            ColumnCreated = await _databaseLocalizationService.GetTranslationAsync("uom.column.created") ?? "Created";
            ColumnStatus = await _databaseLocalizationService.GetTranslationAsync("uom.column.status") ?? "Status";
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
            FilterUoms();
        }
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateUom = _currentUserService.HasPermission(ScreenNames.UOM, TypeMatrix.CREATE);
            CanEditUom = _currentUserService.HasPermission(ScreenNames.UOM, TypeMatrix.UPDATE);
            CanDeleteUom = _currentUserService.HasPermission(ScreenNames.UOM, TypeMatrix.DELETE);
        }
        catch (Exception)
        {
            // Fail-secure: all permissions default to false
            CanCreateUom = false;
            CanEditUom = false;
            CanDeleteUom = false;
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