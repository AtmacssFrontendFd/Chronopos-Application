using ChronoPos.Application.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
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

        var confirmDialog = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the customer group '{customerGroup.Name}'?\n\nThis action cannot be undone.",
            ConfirmationDialog.DialogType.Danger);

        if (confirmDialog.ShowDialog() == true)
        {
            try
            {
                await _customerGroupService.DeleteAsync(customerGroup.Id);
                await LoadCustomerGroupsAsync();
                
                var successDialog = new MessageDialog(
                    "Success",
                    "Customer group deleted successfully!",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to delete customer group.\n\nError: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                    if (ex.InnerException.InnerException != null)
                        errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
                
                var errorDialog = new MessageDialog(
                    "Delete Error",
                    errorMessage,
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
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
                // Export template without Id - 8 fields matching the form
                csv.AppendLine("Name,NameAr,SellingPriceTypeId,DiscountId,DiscountValue,DiscountMaxValue,IsPercentage,Status");

                foreach (var group in CustomerGroups)
                {
                    var statusDisplay = group.Status == "Active" ? "Active" : "Inactive";
                    var isPercentageDisplay = group.IsPercentage ? "Yes" : "No";
                    
                    csv.AppendLine($"\"{group.Name}\"," +
                                 $"\"{group.NameAr ?? ""}\"," +
                                 $"{group.SellingPriceTypeId ?? 0}," +
                                 $"{group.DiscountId ?? 0}," +
                                 $"{group.DiscountValue ?? 0}," +
                                 $"{group.DiscountMaxValue ?? 0}," +
                                 $"\"{isPercentageDisplay}\"," +
                                 $"\"{statusDisplay}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {CustomerGroups.Count} customer groups successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {CustomerGroups.Count} customer group(s) to:\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting customer groups: {ex.Message}";
            
            var errorMessage = $"Failed to export customer groups.\n\nError: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
            }
            
            var errorDialog = new MessageDialog(
                "Export Error",
                errorMessage,
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
        var importDialog = new ImportDialog();
        importDialog.ShowDialog();
        
        if (importDialog.SelectedAction == ImportDialog.ImportAction.None)
            return;

        if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
        {
            // Download Template
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"CustomerGroups_Template_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Name,NameAr,SellingPriceTypeId,DiscountId,DiscountValue,DiscountMaxValue,IsPercentage,Status");
                    templateCsv.AppendLine("VIP Customers,العملاء المميزين,0,0,10,100,Yes,Active");
                    templateCsv.AppendLine("Wholesale,الجملة,0,0,15,500,Yes,Active");
                    templateCsv.AppendLine("Retail,التجزئة,0,0,0,0,No,Active");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Failed to download template.\n\nError: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                            errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                    }
                    
                    var errorDialog = new MessageDialog(
                        "Download Error",
                        errorMessage,
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
                }
            }
            return;
        }

        if (importDialog.SelectedAction == ImportDialog.ImportAction.UploadFile)
        {
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
                
                try
                {
                    // Reload customer groups to ensure we have the latest data for duplicate checking
                    await LoadCustomerGroupsAsync();
                    
                    var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                    if (lines.Length < 2)
                    {
                        var warningDialog = new MessageDialog(
                            "Import Warning",
                            "The CSV file is empty or contains only headers. Please add customer group data and try again.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
                        return;
                    }

                    // Validation phase - check all rows before importing
                    var validationErrors = new StringBuilder();
                    var validGroups = new List<CreateCustomerGroupDto>();
                    var existingNames = CustomerGroups.Select(g => g.Name.ToLower()).ToHashSet();
                    var newNames = new HashSet<string>();

                    // Skip header row
                    for (int i = 1; i < lines.Length; i++)
                    {
                        try
                        {
                            var line = lines[i];
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var values = ParseCsvLine(line);
                            if (values.Length < 8)
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid format (expected 8 columns: Name,NameAr,SellingPriceTypeId,DiscountId,DiscountValue,DiscountMaxValue,IsPercentage,Status)");
                                continue;
                            }

                            var name = values[0].Trim('"').Trim();
                            var nameAr = values[1].Trim('"').Trim();
                            var sellingPriceTypeIdStr = values[2].Trim();
                            var discountIdStr = values[3].Trim();
                            var discountValueStr = values[4].Trim();
                            var discountMaxValueStr = values[5].Trim();
                            var isPercentageStr = values[6].Trim('"').Trim();
                            var status = values[7].Trim('"').Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Name is required");
                                continue;
                            }

                            // Check for duplicate names in existing data
                            if (existingNames.Contains(name.ToLower()))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Customer group name '{name}' already exists");
                                continue;
                            }

                            // Check for duplicate names within the import file
                            if (newNames.Contains(name.ToLower()))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Duplicate customer group name '{name}' in import file");
                                continue;
                            }

                            // Validate IsPercentage format
                            bool isPercentage;
                            if (isPercentageStr.Equals("Yes", StringComparison.OrdinalIgnoreCase) || 
                                isPercentageStr.Equals("True", StringComparison.OrdinalIgnoreCase))
                                isPercentage = true;
                            else if (isPercentageStr.Equals("No", StringComparison.OrdinalIgnoreCase) || 
                                     isPercentageStr.Equals("False", StringComparison.OrdinalIgnoreCase))
                                isPercentage = false;
                            else
                            {
                                validationErrors.AppendLine($"Line {i + 1}: IsPercentage must be 'Yes', 'No', 'True', or 'False', found '{isPercentageStr}'");
                                continue;
                            }

                            // Validate Status format
                            if (!status.Equals("Active", StringComparison.OrdinalIgnoreCase) && 
                                !status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Status must be 'Active' or 'Inactive', found '{status}'");
                                continue;
                            }

                            // Parse numeric values
                            long? sellingPriceTypeId = null;
                            if (!string.IsNullOrWhiteSpace(sellingPriceTypeIdStr) && sellingPriceTypeIdStr != "0")
                            {
                                if (!long.TryParse(sellingPriceTypeIdStr, out var tempId))
                                {
                                    validationErrors.AppendLine($"Line {i + 1}: Invalid SellingPriceTypeId '{sellingPriceTypeIdStr}'");
                                    continue;
                                }
                                sellingPriceTypeId = tempId;
                            }

                            int? discountId = null;
                            if (!string.IsNullOrWhiteSpace(discountIdStr) && discountIdStr != "0")
                            {
                                if (!int.TryParse(discountIdStr, out var tempId))
                                {
                                    validationErrors.AppendLine($"Line {i + 1}: Invalid DiscountId '{discountIdStr}'");
                                    continue;
                                }
                                discountId = tempId;
                            }

                            decimal? discountValue = null;
                            if (!string.IsNullOrWhiteSpace(discountValueStr) && discountValueStr != "0")
                            {
                                if (!decimal.TryParse(discountValueStr, out var tempValue))
                                {
                                    validationErrors.AppendLine($"Line {i + 1}: Invalid DiscountValue '{discountValueStr}'");
                                    continue;
                                }
                                discountValue = tempValue;
                            }

                            decimal? discountMaxValue = null;
                            if (!string.IsNullOrWhiteSpace(discountMaxValueStr) && discountMaxValueStr != "0")
                            {
                                if (!decimal.TryParse(discountMaxValueStr, out var tempValue))
                                {
                                    validationErrors.AppendLine($"Line {i + 1}: Invalid DiscountMaxValue '{discountMaxValueStr}'");
                                    continue;
                                }
                                discountMaxValue = tempValue;
                            }

                            newNames.Add(name.ToLower());
                            validGroups.Add(new CreateCustomerGroupDto
                            {
                                Name = name,
                                NameAr = string.IsNullOrWhiteSpace(nameAr) ? null : nameAr,
                                SellingPriceTypeId = sellingPriceTypeId,
                                DiscountId = discountId,
                                DiscountValue = discountValue,
                                DiscountMaxValue = discountMaxValue,
                                IsPercentage = isPercentage,
                                Status = status
                            });
                        }
                        catch (Exception ex)
                        {
                            validationErrors.AppendLine($"Line {i + 1}: Validation error - {ex.Message}");
                        }
                    }

                    // If validation errors exist, show them and abort
                    if (validationErrors.Length > 0)
                    {
                        var errorDialog = new MessageDialog(
                            "Validation Errors",
                            $"Found {validationErrors.ToString().Split('\n').Length - 1} validation error(s). Please fix these issues and try again:\n\n{validationErrors}",
                            MessageDialog.MessageType.Error);
                        errorDialog.ShowDialog();
                        return;
                    }

                    // Import phase - all validations passed
                    int successCount = 0;
                    int errorCount = 0;
                    var importErrors = new StringBuilder();

                    foreach (var group in validGroups)
                    {
                        try
                        {
                            await _customerGroupService.CreateAsync(group);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMsg = $"Customer group '{group.Name}': {ex.Message}";
                            if (ex.InnerException != null)
                            {
                                errorMsg += $" | Inner: {ex.InnerException.Message}";
                                if (ex.InnerException.InnerException != null)
                                    errorMsg += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                            importErrors.AppendLine(errorMsg);
                        }
                    }

                    await LoadCustomerGroupsAsync();

                    // Show results
                    if (errorCount == 0)
                    {
                        var successDialog = new MessageDialog(
                            "Import Successful",
                            $"Successfully imported {successCount} customer group(s)!",
                            MessageDialog.MessageType.Success);
                        successDialog.ShowDialog();
                    }
                    else
                    {
                        var message = $"Import completed with some errors:\n\nSuccessfully imported: {successCount}\nFailed: {errorCount}\n\nErrors:\n{importErrors}";
                        var resultDialog = new MessageDialog(
                            "Import Completed with Errors",
                            message,
                            MessageDialog.MessageType.Warning);
                        resultDialog.ShowDialog();
                    }
                    
                    StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error importing customer groups: {ex.Message}";
                    
                    var errorMessage = $"Failed to import customer groups.\n\nError: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                            errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                    }
                    
                    var errorDialog = new MessageDialog(
                        "Import Error",
                        errorMessage,
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
                }
                finally
                {
                    IsLoading = false;
                }
            }
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
