using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Constants;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;

namespace ChronoPos.Desktop.ViewModels;

public partial class SuppliersViewModel : ObservableObject
{
    private readonly ISupplierService _supplierService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _suppliers = new();

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _filteredSuppliers = new();

    [ObservableProperty]
    private SupplierDto _selectedSupplier = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showActiveOnly = true;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    // Side panel properties
    [ObservableProperty]
    private bool _isSidePanelOpen = false;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private SupplierDto _currentSupplier = new();

    [ObservableProperty]
    private SupplierSidePanelViewModel? _sidePanelViewModel;

    [ObservableProperty]
    private System.Windows.FlowDirection _currentFlowDirection = System.Windows.FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool canCreateSupplier = false;

    [ObservableProperty]
    private bool canEditSupplier = false;

    [ObservableProperty]
    private bool canDeleteSupplier = false;

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any suppliers to display
    /// </summary>
    public bool HasSuppliers => FilteredSuppliers.Count > 0;

    /// <summary>
    /// Total number of suppliers
    /// </summary>
    public int TotalSuppliers => Suppliers.Count;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public SuppliersViewModel(
        ISupplierService supplierService,
        ICurrentUserService currentUserService)
    {
        _supplierService = supplierService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        
        InitializePermissions();
        _ = LoadSuppliersAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterSuppliers();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterSuppliers();
        OnPropertyChanged(nameof(ActiveFilterButtonText));
    }

    private void FilterSuppliers()
    {
        var filtered = Suppliers.AsEnumerable();

        if (ShowActiveOnly)
        {
            filtered = filtered.Where(s => s.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(s => 
                s.CompanyName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (s.ContactName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.PhoneNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.VatTrnNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (s.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        FilteredSuppliers = new ObservableCollection<SupplierDto>(filtered);
        StatusMessage = $"Showing {FilteredSuppliers.Count} of {Suppliers.Count} suppliers";
        OnPropertyChanged(nameof(HasSuppliers));
        OnPropertyChanged(nameof(TotalSuppliers));
    }

    [RelayCommand]
    private async Task LoadSuppliersAsync()
    {
        try
        {
            StatusMessage = "Loading suppliers...";
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            Suppliers = new ObservableCollection<SupplierDto>(suppliers);
            FilterSuppliers();
        }
        catch (Exception ex)
        {
            StatusMessage = "Error loading suppliers";
            MessageBox.Show($"Error loading suppliers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddSupplier()
    {
        CurrentSupplier = new SupplierDto
        {
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        IsEditMode = false;
        InitializeSidePanelViewModel();
        IsSidePanelOpen = true;
    }

    [RelayCommand]
    private void EditSupplier(SupplierDto? supplier = null)
    {
        var targetSupplier = supplier ?? SelectedSupplier;
        if (targetSupplier?.SupplierId > 0)
        {
            // Create a copy for editing
            CurrentSupplier = new SupplierDto
            {
                SupplierId = targetSupplier.SupplierId,
                ShopId = targetSupplier.ShopId,
                CompanyName = targetSupplier.CompanyName,
                LogoPicture = targetSupplier.LogoPicture,
                LicenseNumber = targetSupplier.LicenseNumber,
                OwnerName = targetSupplier.OwnerName,
                OwnerMobile = targetSupplier.OwnerMobile,
                VatTrnNumber = targetSupplier.VatTrnNumber,
                Email = targetSupplier.Email,
                AddressLine1 = targetSupplier.AddressLine1,
                AddressLine2 = targetSupplier.AddressLine2,
                Building = targetSupplier.Building,
                Area = targetSupplier.Area,
                PoBox = targetSupplier.PoBox,
                City = targetSupplier.City,
                State = targetSupplier.State,
                Country = targetSupplier.Country,
                Website = targetSupplier.Website,
                KeyContactName = targetSupplier.KeyContactName,
                KeyContactMobile = targetSupplier.KeyContactMobile,
                KeyContactEmail = targetSupplier.KeyContactEmail,
                Mobile = targetSupplier.Mobile,
                LocationLatitude = targetSupplier.LocationLatitude,
                LocationLongitude = targetSupplier.LocationLongitude,
                CompanyPhoneNumber = targetSupplier.CompanyPhoneNumber,
                Gstin = targetSupplier.Gstin,
                Pan = targetSupplier.Pan,
                PaymentTerms = targetSupplier.PaymentTerms,
                OpeningBalance = targetSupplier.OpeningBalance,
                BalanceType = targetSupplier.BalanceType,
                Status = targetSupplier.Status,
                CreditLimit = targetSupplier.CreditLimit,
                CreatedAt = targetSupplier.CreatedAt,
                UpdatedAt = targetSupplier.UpdatedAt
            };
            IsEditMode = true;
            InitializeSidePanelViewModel();
            IsSidePanelOpen = true;
        }
    }

    [RelayCommand]
    private async Task SaveSupplier()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CurrentSupplier.CompanyName))
            {
                MessageBox.Show("Company name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentSupplier.AddressLine1))
            {
                MessageBox.Show("Address is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = IsEditMode ? "Updating supplier..." : "Creating supplier...";

            if (IsEditMode)
            {
                await _supplierService.UpdateSupplierAsync(CurrentSupplier);
                MessageBox.Show("Supplier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _supplierService.CreateSupplierAsync(CurrentSupplier);
                MessageBox.Show("Supplier created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            IsSidePanelOpen = false;
            await LoadSuppliersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = IsEditMode ? "Error updating supplier" : "Error creating supplier";
            MessageBox.Show($"Error saving supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsSidePanelOpen = false;
        CurrentSupplier = new SupplierDto();
    }

    [RelayCommand]
    private async Task DeleteSupplier(SupplierDto? supplier = null)
    {
        var targetSupplier = supplier ?? SelectedSupplier;
        if (targetSupplier?.SupplierId > 0)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the supplier '{targetSupplier.CompanyName}'?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    StatusMessage = "Deleting supplier...";
                    await _supplierService.DeleteSupplierAsync(targetSupplier.SupplierId);
                    MessageBox.Show("Supplier deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadSuppliersAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = "Error deleting supplier";
                    MessageBox.Show($"Error deleting supplier: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadSuppliersAsync();
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void CloseSidePanel()
    {
        IsSidePanelOpen = false;
        CurrentSupplier = new SupplierDto();
        SidePanelViewModel = null;
    }

    [RelayCommand]
    private void Cancel()
    {
        IsSidePanelOpen = false;
        CurrentSupplier = new SupplierDto();
        SidePanelViewModel = null;
    }

    [RelayCommand]
    private async Task ToggleActive(SupplierDto? supplier)
    {
        if (supplier == null) return;

        try
        {
            supplier.IsActive = !supplier.IsActive;
            supplier.Status = supplier.IsActive ? "Active" : "Inactive";
            await _supplierService.UpdateSupplierAsync(supplier);
            StatusMessage = $"Supplier '{supplier.CompanyName}' {(supplier.IsActive ? "activated" : "deactivated")} successfully.";
            FilterSuppliers(); // Refresh the filtered list
        }
        catch (Exception ex)
        {
            // Revert the change
            supplier.IsActive = !supplier.IsActive;
            supplier.Status = supplier.IsActive ? "Active" : "Inactive";
            StatusMessage = $"Error updating supplier: {ex.Message}";
            MessageBox.Show($"Error updating supplier status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void InitializeSidePanelViewModel()
    {
        SidePanelViewModel = new SupplierSidePanelViewModel(
            _supplierService,
            closeSidePanel: () => IsSidePanelOpen = false,
            refreshParent: LoadSuppliersAsync
        );

        if (IsEditMode && CurrentSupplier.SupplierId > 0)
        {
            SidePanelViewModel.InitializeForEdit(CurrentSupplier);
        }
        else
        {
            SidePanelViewModel.InitializeForAdd();
        }
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS_ADD_OPTIONS, TypeMatrix.CREATE);
            CanEditSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS_ADD_OPTIONS, TypeMatrix.UPDATE);
            CanDeleteSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS_ADD_OPTIONS, TypeMatrix.DELETE);
        }
        catch (Exception)
        {
            CanCreateSupplier = false;
            CanEditSupplier = false;
            CanDeleteSupplier = false;
        }
    }
}