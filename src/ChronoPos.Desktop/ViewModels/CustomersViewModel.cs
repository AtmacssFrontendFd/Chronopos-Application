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

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

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
            filtered = filtered.Where(c => c.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c => 
                c.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                c.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (c.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                c.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }
        
        FilteredCustomers = new ObservableCollection<CustomerDto>(filtered);
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
                $"Are you sure you want to delete the customer '{targetCustomer.FullName}'?",
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
