using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class BrandViewModel : ObservableObject
{
    private readonly IBrandService _brandService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<BrandDto> brands = new();

    [ObservableProperty]
    private BrandDto? selectedBrand;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private BrandSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool showActiveOnly = false;

    // Permission Properties
    [ObservableProperty]
    private bool canCreateBrand = false;

    [ObservableProperty]
    private bool canEditBrand = false;

    [ObservableProperty]
    private bool canDeleteBrand = false;

    private readonly ICollectionView _filteredBrandsView;

    public ICollectionView FilteredBrands => _filteredBrandsView;

    public bool HasBrands => Brands.Count > 0;
    public int TotalBrands => Brands.Count;

    public BrandViewModel(IBrandService brandService, ICurrentUserService currentUserService, Action? navigateBack = null)
    {
        _brandService = brandService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _navigateBack = navigateBack;
        
        // Initialize permissions
        InitializePermissions();
        
        // Initialize filtered view
        _filteredBrandsView = CollectionViewSource.GetDefaultView(Brands);
        _filteredBrandsView.Filter = FilterBrands;

        // Initialize commands
        LoadBrandsCommand = new AsyncRelayCommand(LoadBrandsAsync);
        AddBrandCommand = new RelayCommand(ShowAddBrandPanel);
        EditBrandCommand = new RelayCommand<BrandDto?>(ShowEditBrandPanel);
        DeleteBrandCommand = new AsyncRelayCommand<BrandDto?>(DeleteBrandAsync);
        FilterActiveCommand = new RelayCommand(FilterActive);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshDataCommand = new AsyncRelayCommand(LoadBrandsAsync);
        ToggleActiveCommand = new AsyncRelayCommand<BrandDto?>(ToggleActiveAsync);
        ViewBrandDetailsCommand = new RelayCommand<BrandDto?>(ViewBrandDetails);
        BackCommand = new RelayCommand(GoBack);
        CloseSidePanelCommand = new RelayCommand(CloseSidePanel);

        // Subscribe to search text changes
        PropertyChanged += OnPropertyChanged;
        
        // Load brands on startup
        _ = LoadBrandsAsync();
    }

    public IAsyncRelayCommand LoadBrandsCommand { get; }
    public RelayCommand AddBrandCommand { get; }
    public RelayCommand<BrandDto?> EditBrandCommand { get; }
    public IAsyncRelayCommand<BrandDto?> DeleteBrandCommand { get; }
    public RelayCommand FilterActiveCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public IAsyncRelayCommand<BrandDto?> ToggleActiveCommand { get; }
    public RelayCommand<BrandDto?> ViewBrandDetailsCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand CloseSidePanelCommand { get; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(ShowActiveOnly))
        {
            _filteredBrandsView.Refresh();
        }
        else if (e.PropertyName == nameof(Brands))
        {
            OnPropertyChanged(nameof(HasBrands));
            OnPropertyChanged(nameof(TotalBrands));
            _filteredBrandsView.Refresh();
        }
    }

    private bool FilterBrands(object obj)
    {
        if (obj is not BrandDto brand) return false;
        
        // Apply active filter
        if (ShowActiveOnly && !brand.IsActive) return false;
        
        // Apply search filter
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        
        return brand.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrEmpty(brand.NameArabic) && brand.NameArabic.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
               (!string.IsNullOrEmpty(brand.Description) && brand.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }

    private async Task LoadBrandsAsync()
    {
        IsLoading = true;
        try
        {
            var allBrands = await _brandService.GetAllAsync();
            
            // Clear and repopulate the existing collection to maintain the filtered view
            Brands.Clear();
            foreach (var brand in allBrands)
            {
                Brands.Add(brand);
            }
            
            StatusMessage = $"Loaded {Brands.Count} brands.";
            
            // Refresh the filtered view
            _filteredBrandsView.Refresh();
            OnPropertyChanged(nameof(HasBrands));
            OnPropertyChanged(nameof(TotalBrands));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading brands: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddBrandPanel()
    {
        SidePanelViewModel = new BrandSidePanelViewModel(
            _brandService,
            OnBrandSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = "Add new brand...";
    }

    private void ShowEditBrandPanel(BrandDto? brand)
    {
        if (brand == null) return;
        
        SidePanelViewModel = new BrandSidePanelViewModel(
            _brandService,
            brand,
            OnBrandSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = $"Edit brand '{brand.Name}'...";
    }

    private void OnBrandSaved(bool success)
    {
        if (success)
        {
            CloseSidePanel();
            // Reload brands to reflect changes
            _ = LoadBrandsAsync();
        }
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Brand editing cancelled.";
    }

    private async Task DeleteBrandAsync(BrandDto? brand)
    {
        if (brand == null) return;
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete the brand '{brand.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result != MessageBoxResult.Yes) return;
        
        try
        {
            var success = await _brandService.DeleteAsync(brand.Id);
            if (success)
            {
                Brands.Remove(brand);
                StatusMessage = $"Brand '{brand.Name}' deleted.";
                _filteredBrandsView.Refresh();
                OnPropertyChanged(nameof(HasBrands));
                OnPropertyChanged(nameof(TotalBrands));
            }
            else
            {
                StatusMessage = $"Failed to delete brand '{brand.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting brand: {ex.Message}";
        }
    }

    private async Task ToggleActiveAsync(BrandDto? brand)
    {
        if (brand == null) return;
        
        try
        {
            var updateDto = new UpdateBrandDto
            {
                Name = brand.Name,
                NameArabic = brand.NameArabic,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                IsActive = !brand.IsActive
            };
            
            var updatedBrand = await _brandService.UpdateAsync(brand.Id, updateDto);
            if (updatedBrand != null)
            {
                brand.IsActive = updatedBrand.IsActive;
                StatusMessage = $"Brand '{brand.Name}' {(brand.IsActive ? "activated" : "deactivated")}.";
            }
            else
            {
                StatusMessage = $"Failed to update brand '{brand.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating brand: {ex.Message}";
        }
    }

    private void ViewBrandDetails(BrandDto? brand)
    {
        if (brand == null) return;
        StatusMessage = $"Viewing details for brand '{brand.Name}'...";
        // TODO: Implement brand details view
    }

    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        StatusMessage = ShowActiveOnly ? "Showing active brands only." : "Showing all brands.";
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared.";
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.CREATE);
            CanEditBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.UPDATE);
            CanDeleteBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.DELETE);
        }
        catch (Exception)
        {
            // Fail-secure: all permissions default to false
            CanCreateBrand = false;
            CanEditBrand = false;
            CanDeleteBrand = false;
        }
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
