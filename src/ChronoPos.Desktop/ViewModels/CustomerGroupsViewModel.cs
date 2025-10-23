using ChronoPos.Application.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Customer Groups management page
/// </summary>
public partial class CustomerGroupsViewModel : ObservableObject
{
    private readonly ICustomerGroupService _customerGroupService;
    private readonly ICustomerGroupRelationService _customerGroupRelationService;
    private readonly ICustomerService _customerService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IDiscountService _discountService;
    private readonly ICurrentUserService _currentUserService;

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

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool canCreateCustomerGroup = false;

    [ObservableProperty]
    private bool canEditCustomerGroup = false;

    [ObservableProperty]
    private bool canDeleteCustomerGroup = false;

    [ObservableProperty]
    private bool canImportCustomerGroup = false;

    [ObservableProperty]
    private bool canExportCustomerGroup = false;

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any customer groups to display
    /// </summary>
    public bool HasCustomerGroups => FilteredCustomerGroups.Count > 0;

    /// <summary>
    /// Total number of customer groups
    /// </summary>
    public int TotalCustomerGroups => CustomerGroups.Count;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public CustomerGroupsViewModel(
        ICustomerGroupService customerGroupService,
        ICustomerGroupRelationService customerGroupRelationService,
        ICustomerService customerService,
        ISellingPriceTypeService sellingPriceTypeService,
        IDiscountService discountService,
        ICurrentUserService currentUserService)
    {
        _customerGroupService = customerGroupService;
        _customerGroupRelationService = customerGroupRelationService;
        _customerService = customerService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _discountService = discountService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        InitializePermissions();
        
        // Initialize side panel view model
        SidePanelViewModel = new CustomerGroupSidePanelViewModel(
            _customerGroupService,
            _customerGroupRelationService,
            _customerService,
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
        OnPropertyChanged(nameof(TotalCustomerGroups));
        StatusMessage = $"Showing {FilteredCustomerGroups.Count} of {CustomerGroups.Count} customer group(s)";
    }

    [RelayCommand]
    private async Task LoadCustomerGroupsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading customer groups...";
            
            var groups = await _customerGroupService.GetAllAsync();
            CustomerGroups = new ObservableCollection<CustomerGroupDto>(groups);
            FilterCustomerGroups();
            
            StatusMessage = $"Loaded {CustomerGroups.Count} customer group(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = "Error loading customer groups";
            MessageBox.Show($"Error loading customer groups: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
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

    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared";
    }

    [RelayCommand]
    private void ViewCustomerGroupDetails(CustomerGroupDto customerGroup)
    {
        if (customerGroup != null)
        {
            MessageBox.Show($"Customer Group Details:\n\nName: {customerGroup.Name}\nArabic Name: {customerGroup.NameAr}\nStatus: {customerGroup.Status}", 
                "Customer Group Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    /// <summary>
    /// Command to export customer groups to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"CustomerGroups_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting customer groups...";

                var csv = new StringBuilder();
                csv.AppendLine("Id,Name,NameAr,SellingPriceTypeId,DiscountId,DiscountValue,DiscountMaxValue,IsPercentage,Status");

                foreach (var group in CustomerGroups)
                {
                    csv.AppendLine($"{group.Id}," +
                                 $"\"{group.Name}\"," +
                                 $"\"{group.NameAr ?? ""}\"," +
                                 $"{group.SellingPriceTypeId}," +
                                 $"{group.DiscountId}," +
                                 $"{group.DiscountValue}," +
                                 $"{group.DiscountMaxValue}," +
                                 $"{group.IsPercentage}," +
                                 $"\"{group.Status}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {CustomerGroups.Count} customer groups successfully";
                MessageBox.Show($"Exported {CustomerGroups.Count} customer groups to:\n{saveFileDialog.FileName}", 
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting customer groups: {ex.Message}";
            MessageBox.Show($"Error exporting customer groups: {ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to import customer groups from CSV
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
                "Import Customer Groups",
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
                    FileName = "CustomerGroups_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Id,Name,NameAr,SellingPriceTypeId,DiscountId,DiscountValue,DiscountMaxValue,IsPercentage,Status");
                    templateCsv.AppendLine("0,VIP Customers,العملاء المميزين,1,1,10,100,true,Active");
                    templateCsv.AppendLine("0,Wholesale,الجملة,2,2,15,500,true,Active");

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
                IsLoading = true;
                StatusMessage = "Importing customer groups...";

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
                        if (values.Length < 9)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 9 columns)");
                            continue;
                        }

                        var createDto = new CreateCustomerGroupDto
                        {
                            Name = values[1].Trim('"'),
                            NameAr = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            SellingPriceTypeId = string.IsNullOrWhiteSpace(values[3]) ? null : long.Parse(values[3]),
                            DiscountId = string.IsNullOrWhiteSpace(values[4]) ? null : int.Parse(values[4]),
                            DiscountValue = string.IsNullOrWhiteSpace(values[5]) ? null : decimal.Parse(values[5]),
                            DiscountMaxValue = string.IsNullOrWhiteSpace(values[6]) ? null : decimal.Parse(values[6]),
                            IsPercentage = bool.Parse(values[7]),
                            Status = values[8].Trim('"')
                        };

                        await _customerGroupService.CreateAsync(createDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.AppendLine($"Line {i + 1}: {ex.Message}");
                    }
                }

                await LoadCustomerGroupsAsync();

                var message = $"Import completed:\n✓ {successCount} customer groups imported successfully";
                if (errorCount > 0)
                {
                    message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                }

                MessageBox.Show(message, "Import Complete", 
                    MessageBoxButton.OK, errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                
                StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing customer groups: {ex.Message}";
            MessageBox.Show($"Error importing customer groups: {ex.Message}", "Import Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
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

    private void InitializePermissions()
    {
        try
        {
            CanCreateCustomerGroup = _currentUserService.HasPermission(ScreenNames.CUSTOMER_GROUPS, TypeMatrix.CREATE);
            CanEditCustomerGroup = _currentUserService.HasPermission(ScreenNames.CUSTOMER_GROUPS, TypeMatrix.UPDATE);
            CanDeleteCustomerGroup = _currentUserService.HasPermission(ScreenNames.CUSTOMER_GROUPS, TypeMatrix.DELETE);
            CanImportCustomerGroup = _currentUserService.HasPermission(ScreenNames.CUSTOMER_GROUPS, TypeMatrix.IMPORT);
            CanExportCustomerGroup = _currentUserService.HasPermission(ScreenNames.CUSTOMER_GROUPS, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateCustomerGroup = false;
            CanEditCustomerGroup = false;
            CanDeleteCustomerGroup = false;
            CanImportCustomerGroup = false;
            CanExportCustomerGroup = false;
        }
    }
}
