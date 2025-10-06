using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Product Groups management page
/// </summary>
public partial class ProductGroupsViewModel : ObservableObject
{
    private readonly IProductGroupService _productGroupService;
    private readonly IProductGroupItemService _productGroupItemService;
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _productUnitService;

    [ObservableProperty]
    private ObservableCollection<ProductGroupDto> _productGroups = new();

    [ObservableProperty]
    private ObservableCollection<ProductGroupDto> _filteredProductGroups = new();

    [ObservableProperty]
    private ProductGroupDto? _selectedProductGroup;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    [ObservableProperty]
    private ProductGroupSidePanelViewModel? _sidePanelViewModel;

    [ObservableProperty]
    private bool _showActiveOnly = true;

    [ObservableProperty]
    private System.Windows.FlowDirection _currentFlowDirection = System.Windows.FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any product groups to display
    /// </summary>
    public bool HasProductGroups => FilteredProductGroups.Count > 0;

    /// <summary>
    /// Total number of product groups (before filtering)
    /// </summary>
    public int TotalProductGroups => ProductGroups.Count;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public ProductGroupsViewModel(
        IProductGroupService productGroupService,
        IProductGroupItemService productGroupItemService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        ISellingPriceTypeService sellingPriceTypeService,
        IProductService productService,
        IProductUnitService productUnitService)
    {
        _productGroupService = productGroupService;
        _productGroupItemService = productGroupItemService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _productService = productService;
        _productUnitService = productUnitService;
        
        // Initialize side panel view model
        SidePanelViewModel = new ProductGroupSidePanelViewModel(
            _productGroupService,
            _productGroupItemService,
            _discountService,
            _taxTypeService,
            _sellingPriceTypeService,
            _productService,
            _productUnitService,
            CloseSidePanel,
            LoadProductGroupsAsync);
            
        _ = LoadProductGroupsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterProductGroups();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterProductGroups();
        OnPropertyChanged(nameof(ActiveFilterButtonText));
    }

    private void FilterProductGroups()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredProductGroups = new ObservableCollection<ProductGroupDto>(
                ShowActiveOnly 
                    ? ProductGroups.Where(pg => pg.Status == "Active")
                    : ProductGroups
            );
        }
        else
        {
            var searchLower = SearchText.ToLower();
            FilteredProductGroups = new ObservableCollection<ProductGroupDto>(
                (ShowActiveOnly 
                    ? ProductGroups.Where(pg => pg.Status == "Active")
                    : ProductGroups)
                .Where(pg => 
                    (pg.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (pg.NameAr?.ToLower().Contains(searchLower) ?? false) ||
                    (pg.SkuPrefix?.ToLower().Contains(searchLower) ?? false) ||
                    (pg.Description?.ToLower().Contains(searchLower) ?? false))
            );
        }

        OnPropertyChanged(nameof(HasProductGroups));
        OnPropertyChanged(nameof(TotalProductGroups));
        UpdateStatusMessage();
    }

    private void UpdateStatusMessage()
    {
        if (IsLoading)
        {
            StatusMessage = "Loading product groups...";
        }
        else if (!HasProductGroups && !string.IsNullOrWhiteSpace(SearchText))
        {
            StatusMessage = "No product groups match your search criteria";
        }
        else if (!HasProductGroups && ShowActiveOnly)
        {
            StatusMessage = "No active product groups found";
        }
        else if (!HasProductGroups)
        {
            StatusMessage = "No product groups found";
        }
        else
        {
            StatusMessage = $"Showing {FilteredProductGroups.Count} of {TotalProductGroups} product groups";
        }
    }

    /// <summary>
    /// Load all product groups from database
    /// </summary>
    [RelayCommand]
    private async Task LoadProductGroupsAsync()
    {
        try
        {
            IsLoading = true;
            UpdateStatusMessage();
            
            var groups = await _productGroupService.GetAllAsync();
            ProductGroups = new ObservableCollection<ProductGroupDto>(groups);
            FilterProductGroups();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading product groups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error loading product groups";
        }
        finally
        {
            IsLoading = false;
            UpdateStatusMessage();
        }
    }

    /// <summary>
    /// Toggle the active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
    }

    /// <summary>
    /// Clear all filters and search text
    /// </summary>
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
    }

    /// <summary>
    /// Toggle active status of a product group
    /// </summary>
    [RelayCommand]
    private async Task ToggleActiveAsync(ProductGroupDto productGroup)
    {
        if (productGroup == null)
            return;

        try
        {
            // Load full details
            var details = await _productGroupService.GetDetailByIdAsync(productGroup.Id);
            if (details != null)
            {
                // Create update DTO
                var updateDto = new UpdateProductGroupDto
                {
                    Id = details.Id,
                    Name = details.Name,
                    NameAr = details.NameAr,
                    Description = details.Description,
                    DescriptionAr = details.DescriptionAr,
                    DiscountId = details.DiscountId,
                    TaxTypeId = details.TaxTypeId,
                    PriceTypeId = details.PriceTypeId,
                    SkuPrefix = details.SkuPrefix,
                    Status = details.IsActive ? "Inactive" : "Active"  // Toggle using IsActive property
                };
                
                var result = await _productGroupService.UpdateAsync(updateDto);
                
                if (result != null)
                {
                    await LoadProductGroupsAsync();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error toggling product group status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// View product group details (placeholder for future implementation)
    /// </summary>
    [RelayCommand]
    private void ViewProductGroupDetails(ProductGroupDto productGroup)
    {
        if (productGroup == null)
            return;

        MessageBox.Show($"Product Group Details:\n\nName: {productGroup.Name}\nSKU Prefix: {productGroup.SkuPrefix}\nProducts: {productGroup.ItemCount}",
            "Product Group Details",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Show side panel for adding a new product group
    /// </summary>
    [RelayCommand]
    private void AddProductGroup()
    {
        if (SidePanelViewModel != null)
        {
            SidePanelViewModel.PrepareForAdd();
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Show side panel for editing selected product group
    /// </summary>
    [RelayCommand]
    private async Task EditProductGroupAsync()
    {
        if (SelectedProductGroup == null || SidePanelViewModel == null)
            return;

        try
        {
            // Load full details
            var details = await _productGroupService.GetDetailByIdAsync(SelectedProductGroup.Id);
            if (details != null)
            {
                SidePanelViewModel.PrepareForEdit(details);
                IsSidePanelVisible = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading product group details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Delete selected product group
    /// </summary>
    [RelayCommand]
    private async Task DeleteProductGroupAsync()
    {
        if (SelectedProductGroup == null)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete product group '{SelectedProductGroup.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var success = await _productGroupService.DeleteAsync(SelectedProductGroup.Id);
                if (success)
                {
                    await LoadProductGroupsAsync();
                    MessageBox.Show("Product group deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting product group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Close the side panel
    /// </summary>
    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
    }

    /// <summary>
    /// Go back to previous view
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }
}
