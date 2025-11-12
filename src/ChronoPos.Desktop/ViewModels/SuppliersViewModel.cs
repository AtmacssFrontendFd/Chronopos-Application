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
using System;
using Microsoft.Win32;

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

    [ObservableProperty]
    private bool canImportSupplier = false;

    [ObservableProperty]
    private bool canExportSupplier = false;

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
            
            var errorMessage = $"Error loading suppliers: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
            }
            
            var errorDialog = new MessageDialog(
                "Error",
                errorMessage,
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
                var warningDialog = new MessageDialog(
                    "Validation Error",
                    "Company name is required.",
                    MessageDialog.MessageType.Warning);
                warningDialog.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentSupplier.AddressLine1))
            {
                var warningDialog = new MessageDialog(
                    "Validation Error",
                    "Address is required.",
                    MessageDialog.MessageType.Warning);
                warningDialog.ShowDialog();
                return;
            }

            StatusMessage = IsEditMode ? "Updating supplier..." : "Creating supplier...";

            if (IsEditMode)
            {
                await _supplierService.UpdateSupplierAsync(CurrentSupplier);
                
                var successDialog = new MessageDialog(
                    "Success",
                    "Supplier updated successfully!",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            else
            {
                await _supplierService.CreateSupplierAsync(CurrentSupplier);
                
                var successDialog = new MessageDialog(
                    "Success",
                    "Supplier created successfully!",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }

            IsSidePanelOpen = false;
            await LoadSuppliersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = IsEditMode ? "Error updating supplier" : "Error creating supplier";
            
            var errorMessage = $"Error saving supplier: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
            }
            
            var errorDialog = new MessageDialog(
                "Error",
                errorMessage,
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
            var confirmDialog = new ConfirmationDialog(
                "Confirm Deletion",
                $"Are you sure you want to delete the supplier '{targetSupplier.CompanyName}'?",
                ConfirmationDialog.DialogType.Warning);

            if (confirmDialog.ShowDialog() == true)
            {
                try
                {
                    StatusMessage = "Deleting supplier...";
                    await _supplierService.DeleteSupplierAsync(targetSupplier.SupplierId);
                    
                    var successDialog = new MessageDialog(
                        "Success",
                        "Supplier deleted successfully!",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                    
                    await LoadSuppliersAsync();
                }
                catch (Exception ex)
                {
                    StatusMessage = "Error deleting supplier";
                    
                    var errorMessage = $"Error deleting supplier: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                        }
                    }
                    
                    var errorDialog = new MessageDialog(
                        "Error",
                        errorMessage,
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
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
            
            var errorMessage = $"Error updating supplier status: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
            }
            
            var errorDialog = new MessageDialog(
                "Error",
                errorMessage,
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
    }

    /// <summary>
    /// Command to export suppliers to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Suppliers_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                StatusMessage = "Exporting suppliers...";

                var csv = new StringBuilder();
                csv.AppendLine("CompanyName,LogoPicture,LicenseNumber,OwnerName,OwnerMobile,VatTrnNumber,Email,AddressLine1,AddressLine2,Building,Area,PoBox,City,State,Country,Website,KeyContactName,KeyContactMobile,KeyContactEmail,Mobile,LocationLatitude,LocationLongitude,CompanyPhoneNumber,Gstin,Pan,PaymentTerms,OpeningBalance,BalanceType,Status,CreditLimit");

                foreach (var supplier in Suppliers)
                {
                    csv.AppendLine($"\"{supplier.CompanyName}\"," +
                                 $"\"{supplier.LogoPicture ?? ""}\"," +
                                 $"\"{supplier.LicenseNumber ?? ""}\"," +
                                 $"\"{supplier.OwnerName ?? ""}\"," +
                                 $"\"{supplier.OwnerMobile ?? ""}\"," +
                                 $"\"{supplier.VatTrnNumber ?? ""}\"," +
                                 $"\"{supplier.Email ?? ""}\"," +
                                 $"\"{supplier.AddressLine1}\"," +
                                 $"\"{supplier.AddressLine2 ?? ""}\"," +
                                 $"\"{supplier.Building ?? ""}\"," +
                                 $"\"{supplier.Area ?? ""}\"," +
                                 $"\"{supplier.PoBox ?? ""}\"," +
                                 $"\"{supplier.City ?? ""}\"," +
                                 $"\"{supplier.State ?? ""}\"," +
                                 $"\"{supplier.Country ?? ""}\"," +
                                 $"\"{supplier.Website ?? ""}\"," +
                                 $"\"{supplier.KeyContactName ?? ""}\"," +
                                 $"\"{supplier.KeyContactMobile ?? ""}\"," +
                                 $"\"{supplier.KeyContactEmail ?? ""}\"," +
                                 $"\"{supplier.Mobile ?? ""}\"," +
                                 $"{supplier.LocationLatitude ?? 0}," +
                                 $"{supplier.LocationLongitude ?? 0}," +
                                 $"\"{supplier.CompanyPhoneNumber ?? ""}\"," +
                                 $"\"{supplier.Gstin ?? ""}\"," +
                                 $"\"{supplier.Pan ?? ""}\"," +
                                 $"\"{supplier.PaymentTerms ?? ""}\"," +
                                 $"{supplier.OpeningBalance}," +
                                 $"\"{supplier.BalanceType ?? ""}\"," +
                                 $"\"{supplier.Status}\"," +
                                 $"{supplier.CreditLimit}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {Suppliers.Count} suppliers successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Exported {Suppliers.Count} suppliers to:\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting suppliers: {ex.Message}";
            
            var errorMessage = $"Error exporting suppliers: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
            }
            
            var errorDialog = new MessageDialog(
                "Export Error",
                errorMessage,
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
    }

    /// <summary>
    /// Command to import suppliers from CSV
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            // Show import dialog
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
                    FileName = "Suppliers_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("CompanyName,LogoPicture,LicenseNumber,OwnerName,OwnerMobile,VatTrnNumber,Email,AddressLine1,AddressLine2,Building,Area,PoBox,City,State,Country,Website,KeyContactName,KeyContactMobile,KeyContactEmail,Mobile,LocationLatitude,LocationLongitude,CompanyPhoneNumber,Gstin,Pan,PaymentTerms,OpeningBalance,BalanceType,Status,CreditLimit");
                    templateCsv.AppendLine("Sample Supplier Ltd,,LIC123,John Doe,1234567890,VAT123,supplier@example.com,123 Main St,Suite 100,Building A,Downtown,12345,New York,NY,USA,www.supplier.com,Jane Smith,0987654321,jane@supplier.com,1234567890,25.2048,55.2708,0123456789,GST123,PAN123,Net 30,10000,Credit,Active,50000");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
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
                    StatusMessage = "Importing suppliers...";

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
                            if (values.Length < 30)
                            {
                                errorCount++;
                                errors.AppendLine($"Line {i + 1}: Invalid format (expected 30 columns, got {values.Length})");
                                continue;
                            }

                            var supplierDto = new SupplierDto
                            {
                                CompanyName = values[0].Trim('"'),
                                LogoPicture = values[1].Trim('"'),
                                LicenseNumber = values[2].Trim('"'),
                                OwnerName = values[3].Trim('"'),
                                OwnerMobile = values[4].Trim('"'),
                                VatTrnNumber = values[5].Trim('"'),
                                Email = values[6].Trim('"'),
                                AddressLine1 = values[7].Trim('"'),
                                AddressLine2 = values[8].Trim('"'),
                                Building = values[9].Trim('"'),
                                Area = values[10].Trim('"'),
                                PoBox = values[11].Trim('"'),
                                City = values[12].Trim('"'),
                                State = values[13].Trim('"'),
                                Country = values[14].Trim('"'),
                                Website = values[15].Trim('"'),
                                KeyContactName = values[16].Trim('"'),
                                KeyContactMobile = values[17].Trim('"'),
                                KeyContactEmail = values[18].Trim('"'),
                                Mobile = values[19].Trim('"'),
                                LocationLatitude = decimal.TryParse(values[20], out var lat) ? lat : (decimal?)null,
                                LocationLongitude = decimal.TryParse(values[21], out var lon) ? lon : (decimal?)null,
                                CompanyPhoneNumber = values[22].Trim('"'),
                                Gstin = values[23].Trim('"'),
                                Pan = values[24].Trim('"'),
                                PaymentTerms = values[25].Trim('"'),
                                OpeningBalance = decimal.TryParse(values[26], out var openBal) ? openBal : 0,
                                BalanceType = values[27].Trim('"'),
                                Status = values[28].Trim('"'),
                                CreditLimit = decimal.TryParse(values[29], out var credLimit) ? credLimit : 0,
                                IsActive = values[28].Trim('"').Equals("Active", StringComparison.OrdinalIgnoreCase),
                                CreatedAt = DateTime.UtcNow
                            };

                            await _supplierService.CreateSupplierAsync(supplierDto);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorDetail = ex.Message;
                            if (ex.InnerException != null)
                            {
                                errorDetail += $" | Inner: {ex.InnerException.Message}";
                                if (ex.InnerException.InnerException != null)
                                {
                                    errorDetail += $" | Details: {ex.InnerException.InnerException.Message}";
                                }
                            }
                            errors.AppendLine($"Line {i + 1}: {errorDetail}");
                        }
                    }

                    await LoadSuppliersAsync();

                    var message = $"Import completed:\n✓ {successCount} suppliers imported successfully";
                    if (errorCount > 0)
                    {
                        message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                    }

                    var resultDialog = new MessageDialog(
                        "Import Complete",
                        message,
                        errorCount > 0 ? MessageDialog.MessageType.Warning : MessageDialog.MessageType.Success);
                    resultDialog.ShowDialog();
                    
                    StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing suppliers: {ex.Message}";
            
            var errorMessage = $"Error importing suppliers: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\nDetails: {ex.InnerException.InnerException.Message}";
                }
            }
            
            var errorDialog = new MessageDialog(
                "Import Error",
                errorMessage,
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
            CanCreateSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS, TypeMatrix.CREATE);
            CanEditSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS, TypeMatrix.UPDATE);
            CanDeleteSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS, TypeMatrix.DELETE);
            CanImportSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS, TypeMatrix.IMPORT);
            CanExportSupplier = _currentUserService.HasPermission(ScreenNames.SUPPLIERS, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateSupplier = false;
            CanEditSupplier = false;
            CanDeleteSupplier = false;
            CanImportSupplier = false;
            CanExportSupplier = false;
        }
    }
}