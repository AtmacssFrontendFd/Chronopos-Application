using ChronoPos.Application.Constants;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;
using System.IO;
using System.Text;
using Microsoft.Win32;
using ChronoPos.Desktop.Views.Dialogs;

namespace ChronoPos.Desktop.ViewModels;

public partial class StoreViewModel : ObservableObject
{
    private readonly IStoreService _storeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<StoreDto> stores = new();

    [ObservableProperty]
    private StoreDto? selectedStore;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private StoreSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool showActiveOnly = false;

    [ObservableProperty]
    private bool canCreateStore = false;

    [ObservableProperty]
    private bool canEditStore = false;

    [ObservableProperty]
    private bool canDeleteStore = false;

    [ObservableProperty]
    private bool canImportStore = false;

    [ObservableProperty]
    private bool canExportStore = false;

    private readonly ICollectionView _filteredStoresView;

    public ICollectionView FilteredStores => _filteredStoresView;

    public bool HasStores => Stores.Count > 0;
    public int TotalStores => Stores.Count;

    public StoreViewModel(IStoreService storeService, ICurrentUserService currentUserService, Action? navigateBack = null)
    {
        _storeService = storeService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _navigateBack = navigateBack;

        InitializePermissions();
        
        // Initialize filtered view
        _filteredStoresView = CollectionViewSource.GetDefaultView(Stores);
        _filteredStoresView.Filter = FilterStores;

        // Initialize commands
        LoadStoresCommand = new AsyncRelayCommand(LoadStoresAsync);
        AddStoreCommand = new RelayCommand(ShowAddStorePanel);
        EditStoreCommand = new RelayCommand<StoreDto?>(ShowEditStorePanel);
        DeleteStoreCommand = new AsyncRelayCommand<StoreDto?>(DeleteStoreAsync);
        FilterActiveCommand = new RelayCommand(FilterActive);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshDataCommand = new AsyncRelayCommand(LoadStoresAsync);
        ToggleActiveCommand = new AsyncRelayCommand<StoreDto?>(ToggleActiveAsync);
        SetAsDefaultCommand = new AsyncRelayCommand<StoreDto?>(SetAsDefaultAsync);
        BackCommand = new RelayCommand(GoBack);
        CloseSidePanelCommand = new RelayCommand(CloseSidePanel);
        ExportCommand = new AsyncRelayCommand(ExportAsync);
        ImportCommand = new AsyncRelayCommand(ImportAsync);

        // Subscribe to search text changes
        PropertyChanged += OnPropertyChanged;
        
        // Load stores on startup
        _ = LoadStoresAsync();
    }

    public IAsyncRelayCommand LoadStoresCommand { get; }
    public RelayCommand AddStoreCommand { get; }
    public RelayCommand<StoreDto?> EditStoreCommand { get; }
    public IAsyncRelayCommand<StoreDto?> DeleteStoreCommand { get; }
    public RelayCommand FilterActiveCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public IAsyncRelayCommand<StoreDto?> ToggleActiveCommand { get; }
    public IAsyncRelayCommand<StoreDto?> SetAsDefaultCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand CloseSidePanelCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }
    public IAsyncRelayCommand ImportCommand { get; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(ShowActiveOnly))
        {
            _filteredStoresView.Refresh();
        }
        else if (e.PropertyName == nameof(Stores))
        {
            OnPropertyChanged(nameof(HasStores));
            OnPropertyChanged(nameof(TotalStores));
            _filteredStoresView.Refresh();
        }
    }

    private bool FilterStores(object obj)
    {
        if (obj is not StoreDto store) return false;
        
        // Apply active filter
        if (ShowActiveOnly && !store.IsActive) return false;
        
        // Apply search filter
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        
        return store.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrEmpty(store.Address) && store.Address.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
               (!string.IsNullOrEmpty(store.PhoneNumber) && store.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
               (!string.IsNullOrEmpty(store.Email) && store.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
               (!string.IsNullOrEmpty(store.ManagerName) && store.ManagerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }

    private async Task LoadStoresAsync()
    {
        IsLoading = true;
        try
        {
            var allStores = await _storeService.GetAllAsync();
            
            // Clear and repopulate the existing collection to maintain the filtered view
            Stores.Clear();
            foreach (var store in allStores)
            {
                Stores.Add(store);
            }
            
            StatusMessage = $"Loaded {Stores.Count} stores.";
            
            // Refresh the filtered view
            _filteredStoresView.Refresh();
            OnPropertyChanged(nameof(HasStores));
            OnPropertyChanged(nameof(TotalStores));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading stores: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddStorePanel()
    {
        SidePanelViewModel = new StoreSidePanelViewModel(
            _storeService,
            OnStoreSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = "Add new store...";
    }

    private void ShowEditStorePanel(StoreDto? store)
    {
        if (store == null) return;
        
        SidePanelViewModel = new StoreSidePanelViewModel(
            _storeService,
            store,
            OnStoreSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = $"Edit store '{store.Name}'...";
    }

    private void OnStoreSaved(bool success)
    {
        if (success)
        {
            CloseSidePanel();
            // Reload stores to reflect changes
            _ = LoadStoresAsync();
        }
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Store editing cancelled.";
    }

    private async Task DeleteStoreAsync(StoreDto? store)
    {
        if (store == null) return;
        
        var confirmDialog = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the store '{store.Name}'?",
            ConfirmationDialog.DialogType.Danger);
            
        if (confirmDialog.ShowDialog() != true) return;
        
        try
        {
            var success = await _storeService.DeleteAsync(store.Id);
            if (success)
            {
                Stores.Remove(store);
                StatusMessage = $"Store '{store.Name}' deleted.";
                _filteredStoresView.Refresh();
                OnPropertyChanged(nameof(HasStores));
                OnPropertyChanged(nameof(TotalStores));
                
                var successDialog = new MessageDialog(
                    "Success",
                    $"Store '{store.Name}' deleted successfully!",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            else
            {
                StatusMessage = $"Failed to delete store '{store.Name}'.";
                
                var errorDialog = new MessageDialog(
                    "Error",
                    $"Failed to delete store '{store.Name}'.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting store: {ex.Message}";
            
            var errorMessage = $"Error deleting store: {ex.Message}";
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

    private async Task ToggleActiveAsync(StoreDto? store)
    {
        if (store == null) return;
        
        try
        {
            var updateDto = new UpdateStoreDto
            {
                Name = store.Name,
                Address = store.Address,
                PhoneNumber = store.PhoneNumber,
                Email = store.Email,
                ManagerName = store.ManagerName,
                IsActive = !store.IsActive,
                IsDefault = store.IsDefault
            };
            
            var updatedStore = await _storeService.UpdateAsync(store.Id, updateDto);
            if (updatedStore != null)
            {
                store.IsActive = updatedStore.IsActive;
                StatusMessage = $"Store '{store.Name}' {(store.IsActive ? "activated" : "deactivated")}.";
            }
            else
            {
                StatusMessage = $"Failed to update store '{store.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating store: {ex.Message}";
        }
    }

    private async Task SetAsDefaultAsync(StoreDto? store)
    {
        if (store == null) return;
        
        try
        {
            var updatedStore = await _storeService.SetAsDefaultAsync(store.Id);
            if (updatedStore != null)
            {
                // Update all stores in the collection
                foreach (var s in Stores)
                {
                    s.IsDefault = s.Id == store.Id;
                }
                StatusMessage = $"Store '{store.Name}' set as default.";
                _filteredStoresView.Refresh();
            }
            else
            {
                StatusMessage = $"Failed to set store '{store.Name}' as default.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error setting default store: {ex.Message}";
        }
    }

    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        StatusMessage = ShowActiveOnly ? "Showing active stores only." : "Showing all stores.";
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared.";
    }

    private void GoBack()
    {
        if (_navigateBack != null)
        {
            _navigateBack.Invoke();
        }
        else
        {
            StatusMessage = "Navigation back not configured.";
        }
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateStore = _currentUserService.HasPermission(ScreenNames.SHOP, TypeMatrix.CREATE);
            CanEditStore = _currentUserService.HasPermission(ScreenNames.SHOP, TypeMatrix.UPDATE);
            CanDeleteStore = _currentUserService.HasPermission(ScreenNames.SHOP, TypeMatrix.DELETE);
            CanImportStore = _currentUserService.HasPermission(ScreenNames.SHOP, TypeMatrix.IMPORT);
            CanExportStore = _currentUserService.HasPermission(ScreenNames.SHOP, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreateStore = false;
            CanEditStore = false;
            CanDeleteStore = false;
            CanImportStore = false;
            CanExportStore = false;
        }
    }

    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Stores_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var stores = await _storeService.GetAllAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Name,Address,PhoneNumber,Email,ManagerName,IsActive");

                foreach (var store in stores)
                {
                    csv.AppendLine($"\"{store.Name}\"," +
                                 $"\"{store.Address ?? ""}\"," +
                                 $"\"{store.PhoneNumber ?? ""}\"," +
                                 $"\"{store.Email ?? ""}\"," +
                                 $"\"{store.ManagerName ?? ""}\"," +
                                 $"{(store.IsActive ? "Active" : "Inactive")}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {stores.Count()} stores to:\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error exporting stores: {ex.Message}";
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
        finally
        {
            IsLoading = false;
        }
    }

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
                FileName = $"Stores_Template_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Name,Address,PhoneNumber,Email,ManagerName,IsActive");
                    templateCsv.AppendLine("Main Store,123 Main Street,+1234567890,main@store.com,John Doe,Active");
                    templateCsv.AppendLine("Branch Store,456 Oak Avenue,+1234567891,branch@store.com,Jane Smith,Active");
                    templateCsv.AppendLine("Warehouse,789 Industrial Blvd,+1234567892,warehouse@store.com,Mike Johnson,Inactive");

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
                try
                {
                    // Reload stores to ensure we have the latest data for duplicate checking
                    await LoadStoresAsync();
                    
                    var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                    if (lines.Length < 2)
                    {
                        var warningDialog = new MessageDialog(
                            "Import Warning",
                            "The CSV file is empty or contains only headers. Please add store data and try again.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
                        return;
                    }

                    // Validation phase - check all rows before importing
                    var validationErrors = new StringBuilder();
                    var validStores = new List<CreateStoreDto>();
                    var existingNames = Stores.Select(s => s.Name.ToLower()).ToHashSet();
                    var newNames = new HashSet<string>();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        try
                        {
                            var values = ParseCsvLine(lines[i]);
                            if (values.Length < 6)
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Invalid format (expected 6 columns: Name,Address,PhoneNumber,Email,ManagerName,IsActive)");
                                continue;
                            }

                            var name = values[0].Trim('"').Trim();
                            var address = values[1].Trim('"').Trim();
                            var phoneNumber = values[2].Trim('"').Trim();
                            var email = values[3].Trim('"').Trim();
                            var managerName = values[4].Trim('"').Trim();
                            var isActiveStr = values[5].Trim('"').Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Name is required");
                                continue;
                            }

                            // Check for duplicate names in existing data
                            if (existingNames.Contains(name.ToLower()))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Store name '{name}' already exists");
                                continue;
                            }

                            // Check for duplicate names within the import file
                            if (newNames.Contains(name.ToLower()))
                            {
                                validationErrors.AppendLine($"Line {i + 1}: Duplicate store name '{name}' in import file");
                                continue;
                            }

                            // Validate IsActive format
                            bool isActive;
                            if (isActiveStr.Equals("Active", StringComparison.OrdinalIgnoreCase))
                                isActive = true;
                            else if (isActiveStr.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
                                isActive = false;
                            else
                            {
                                validationErrors.AppendLine($"Line {i + 1}: IsActive must be 'Active' or 'Inactive', found '{isActiveStr}'");
                                continue;
                            }

                            newNames.Add(name.ToLower());
                            validStores.Add(new CreateStoreDto
                            {
                                Name = name,
                                Address = string.IsNullOrWhiteSpace(address) ? null : address,
                                PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
                                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                                ManagerName = string.IsNullOrWhiteSpace(managerName) ? null : managerName,
                                IsActive = isActive,
                                IsDefault = false // Never allow imports to set default store
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

                    foreach (var store in validStores)
                    {
                        try
                        {
                            await _storeService.CreateAsync(store);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMsg = $"Store '{store.Name}': {ex.Message}";
                            if (ex.InnerException != null)
                            {
                                errorMsg += $" | Inner: {ex.InnerException.Message}";
                                if (ex.InnerException.InnerException != null)
                                    errorMsg += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                            importErrors.AppendLine(errorMsg);
                        }
                    }

                    await LoadStoresAsync();

                    // Show results
                    if (errorCount == 0)
                    {
                        var successDialog = new MessageDialog(
                            "Import Successful",
                            $"Successfully imported {successCount} store(s)!",
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
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Failed to import stores.\n\nError: {ex.Message}";
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
}