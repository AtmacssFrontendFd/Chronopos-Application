using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Domain.Entities;
using ChronoPos.Desktop.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class ProductCombinationViewModel : ObservableObject
{
    private readonly IProductCombinationItemService _combinationService;
    private readonly IProductUnitService _productUnitService;
    private readonly IProductAttributeService _attributeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<ProductUnitWithAttributes> productUnits = new();

    [ObservableProperty]
    private ProductUnitWithAttributes? selectedProductUnit;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private ProductCombinationSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool canCreateProductCombination = false;

    [ObservableProperty]
    private bool canEditProductCombination = false;

    [ObservableProperty]
    private bool canDeleteProductCombination = false;

    private readonly ICollectionView _filteredProductUnitsView;

    public ICollectionView FilteredCombinations => _filteredProductUnitsView;

    public bool HasCombinations => ProductUnits.Count > 0;
    public int TotalCombinations => ProductUnits.Count;

    public ProductCombinationViewModel(
        IProductCombinationItemService combinationService,
        IProductUnitService productUnitService,
        IProductAttributeService attributeService,
        ICurrentUserService currentUserService,
        Action? navigateBack = null)
    {
        _combinationService = combinationService;
        _productUnitService = productUnitService;
        _attributeService = attributeService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _navigateBack = navigateBack;
        
        InitializePermissions();
        
        // Initialize filtered view
        _filteredProductUnitsView = CollectionViewSource.GetDefaultView(ProductUnits);
        _filteredProductUnitsView.Filter = FilterCombinations;

        // Initialize commands
        LoadCombinationsCommand = new AsyncRelayCommand(LoadCombinationsAsync);
        AddCombinationCommand = new RelayCommand<ProductUnitWithAttributes?>(ShowAddCombinationPanel);
        EditCombinationCommand = new RelayCommand<ProductUnitWithAttributes?>(ShowEditCombinationPanel);
        DeleteCombinationCommand = new AsyncRelayCommand<ProductUnitWithAttributes?>(DeleteProductUnitCombinationsAsync);
        FilterCommand = new RelayCommand(FilterCombinations);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshDataCommand = new AsyncRelayCommand(LoadCombinationsAsync);
        BackCommand = new RelayCommand(GoBack);
        CloseSidePanelCommand = new RelayCommand(CloseSidePanel);

        // Subscribe to search text changes
        PropertyChanged += OnPropertyChanged;
        
        // Load combinations on startup
        _ = LoadCombinationsAsync();
    }

    public IAsyncRelayCommand LoadCombinationsCommand { get; }
    public RelayCommand<ProductUnitWithAttributes?> AddCombinationCommand { get; }
    public RelayCommand<ProductUnitWithAttributes?> EditCombinationCommand { get; }
    public IAsyncRelayCommand<ProductUnitWithAttributes?> DeleteCombinationCommand { get; }
    public RelayCommand FilterCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand CloseSidePanelCommand { get; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText))
        {
            _filteredProductUnitsView.Refresh();
        }
        else if (e.PropertyName == nameof(ProductUnits))
        {
            OnPropertyChanged(nameof(HasCombinations));
            OnPropertyChanged(nameof(TotalCombinations));
        }
    }

    private async Task LoadCombinationsAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading product units...";

            var productUnitsData = await _productUnitService.GetAllWithNavigationAsync();
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                ProductUnits.Clear();
                foreach (var productUnit in productUnitsData)
                {
                    var productUnitWithAttributes = new ProductUnitWithAttributes(productUnit);
                    
                    // Load attribute values for this product unit
                    var combinationItems = await _combinationService.GetCombinationItemsByProductUnitIdAsync(productUnit.Id);
                    var attributeValues = combinationItems
                        .Where(ci => !string.IsNullOrEmpty(ci.AttributeValueName))
                        .Select(ci => ci.AttributeValueName!)
                        .Distinct()
                        .OrderBy(name => name);
                    
                    productUnitWithAttributes.AttributeValues = string.Join(", ", attributeValues);
                    productUnitWithAttributes.CombinationCount = combinationItems.Count();
                    
                    ProductUnits.Add(productUnitWithAttributes);
                }
            });

            StatusMessage = $"Loaded {ProductUnits.Count} product units";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading product units: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddCombinationPanel(ProductUnitWithAttributes? productUnit)
    {
        try
        {
            SidePanelViewModel = new ProductCombinationSidePanelViewModel(
                _combinationService,
                _productUnitService,
                _attributeService,
                async (success) =>
                {
                    if (success)
                    {
                        await LoadCombinationsAsync();
                    }
                    CloseSidePanel();
                },
                CloseSidePanel);

            // If a specific product unit was selected, set it in the side panel
            if (productUnit != null && SidePanelViewModel != null)
            {
                // Convert ProductUnitWithAttributes to ProductUnitDto for the side panel
                var productUnitDto = new ProductUnitDto
                {
                    Id = productUnit.Id,
                    ProductId = productUnit.ProductId,
                    UnitId = productUnit.UnitId,
                    Sku = productUnit.Sku,
                    QtyInUnit = productUnit.QtyInUnit,
                    CostOfUnit = productUnit.CostOfUnit,
                    PriceOfUnit = productUnit.PriceOfUnit,
                    SellingPriceId = productUnit.SellingPriceId,
                    PriceType = productUnit.PriceType,
                    DiscountAllowed = productUnit.DiscountAllowed,
                    IsBase = productUnit.IsBase,
                    CreatedAt = productUnit.CreatedAt,
                    UpdatedAt = productUnit.UpdatedAt,
                    ProductName = productUnit.Product?.Name,
                    UnitName = productUnit.Unit?.Name,
                    UnitAbbreviation = productUnit.Unit?.Abbreviation
                };
                
                // This will need to be set after the side panel loads its data
                Task.Run(async () =>
                {
                    await Task.Delay(100); // Small delay to let the side panel initialize
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        SidePanelViewModel.SelectedProductUnit = productUnitDto;
                    });
                });
            }

            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening add panel: {ex.Message}";
        }
    }

    private void ShowEditCombinationPanel(ProductUnitWithAttributes? productUnit)
    {
        if (productUnit == null) return;

        try
        {
            SidePanelViewModel = new ProductCombinationSidePanelViewModel(
                _combinationService,
                _productUnitService,
                _attributeService,
                async (success) =>
                {
                    if (success)
                    {
                        await LoadCombinationsAsync();
                    }
                    CloseSidePanel();
                },
                CloseSidePanel);

            // Convert ProductUnitWithAttributes to ProductUnitDto for the side panel
            var productUnitDto = new ProductUnitDto
            {
                Id = productUnit.Id,
                ProductId = productUnit.ProductId,
                UnitId = productUnit.UnitId,
                Sku = productUnit.Sku,
                QtyInUnit = productUnit.QtyInUnit,
                CostOfUnit = productUnit.CostOfUnit,
                PriceOfUnit = productUnit.PriceOfUnit,
                SellingPriceId = productUnit.SellingPriceId,
                PriceType = productUnit.PriceType,
                DiscountAllowed = productUnit.DiscountAllowed,
                IsBase = productUnit.IsBase,
                CreatedAt = productUnit.CreatedAt,
                UpdatedAt = productUnit.UpdatedAt,
                ProductName = productUnit.Product?.Name,
                UnitName = productUnit.Unit?.Name,
                UnitAbbreviation = productUnit.Unit?.Abbreviation
            };

            // Set the selected product unit and load existing combinations
            Task.Run(async () =>
            {
                await Task.Delay(200); // Small delay to let the side panel initialize
                
                // First set the selected product unit on UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SidePanelViewModel.SelectedProductUnit = productUnitDto;
                });
                
                // Wait a bit more for the UI to settle
                await Task.Delay(300);
                
                // Then load existing combinations (this doesn't need to be on UI thread)
                await SidePanelViewModel.LoadExistingCombinationsAsync(productUnit.Id);
            });

            IsSidePanelVisible = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error opening edit panel: {ex.Message}";
        }
    }

    private async Task DeleteProductUnitCombinationsAsync(ProductUnitWithAttributes? productUnit)
    {
        if (productUnit == null) return;

        try
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete all combinations for '{productUnit.DisplayName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                StatusMessage = "Deleting combinations...";
                // Get all combinations for this product unit and delete them
                var combinations = await _combinationService.GetCombinationItemsByProductUnitIdAsync(productUnit.Id);
                foreach (var combination in combinations)
                {
                    await _combinationService.DeleteCombinationItemAsync(combination.Id);
                }
                StatusMessage = "Combinations deleted successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting combinations: {ex.Message}";
            MessageBox.Show($"Failed to delete combinations: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool FilterCombinations(object obj)
    {
        if (obj is not ProductUnitWithAttributes productUnit) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var searchLower = SearchText.ToLower();
        return productUnit.DisplayName?.ToLower().Contains(searchLower) == true ||
               productUnit.Product?.Name?.ToLower().Contains(searchLower) == true ||
               productUnit.Unit?.Name?.ToLower().Contains(searchLower) == true ||
               productUnit.Sku?.ToLower().Contains(searchLower) == true ||
               productUnit.AttributeValues?.ToLower().Contains(searchLower) == true;
    }

    private void FilterCombinations()
    {
        _filteredProductUnitsView.Refresh();
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        _filteredProductUnitsView.Refresh();
    }

    private void GoBack()
    {
        _navigateBack?.Invoke();
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateProductCombination = _currentUserService.HasPermission(ScreenNames.PRODUCT_COMBINATIONS, TypeMatrix.CREATE);
            CanEditProductCombination = _currentUserService.HasPermission(ScreenNames.PRODUCT_COMBINATIONS, TypeMatrix.UPDATE);
            CanDeleteProductCombination = _currentUserService.HasPermission(ScreenNames.PRODUCT_COMBINATIONS, TypeMatrix.DELETE);
        }
        catch (Exception)
        {
            CanCreateProductCombination = false;
            CanEditProductCombination = false;
            CanDeleteProductCombination = false;
        }
    }
}