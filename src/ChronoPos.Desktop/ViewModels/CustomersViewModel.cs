using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Views.Dialogs;
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
            var errorDialog = new MessageDialog(
                "Loading Error",
                $"An error occurred while loading customers:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
            var confirmDialog = new ConfirmationDialog(
                "Delete Customer",
                $"Are you sure you want to delete the customer '{targetCustomer.DisplayName}'?\n\nThis action cannot be undone.",
                ConfirmationDialog.DialogType.Danger);
            
            var result = confirmDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    await _customerService.DeleteCustomerAsync(targetCustomer.Id);
                    
                    var successDialog = new MessageDialog(
                        "Success",
                        "Customer deleted successfully!",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                    
                    await LoadCustomersAsync();
                }
                catch (Exception ex)
                {
                    var errorDialog = new MessageDialog(
                        "Delete Error",
                        $"An error occurred while deleting the customer:\n\n{ex.Message}",
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
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
                // Include ALL customer fields that are asked in the form
                csv.AppendLine("CustomerFullName,BusinessFullName,IsBusiness,BusinessTypeName,LicenseNo,TrnNo," +
                              "MobileNo,HomePhone,OfficePhone,ContactMobileNo,OfficialEmail," +
                              "CreditAllowed,CreditAmountMax,CreditDays,CreditReference1Name,CreditReference2Name," +
                              "KeyContactName,KeyContactMobile,KeyContactEmail," +
                              "FinancePersonName,FinancePersonMobile,FinancePersonEmail," +
                              "PostDatedChequesAllowed,CustomerGroupName,CustomerBalanceAmount,Status");

                foreach (var customer in Customers)
                {
                    csv.AppendLine($"\"{customer.CustomerFullName}\"," +
                                 $"\"{customer.BusinessFullName}\"," +
                                 $"{customer.IsBusiness}," +
                                 $"\"{customer.BusinessTypeName ?? ""}\"," +
                                 $"\"{customer.LicenseNo}\"," +
                                 $"\"{customer.TrnNo}\"," +
                                 $"\"{customer.MobileNo}\"," +
                                 $"\"{customer.HomePhone}\"," +
                                 $"\"{customer.OfficePhone}\"," +
                                 $"\"{customer.ContactMobileNo}\"," +
                                 $"\"{customer.OfficialEmail}\"," +
                                 $"{customer.CreditAllowed}," +
                                 $"{customer.CreditAmountMax ?? 0}," +
                                 $"{customer.CreditDays ?? 0}," +
                                 $"\"{customer.CreditReference1Name}\"," +
                                 $"\"{customer.CreditReference2Name}\"," +
                                 $"\"{customer.KeyContactName}\"," +
                                 $"\"{customer.KeyContactMobile}\"," +
                                 $"\"{customer.KeyContactEmail}\"," +
                                 $"\"{customer.FinancePersonName}\"," +
                                 $"\"{customer.FinancePersonMobile}\"," +
                                 $"\"{customer.FinancePersonEmail}\"," +
                                 $"{customer.PostDatedChequesAllowed}," +
                                 $"\"{customer.CustomerGroupName ?? ""}\"," +
                                 $"{customer.CustomerBalanceAmount}," +
                                 $"\"{customer.Status}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {Customers.Count} customers to:\n\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new MessageDialog(
                "Export Error",
                $"An error occurred while exporting customers:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
            // Show custom import dialog
            var importDialog = new ImportDialog();
            var dialogResult = importDialog.ShowDialog();
            
            if (dialogResult != true)
                return;

            if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
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
                    // Include ALL customer fields that match the export template
                    templateCsv.AppendLine("CustomerFullName,BusinessFullName,IsBusiness,BusinessTypeName,LicenseNo,TrnNo," +
                                          "MobileNo,HomePhone,OfficePhone,ContactMobileNo,OfficialEmail," +
                                          "CreditAllowed,CreditAmountMax,CreditDays,CreditReference1Name,CreditReference2Name," +
                                          "KeyContactName,KeyContactMobile,KeyContactEmail," +
                                          "FinancePersonName,FinancePersonMobile,FinancePersonEmail," +
                                          "PostDatedChequesAllowed,CustomerGroupName,CustomerBalanceAmount,Status");
                    templateCsv.AppendLine("John Doe,Doe Enterprises,true,Retail,LIC123,TRN123456," +
                                          "1234567890,0987654321,1122334455,9988776655,john@example.com," +
                                          "true,5000,30,Reference One,Reference Two," +
                                          "Jane Doe,5551234567,jane@example.com," +
                                          "Finance Manager,5559876543,finance@example.com," +
                                          "true,VIP,0,Active");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                return;
            }
            else if (importDialog.SelectedAction == ImportDialog.ImportAction.UploadFile)
            {
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
                        var warningDialog = new MessageDialog(
                            "Import Error",
                            "The CSV file is empty or contains only headers.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
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
                        if (values.Length < 25)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 25 columns)");
                            continue;
                        }

                        var customerDto = new CustomerDto
                        {
                            CustomerFullName = values[0].Trim('"'),
                            BusinessFullName = values[1].Trim('"'),
                            IsBusiness = bool.Parse(values[2]),
                            BusinessTypeName = values[3].Trim('"'),
                            LicenseNo = values[4].Trim('"'),
                            TrnNo = values[5].Trim('"'),
                            MobileNo = values[6].Trim('"'),
                            HomePhone = values[7].Trim('"'),
                            OfficePhone = values[8].Trim('"'),
                            ContactMobileNo = values[9].Trim('"'),
                            OfficialEmail = values[10].Trim('"'),
                            CreditAllowed = bool.Parse(values[11]),
                            CreditAmountMax = string.IsNullOrWhiteSpace(values[12]) ? null : decimal.Parse(values[12]),
                            CreditDays = string.IsNullOrWhiteSpace(values[13]) ? null : int.Parse(values[13]),
                            CreditReference1Name = values[14].Trim('"'),
                            CreditReference2Name = values[15].Trim('"'),
                            KeyContactName = values[16].Trim('"'),
                            KeyContactMobile = values[17].Trim('"'),
                            KeyContactEmail = values[18].Trim('"'),
                            FinancePersonName = values[19].Trim('"'),
                            FinancePersonMobile = values[20].Trim('"'),
                            FinancePersonEmail = values[21].Trim('"'),
                            PostDatedChequesAllowed = bool.Parse(values[22]),
                            CustomerBalanceAmount = string.IsNullOrWhiteSpace(values[24]) ? 0 : decimal.Parse(values[24]),
                            Status = values[25].Trim('"')
                        };

                        await _customerService.CreateCustomerAsync(customerDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        var errorMessage = ex.Message;
                        
                        // Include inner exception details if available
                        if (ex.InnerException != null)
                        {
                            errorMessage += $" | Inner: {ex.InnerException.Message}";
                            
                            // Go deeper if there's another inner exception
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                        }
                        
                        errors.AppendLine($"Line {i + 1}: {errorMessage}");
                    }
                }

                    await LoadCustomersAsync();

                    var message = $"Import completed:\n\n✓ {successCount} customers imported successfully";
                    if (errorCount > 0)
                    {
                        message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                    }

                    var resultDialog = new MessageDialog(
                        "Import Complete",
                        message,
                        errorCount > 0 ? MessageDialog.MessageType.Warning : MessageDialog.MessageType.Success);
                    resultDialog.ShowDialog();
                }
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new MessageDialog(
                "Import Error",
                $"An error occurred while importing customers:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
