using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _filteredCustomers = new();

    [ObservableProperty]
    private CustomerDto _selectedCustomer = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private CustomerSidePanelViewModel _sidePanelViewModel;

    [ObservableProperty]
    private bool _showActiveOnly = true;

    [ObservableProperty]
    private System.Windows.FlowDirection _currentFlowDirection = System.Windows.FlowDirection.LeftToRight;

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any customers to display
    /// </summary>
    public bool HasCustomers => FilteredCustomers.Count > 0;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public CustomersViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        
        // Initialize side panel view model
        SidePanelViewModel = new CustomerSidePanelViewModel(
            _customerService,
            CloseSidePanel,
            LoadCustomersAsync);
            
        _ = LoadCustomersAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterCustomers();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterCustomers();
        OnPropertyChanged(nameof(ActiveFilterButtonText));
    }

    private void FilterCustomers()
    {
        var filtered = Customers.AsEnumerable();

        if (ShowActiveOnly)
        {
            filtered = filtered.Where(c => c.Status == "Active");
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c => 
                (!string.IsNullOrEmpty(c.CustomerFullName) && c.CustomerFullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.BusinessFullName) && c.BusinessFullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.OfficialEmail) && c.OfficialEmail.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.MobileNo) && c.MobileNo.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(c.KeyContactName) && c.KeyContactName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                c.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }
        
        FilteredCustomers = new ObservableCollection<CustomerDto>(filtered);
        OnPropertyChanged(nameof(HasCustomers));
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            Customers = new ObservableCollection<CustomerDto>(customers);
            FilterCustomers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ShowAddCustomerSidePanel()
    {
        SidePanelViewModel.InitializeForAdd();
        IsSidePanelVisible = true;
    }

    [RelayCommand]
    private void ShowEditCustomerSidePanel(CustomerDto? customer = null)
    {
        var targetCustomer = customer ?? SelectedCustomer;
        if (targetCustomer?.Id > 0)
        {
            SidePanelViewModel.InitializeForEdit(targetCustomer);
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
    private async Task DeleteCustomerAsync(CustomerDto? customer = null)
    {
        var targetCustomer = customer ?? SelectedCustomer;
        if (targetCustomer?.Id > 0)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the customer '{targetCustomer.DisplayName}'?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(targetCustomer.Id);
                    MessageBox.Show("Customer deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting customer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCustomersAsync();
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
    private async Task ToggleActiveAsync(CustomerDto? customer = null)
    {
        var targetCustomer = customer ?? SelectedCustomer;
        if (targetCustomer?.Id > 0)
        {
            try
            {
                // Toggle the active status
                targetCustomer.Status = targetCustomer.Status == "Active" ? "Inactive" : "Active";
                
                // Update the customer in the database
                await _customerService.UpdateCustomerAsync(targetCustomer);
                
                // Refresh the filtered customers to update the display
                FilterCustomers();
            }
            catch (Exception ex)
            {
                // Revert the change if update failed
                targetCustomer.Status = targetCustomer.Status == "Active" ? "Inactive" : "Active";
                MessageBox.Show($"Error updating customer status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
