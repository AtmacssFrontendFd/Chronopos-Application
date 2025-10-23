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
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete the store '{store.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result != MessageBoxResult.Yes) return;
        
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
            }
            else
            {
                StatusMessage = $"Failed to delete store '{store.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting store: {ex.Message}";
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
                FileName = "Stores_Export.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                var stores = await _storeService.GetAllAsync();

                var csv = new StringBuilder();
                csv.AppendLine("Name,Address,PhoneNumber,Email,ManagerName,IsActive,IsDefault");

                foreach (var store in stores)
                {
                    csv.AppendLine($"\"{store.Name}\",\"{store.Address}\",\"{store.PhoneNumber}\",\"{store.Email}\",\"{store.ManagerName}\",\"{store.IsActive}\",\"{store.IsDefault}\"");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                MessageBox.Show($"Successfully exported {stores.Count()} stores to:\n{saveFileDialog.FileName}",
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting stores: {ex.Message}", "Export Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportAsync()
    {
        var result = MessageBox.Show(
            "Would you like to download a template file first?\n\n" +
            "Click 'Yes' to download template\n" +
            "Click 'No' to upload your file\n" +
            "Click 'Cancel' to abort",
            "Import Stores",
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
                FileName = "Stores_Template.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var templateCsv = new StringBuilder();
                templateCsv.AppendLine("Name,Address,PhoneNumber,Email,ManagerName,IsActive,IsDefault");
                templateCsv.AppendLine("Main Store,123 Main Street,+1234567890,main@store.com,John Doe,True,True");
                templateCsv.AppendLine("Branch Store,456 Oak Avenue,+1234567891,branch@store.com,Jane Smith,True,False");

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
            try
            {
                var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                if (lines.Length < 2)
                {
                    MessageBox.Show("The CSV file is empty or contains only headers.", "Import Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new StringBuilder();

                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var values = ParseCsvLine(lines[i]);
                        if (values.Length < 7)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 7 columns)");
                            continue;
                        }

                        var createDto = new CreateStoreDto
                        {
                            Name = values[0].Trim('"'),
                            Address = string.IsNullOrWhiteSpace(values[1].Trim('"')) ? null : values[1].Trim('"'),
                            PhoneNumber = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            Email = string.IsNullOrWhiteSpace(values[3].Trim('"')) ? null : values[3].Trim('"'),
                            ManagerName = string.IsNullOrWhiteSpace(values[4].Trim('"')) ? null : values[4].Trim('"'),
                            IsActive = bool.Parse(values[5]),
                            IsDefault = bool.Parse(values[6])
                        };

                        await _storeService.CreateAsync(createDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.AppendLine($"Line {i + 1}: {ex.Message}");
                    }
                }

                await LoadStoresAsync();

                var message = $"Import completed!\n\nSuccessfully imported: {successCount}\nErrors: {errorCount}";
                if (errorCount > 0)
                {
                    message += $"\n\nError details:\n{errors}";
                }

                MessageBox.Show(message, "Import Complete",
                    MessageBoxButton.OK, errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing stores: {ex.Message}", "Import Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
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