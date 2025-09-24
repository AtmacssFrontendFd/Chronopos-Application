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
    /// Payment type being edited/created
    /// </summary>
    [ObservableProperty]
    private PaymentTypeDto _currentPaymentType = new();

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
    /// Whether to show only active payment types
    /// </summary>
    [ObservableProperty]
    private bool _showOnlyActive = true;

    /// <summary>
    /// Search text for filtering payment types
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

    /// <summary>
    /// Form title (Add/Edit Payment Type)
    /// </summary>
    [ObservableProperty]
    private string _formTitle = "Add Payment Type";

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
            if (ShowOnlyActive)
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

        // Initialize with current values
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Initialize ActiveFilterButtonText
        UpdateActiveFilterButtonText();

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
        CurrentPaymentType = new PaymentTypeDto { Status = true };
        FormTitle = "Add Payment Type";
        IsEditMode = true;
        IsSidePanelOpen = true;
    }

    /// <summary>
    /// Command to edit a payment type
    /// </summary>
    [RelayCommand]
    private void Edit(PaymentTypeDto paymentType)
    {
        if (paymentType != null)
        {
            CurrentPaymentType = new PaymentTypeDto
            {
                Id = paymentType.Id,
                Name = paymentType.Name,
                PaymentCode = paymentType.PaymentCode,
                NameAr = paymentType.NameAr,
                Status = paymentType.Status
            };
            FormTitle = "Edit Payment Type";
            IsEditMode = true;
            IsSidePanelOpen = true;
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
    /// Command to save the current payment type
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(CurrentPaymentType.Name))
            {
                MessageBox.Show("Please enter a payment type name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentPaymentType.PaymentCode))
            {
                MessageBox.Show("Please enter a payment code.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            if (CurrentPaymentType.Id == 0)
            {
                // Creating new payment type
                StatusMessage = "Creating payment type...";
                
                // Check if name already exists
                if (await _paymentTypeService.ExistsAsync(CurrentPaymentType.Name))
                {
                    MessageBox.Show("A payment type with this name already exists.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if payment code already exists
                if (await _paymentTypeService.PaymentCodeExistsAsync(CurrentPaymentType.PaymentCode))
                {
                    MessageBox.Show("A payment type with this payment code already exists.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var createDto = new CreatePaymentTypeDto
                {
                    Name = CurrentPaymentType.Name,
                    PaymentCode = CurrentPaymentType.PaymentCode,
                    NameAr = CurrentPaymentType.NameAr,
                    Status = CurrentPaymentType.Status,
                    CreatedBy = 1 // Assuming user ID 1 for now
                };

                await _paymentTypeService.CreateAsync(createDto);
                StatusMessage = "Payment type created successfully";
            }
            else
            {
                // Updating existing payment type
                StatusMessage = "Updating payment type...";
                
                // Check if name already exists (excluding current item)
                if (await _paymentTypeService.ExistsAsync(CurrentPaymentType.Name, CurrentPaymentType.Id))
                {
                    MessageBox.Show("A payment type with this name already exists.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if payment code already exists (excluding current item)
                if (await _paymentTypeService.PaymentCodeExistsAsync(CurrentPaymentType.PaymentCode, CurrentPaymentType.Id))
                {
                    MessageBox.Show("A payment type with this payment code already exists.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updateDto = new UpdatePaymentTypeDto
                {
                    Id = CurrentPaymentType.Id,
                    Name = CurrentPaymentType.Name,
                    PaymentCode = CurrentPaymentType.PaymentCode,
                    NameAr = CurrentPaymentType.NameAr,
                    Status = CurrentPaymentType.Status,
                    UpdatedBy = 1 // Assuming user ID 1 for now
                };

                await _paymentTypeService.UpdateAsync(updateDto);
                StatusMessage = "Payment type updated successfully";
            }

            // Close the side panel and refresh the list
            IsEditMode = false;
            IsSidePanelOpen = false;
            CurrentPaymentType = new PaymentTypeDto();
            await LoadPaymentTypesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving payment type: {ex.Message}";
            MessageBox.Show($"Error saving payment type: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to cancel editing
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        IsEditMode = false;
        IsSidePanelOpen = false;
        CurrentPaymentType = new PaymentTypeDto();
    }

    /// <summary>
    /// Command to toggle active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowOnlyActive = !ShowOnlyActive;
        UpdateActiveFilterButtonText();
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
    /// Update the active filter button text
    /// </summary>
    private void UpdateActiveFilterButtonText()
    {
        ActiveFilterButtonText = ShowOnlyActive ? "Show All" : "Active Only";
    }

    #endregion

    #region Property Changed Handlers

    /// <summary>
    /// Handle changes to ShowOnlyActive property
    /// </summary>
    partial void OnShowOnlyActiveChanged(bool value)
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