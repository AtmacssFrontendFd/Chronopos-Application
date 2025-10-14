using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Constants;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly ICustomerGroupService _customerGroupService;
    private readonly ICurrentUserService _currentUserService;

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

    [ObservableProperty]
    private bool canCreateCustomer = false;

    [ObservableProperty]
    private bool canEditCustomer = false;

    [ObservableProperty]
    private bool canDeleteCustomer = false;

    [ObservableProperty]
    private bool canImportCustomer = false;

    [ObservableProperty]
    private bool canExportCustomer = false;

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

    public CustomersViewModel(
        ICustomerService customerService, 
        ICustomerGroupService customerGroupService,
        ICurrentUserService currentUserService)
    {
        _customerService = customerService;
        _customerGroupService = customerGroupService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        
        // Initialize side panel view model
        SidePanelViewModel = new CustomerSidePanelViewModel(
            _customerService,
            _customerGroupService,
            CloseSidePanel,
            LoadCustomersAsync);
        
        InitializePermissions();
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

    /// <summary>
    /// Command to export customers to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Customers_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var csv = new StringBuilder();
                csv.AppendLine("Id,CustomerFullName,BusinessFullName,IsBusiness,MobileNo,OfficialEmail,CustomerGroupName,Status,CreditAllowed,CreditAmountMax,TrnNo,CustomerBalanceAmount,CreatedAt");

                foreach (var customer in Customers)
                {
                    csv.AppendLine($"{customer.Id}," +
                                 $"\"{customer.CustomerFullName}\"," +
                                 $"\"{customer.BusinessFullName}\"," +
                                 $"{customer.IsBusiness}," +
                                 $"\"{customer.MobileNo}\"," +
                                 $"\"{customer.OfficialEmail}\"," +
                                 $"\"{customer.CustomerGroupName ?? ""}\"," +
                                 $"\"{customer.Status}\"," +
                                 $"{customer.CreditAllowed}," +
                                 $"{customer.CreditAmountMax ?? 0}," +
                                 $"\"{customer.TrnNo}\"," +
                                 $"{customer.CustomerBalanceAmount}," +
                                 $"\"{customer.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                MessageBox.Show($"Exported {Customers.Count} customers to:\n{saveFileDialog.FileName}", 
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting customers: {ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Command to import customers from CSV
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            // Show dialog with Download Template and Upload File options
            var result = MessageBox.Show(
                "Would you like to download a template first?\n\n" +
                "• Click 'Yes' to download the CSV template\n" +
                "• Click 'No' to upload your file directly\n" +
                "• Click 'Cancel' to exit",
                "Import Customers",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
            {
                // Download Template
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "Customers_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("CustomerId,FirstName,LastName,Email,Mobile,CustomerType,CompanyName,VatTrnNumber,LicenseNumber,AddressLine1,AddressLine2,Building,Area,PoBox,City,State,Country,OpeningBalance,BalanceType,Status,CreditLimit");
                    templateCsv.AppendLine("0,John,Doe,john.doe@example.com,1234567890,Individual,,,123 Main St,Apt 4B,Building A,Downtown,12345,New York,NY,USA,0,Credit,true,5000");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    MessageBox.Show($"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.", 
                        "Template Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            }

            // Upload File
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                if (lines.Length <= 1)
                {
                    MessageBox.Show("The CSV file is empty or contains only headers.", "Import Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new StringBuilder();

                // Skip header row
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var values = ParseCsvLine(line);
                        if (values.Length < 13)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 13 columns)");
                            continue;
                        }

                        var customerDto = new CustomerDto
                        {
                            CustomerFullName = values[1].Trim('"'),
                            BusinessFullName = values[2].Trim('"'),
                            IsBusiness = bool.Parse(values[3]),
                            MobileNo = values[4].Trim('"'),
                            OfficialEmail = values[5].Trim('"'),
                            Status = values[7].Trim('"'),
                            CreditAllowed = bool.Parse(values[8]),
                            CreditAmountMax = string.IsNullOrWhiteSpace(values[9]) ? null : decimal.Parse(values[9]),
                            TrnNo = values[10].Trim('"')
                        };

                        await _customerService.CreateCustomerAsync(customerDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.AppendLine($"Line {i + 1}: {ex.Message}");
                    }
                }

                await LoadCustomersAsync();

                var message = $"Import completed:\n✓ {successCount} customers imported successfully";
                if (errorCount > 0)
                {
                    message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                }

                MessageBox.Show(message, "Import Complete", 
                    MessageBoxButton.OK, errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error importing customers: {ex.Message}", "Import Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Parse CSV line handling quoted values
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentValue.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString());
        return values.ToArray();
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

    private void InitializePermissions()
    {
        try
        {
            CanCreateCustomer = _currentUserService.HasPermission(ScreenNames.CUSTOMERS_ADD_OPTIONS, TypeMatrix.CREATE);
            CanEditCustomer = _currentUserService.HasPermission(ScreenNames.CUSTOMERS_ADD_OPTIONS, TypeMatrix.UPDATE);
            CanDeleteCustomer = _currentUserService.HasPermission(ScreenNames.CUSTOMERS_ADD_OPTIONS, TypeMatrix.DELETE);
            CanImportCustomer = _currentUserService.HasPermission(ScreenNames.CUSTOMERS_ADD_OPTIONS, TypeMatrix.IMPORT);
            CanExportCustomer = _currentUserService.HasPermission(ScreenNames.CUSTOMERS_ADD_OPTIONS, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateCustomer = false;
            CanEditCustomer = false;
            CanDeleteCustomer = false;
            CanImportCustomer = false;
            CanExportCustomer = false;
        }
    }
}
