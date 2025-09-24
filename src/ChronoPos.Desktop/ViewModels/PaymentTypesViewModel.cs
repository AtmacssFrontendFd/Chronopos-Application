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
/// ViewModel for managing payment types
/// </summary>
public partial class PaymentTypesViewModel : ObservableObject
{
    #region Private Fields

    private readonly IPaymentTypeService _paymentTypeService;
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
    /// Collection of payment types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PaymentTypeDto> _paymentTypes = new();

    /// <summary>
    /// Currently selected payment type
    /// </summary>
    [ObservableProperty]
    private PaymentTypeDto? _selectedPaymentType;

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
    /// Whether to show only active payment types
    /// </summary>
    [ObservableProperty]
    private bool _showActiveOnly = true;

    /// <summary>
    /// Search text for filtering payment types
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
    private PaymentTypeSidePanelViewModel _sidePanelViewModel;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Filtered collection of payment types based on search and active filter
    /// </summary>
    public ObservableCollection<PaymentTypeDto> FilteredPaymentTypes
    {
        get
        {
            var filtered = PaymentTypes.AsEnumerable();

            // Apply active filter
            if (ShowActiveOnly)
            {
                filtered = filtered.Where(p => p.Status);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(p => 
                    p.Name.ToLower().Contains(searchLower) ||
                    p.PaymentCode.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(p.NameAr) && p.NameAr.ToLower().Contains(searchLower)));
            }

            return new ObservableCollection<PaymentTypeDto>(filtered);
        }
    }

    /// <summary>
    /// Whether there are any payment types
    /// </summary>
    public bool HasPaymentTypes => PaymentTypes.Count > 0;

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
    public PaymentTypesViewModel(
        IPaymentTypeService paymentTypeService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        // Initialize side panel view model
        _sidePanelViewModel = new PaymentTypeSidePanelViewModel(paymentTypeService, layoutDirectionService);
        _sidePanelViewModel.OnSave += OnPaymentTypeSaved;
        _sidePanelViewModel.OnCancel += OnSidePanelCancelRequested;
        _sidePanelViewModel.OnClose += OnSidePanelCancelRequested;

        // Initialize with current values
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Load payment types
        _ = Task.Run(LoadPaymentTypesAsync);
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
    /// Command to refresh payment types
    /// </summary>
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadPaymentTypesAsync();
    }

    /// <summary>
    /// Command to add new payment type
    /// </summary>
    [RelayCommand]
    private void AddNew()
    {
        SidePanelViewModel.ResetForNew();
        IsSidePanelVisible = true;
    }

    /// <summary>
    /// Command to edit a payment type
    /// </summary>
    [RelayCommand]
    private void Edit(PaymentTypeDto paymentType)
    {
        if (paymentType != null)
        {
            SidePanelViewModel.LoadPaymentType(paymentType);
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Command to delete a payment type
    /// </summary>
    [RelayCommand]
    private async Task Delete(PaymentTypeDto paymentType)
    {
        if (paymentType != null)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the payment type '{paymentType.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Deleting payment type...";
                    
                    await _paymentTypeService.DeleteAsync(paymentType.Id, 1); // Assuming user ID 1 for now
                    
                    StatusMessage = "Payment type deleted successfully";
                    await LoadPaymentTypesAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting payment type: {ex.Message}";
                    MessageBox.Show($"Error deleting payment type: {ex.Message}", "Error", 
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
    /// Command to toggle active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
        OnPropertyChanged(nameof(FilteredPaymentTypes));
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
    /// Load all payment types from the service
    /// </summary>
    private async Task LoadPaymentTypesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading payment types...";
            
            var paymentTypes = await _paymentTypeService.GetAllAsync();
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                PaymentTypes.Clear();
                foreach (var paymentType in paymentTypes)
                {
                    PaymentTypes.Add(paymentType);
                }
            });
            
            // Force refresh of filtered collection
            OnPropertyChanged(nameof(FilteredPaymentTypes));
            OnPropertyChanged(nameof(HasPaymentTypes));
            
            StatusMessage = $"Loaded {paymentTypes.Count()} payment types";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading payment types: {ex.Message}";
            MessageBox.Show($"Error loading payment types: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle payment type saved event from side panel
    /// </summary>
    private async void OnPaymentTypeSaved()
    {
        IsSidePanelVisible = false;
        await LoadPaymentTypesAsync();
    }

    /// <summary>
    /// Handle cancel request from side panel
    /// </summary>
    private void OnSidePanelCancelRequested()
    {
        IsSidePanelVisible = false;
    }

    #endregion

    #region Property Changed Handlers

    /// <summary>
    /// Handle changes to ShowActiveOnly property
    /// </summary>
    partial void OnShowActiveOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredPaymentTypes));
    }

    /// <summary>
    /// Handle changes to SearchText property
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredPaymentTypes));
    }

    #endregion
}