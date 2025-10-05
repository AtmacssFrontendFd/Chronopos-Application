using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Customer Groups management page
/// </summary>
public partial class CustomerGroupsViewModel : ObservableObject
{
    private readonly ICustomerGroupService _customerGroupService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IDiscountService _discountService;

    [ObservableProperty]
    private ObservableCollection<CustomerGroupDto> _customerGroups = new();

    [ObservableProperty]
    private ObservableCollection<CustomerGroupDto> _filteredCustomerGroups = new();

    [ObservableProperty]
    private CustomerGroupDto? _selectedCustomerGroup;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private CustomerGroupSidePanelViewModel? _sidePanelViewModel;

    [ObservableProperty]
    private bool _showActiveOnly = true;

    [ObservableProperty]
    private System.Windows.FlowDirection _currentFlowDirection = System.Windows.FlowDirection.LeftToRight;

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any customer groups to display
    /// </summary>
    public bool HasCustomerGroups => FilteredCustomerGroups.Count > 0;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public CustomerGroupsViewModel(
        ICustomerGroupService customerGroupService,
        ISellingPriceTypeService sellingPriceTypeService,
        IDiscountService discountService)
    {
        _customerGroupService = customerGroupService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _discountService = discountService;
        
        // Initialize side panel view model
        SidePanelViewModel = new CustomerGroupSidePanelViewModel(
            _customerGroupService,
            _sellingPriceTypeService,
            _discountService,
            CloseSidePanel,
            LoadCustomerGroupsAsync);
            
        _ = LoadCustomerGroupsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterCustomerGroups();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterCustomerGroups();
        OnPropertyChanged(nameof(ActiveFilterButtonText));
    }

    private void FilterCustomerGroups()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredCustomerGroups = new ObservableCollection<CustomerGroupDto>(
                ShowActiveOnly 
                    ? CustomerGroups.Where(cg => cg.Status == "Active")
                    : CustomerGroups
            );
        }
        else
        {
            var searchLower = SearchText.ToLower();
            FilteredCustomerGroups = new ObservableCollection<CustomerGroupDto>(
                CustomerGroups.Where(cg =>
                    (cg.Name?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (cg.NameAr?.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ?? false))
                .Where(cg => !ShowActiveOnly || cg.Status == "Active")
            );
        }

        OnPropertyChanged(nameof(HasCustomerGroups));
    }

    [RelayCommand]
    private async Task LoadCustomerGroupsAsync()
    {
        try
        {
            var groups = await _customerGroupService.GetAllAsync();
            CustomerGroups = new ObservableCollection<CustomerGroupDto>(groups);
            FilterCustomerGroups();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customer groups: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void AddCustomerGroup()
    {
        if (SidePanelViewModel != null)
        {
            SidePanelViewModel.OpenForAdd();
            IsSidePanelVisible = true;
        }
    }

    [RelayCommand]
    private void EditCustomerGroup(CustomerGroupDto customerGroup)
    {
        if (customerGroup != null && SidePanelViewModel != null)
        {
            SelectedCustomerGroup = customerGroup;
            SidePanelViewModel.OpenForEdit(customerGroup);
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Close the side panel
    /// </summary>
    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
    }

    [RelayCommand]
    private async Task DeleteCustomerGroup(CustomerGroupDto customerGroup)
    {
        if (customerGroup == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the customer group '{customerGroup.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _customerGroupService.DeleteAsync(customerGroup.Id);
                await LoadCustomerGroupsAsync();
                MessageBox.Show("Customer group deleted successfully", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer group: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
    }

    [RelayCommand]
    private async Task ToggleActive(CustomerGroupDto customerGroup)
    {
        try
        {
            // Toggle status
            customerGroup.Status = customerGroup.Status == "Active" ? "Inactive" : "Active";
            
            // Update in database
            var updateDto = new UpdateCustomerGroupDto
            {
                Id = customerGroup.Id,
                Name = customerGroup.Name,
                NameAr = customerGroup.NameAr,
                SellingPriceTypeId = customerGroup.SellingPriceTypeId,
                DiscountId = customerGroup.DiscountId,
                DiscountValue = customerGroup.DiscountValue,
                DiscountMaxValue = customerGroup.DiscountMaxValue,
                IsPercentage = customerGroup.IsPercentage,
                Status = customerGroup.Status
            };
            
            await _customerGroupService.UpdateAsync(updateDto);
            
            // Refresh list
            await LoadCustomerGroupsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating status: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    [RelayCommand]
    private void RefreshData()
    {
        _ = LoadCustomerGroupsAsync();
    }
}
