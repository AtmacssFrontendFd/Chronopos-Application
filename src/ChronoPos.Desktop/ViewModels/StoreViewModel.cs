using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class StoreViewModel : ObservableObject
{
    private readonly IStoreService _storeService;
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

    private readonly ICollectionView _filteredStoresView;

    public ICollectionView FilteredStores => _filteredStoresView;

    public bool HasStores => Stores.Count > 0;
    public int TotalStores => Stores.Count;

    public StoreViewModel(IStoreService storeService, Action? navigateBack = null)
    {
        _storeService = storeService;
        _navigateBack = navigateBack;
        
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
}