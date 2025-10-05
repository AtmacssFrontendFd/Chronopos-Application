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
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IProductService _productService;

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

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Check if there are any product groups to display
    /// </summary>
    public bool HasProductGroups => FilteredProductGroups.Count > 0;

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public ProductGroupsViewModel(
        IProductGroupService productGroupService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        ISellingPriceTypeService sellingPriceTypeService,
        IProductService productService)
    {
        _productGroupService = productGroupService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _productService = productService;
        
        // Initialize side panel view model
        SidePanelViewModel = new ProductGroupSidePanelViewModel(
            _productGroupService,
            _discountService,
            _taxTypeService,
            _sellingPriceTypeService,
            _productService,
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
    }

    /// <summary>
    /// Load all product groups from database
    /// </summary>
    [RelayCommand]
    private async Task LoadProductGroupsAsync()
    {
        try
        {
            var groups = await _productGroupService.GetAllAsync();
            ProductGroups = new ObservableCollection<ProductGroupDto>(groups);
            FilterProductGroups();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading product groups: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
