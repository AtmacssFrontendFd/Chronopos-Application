using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Product Group side panel (Add/Edit)
/// </summary>
public partial class ProductGroupSidePanelViewModel : ObservableObject
{
    private readonly IProductGroupService _productGroupService;
    private readonly IProductGroupItemService _productGroupItemService;
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IProductService _productService;
    private readonly IProductUnitService _productUnitService;
    private readonly Action _closeSidePanel;
    private readonly Func<Task> _refreshParent;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _nameAr = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _descriptionAr = string.Empty;

    [ObservableProperty]
    private string _skuPrefix = string.Empty;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private List<DiscountDto> _discounts = new();

    [ObservableProperty]
    private List<TaxTypeDto> _taxTypes = new();

    [ObservableProperty]
    private List<SellingPriceTypeDto> _priceTypes = new();

    [ObservableProperty]
    private DiscountDto? _selectedDiscount;

    [ObservableProperty]
    private TaxTypeDto? _selectedTaxType;

    [ObservableProperty]
    private SellingPriceTypeDto? _selectedPriceType;

    [ObservableProperty]
    private ObservableCollection<ProductGroupItemDto> _groupItems = new();

    [ObservableProperty]
    private ProductGroupItemDto? _selectedGroupItem;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    // Tab management
    [ObservableProperty]
    private int _selectedTabIndex = 0;

    // Add Items tab properties
    [ObservableProperty]
    private List<ProductDto> _products = new();

    [ObservableProperty]
    private List<ProductUnitDto> _productUnits = new();

    [ObservableProperty]
    private ProductDto? _selectedProduct;

    [ObservableProperty]
    private ProductUnitDto? _selectedProductUnit;

    // Multi-selection collections
    [ObservableProperty]
    private ObservableCollection<ProductDto> _selectedProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductUnitDto> _selectedProductUnits = new();

    /// <summary>
    /// Title for the side panel form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Product Group" : "Add Product Group";

    /// <summary>
    /// Save button text
    /// </summary>
    public string SaveButtonText => IsEditMode ? "Update" : "Create";

    /// <summary>
    /// Status for the group
    /// </summary>
    public string Status => IsActive ? "Active" : "Inactive";

    /// <summary>
    /// Check if Basic Info tab is selected
    /// </summary>
    public bool IsBasicInfoTabSelected => SelectedTabIndex == 0;

    /// <summary>
    /// Check if Add Items tab is selected
    /// </summary>
    public bool IsAddItemsTabSelected => SelectedTabIndex == 1;

    /// <summary>
    /// Check if user has made any selection for item creation
    /// </summary>
    public bool HasItemSelection => SelectedProducts.Count > 0 || SelectedProductUnits.Count > 0;

    /// <summary>
    /// Preview text for item creation
    /// </summary>
    public string ItemCreationPreview
    {
        get
        {
            var parts = new List<string>();
            
            if (SelectedProducts.Count > 0)
            {
                parts.Add($"{SelectedProducts.Count} product(s) selected");
            }
            
            if (SelectedProductUnits.Count > 0)
            {
                parts.Add($"{SelectedProductUnits.Count} unit(s) selected");
            }
            
            if (parts.Count == 0)
                return string.Empty;
                
            return string.Join(" and ", parts);
        }
    }

    public ProductGroupSidePanelViewModel(
        IProductGroupService productGroupService,
        IProductGroupItemService productGroupItemService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        ISellingPriceTypeService sellingPriceTypeService,
        IProductService productService,
        IProductUnitService productUnitService,
        Action closeSidePanel,
        Func<Task> refreshParent)
    {
        _productGroupService = productGroupService;
        _productGroupItemService = productGroupItemService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _productService = productService;
        _productUnitService = productUnitService;
        _closeSidePanel = closeSidePanel;
        _refreshParent = refreshParent;

        // Load dropdowns
        _ = LoadDropdownsAsync();
    }

    partial void OnSelectedProductChanged(ProductDto? value)
    {
        OnPropertyChanged(nameof(HasItemSelection));
        OnPropertyChanged(nameof(ItemCreationPreview));
    }

    partial void OnSelectedProductUnitChanged(ProductUnitDto? value)
    {
        OnPropertyChanged(nameof(HasItemSelection));
        OnPropertyChanged(nameof(ItemCreationPreview));
    }

    partial void OnSelectedProductsChanged(ObservableCollection<ProductDto> value)
    {
        OnPropertyChanged(nameof(HasItemSelection));
        OnPropertyChanged(nameof(ItemCreationPreview));
    }

    partial void OnSelectedProductUnitsChanged(ObservableCollection<ProductUnitDto> value)
    {
        OnPropertyChanged(nameof(HasItemSelection));
        OnPropertyChanged(nameof(ItemCreationPreview));
    }

    partial void OnSelectedTabIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsBasicInfoTabSelected));
        OnPropertyChanged(nameof(IsAddItemsTabSelected));
    }

    /// <summary>
    /// Add selected product to the list
    /// </summary>
    [RelayCommand]
    private void AddProduct()
    {
        if (SelectedProduct != null && !SelectedProducts.Any(p => p.Id == SelectedProduct.Id))
        {
            SelectedProducts.Add(SelectedProduct);
            SelectedProduct = null; // Clear selection
            OnPropertyChanged(nameof(HasItemSelection));
            OnPropertyChanged(nameof(ItemCreationPreview));
        }
    }

    /// <summary>
    /// Remove product from the list
    /// </summary>
    [RelayCommand]
    private void RemoveProduct(ProductDto product)
    {
        if (product != null)
        {
            SelectedProducts.Remove(product);
            OnPropertyChanged(nameof(HasItemSelection));
            OnPropertyChanged(nameof(ItemCreationPreview));
        }
    }

    /// <summary>
    /// Add selected product unit to the list
    /// </summary>
    [RelayCommand]
    private void AddProductUnit()
    {
        if (SelectedProductUnit != null && !SelectedProductUnits.Any(u => u.Id == SelectedProductUnit.Id))
        {
            SelectedProductUnits.Add(SelectedProductUnit);
            SelectedProductUnit = null; // Clear selection
            OnPropertyChanged(nameof(HasItemSelection));
            OnPropertyChanged(nameof(ItemCreationPreview));
        }
    }

    /// <summary>
    /// Remove product unit from the list
    /// </summary>
    [RelayCommand]
    private void RemoveProductUnit(ProductUnitDto unit)
    {
        if (unit != null)
        {
            SelectedProductUnits.Remove(unit);
            OnPropertyChanged(nameof(HasItemSelection));
            OnPropertyChanged(nameof(ItemCreationPreview));
        }
    }

    /// <summary>
    /// Load all dropdowns
    /// </summary>
    private async Task LoadDropdownsAsync()
    {
        try
        {
            var discountsTask = _discountService.GetAllAsync();
            var taxTypesTask = _taxTypeService.GetAllAsync();
            var priceTypesTask = _sellingPriceTypeService.GetAllAsync();
            var productsTask = _productService.GetAllProductsAsync();
            var productUnitsTask = _productUnitService.GetAllAsync();

            await Task.WhenAll(discountsTask, taxTypesTask, priceTypesTask, productsTask, productUnitsTask);

            Discounts = discountsTask.Result.ToList();
            TaxTypes = taxTypesTask.Result.ToList();
            PriceTypes = priceTypesTask.Result.ToList();
            Products = productsTask.Result.ToList();
            ProductUnits = productUnitsTask.Result.ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dropdowns: {ex.Message}");
        }
    }

    /// <summary>
    /// Select tab command
    /// </summary>
    [RelayCommand]
    private void SelectTab(string tabIndex)
    {
        if (int.TryParse(tabIndex, out int index))
        {
            SelectedTabIndex = index;
        }
    }

    /// <summary>
    /// Prepare form for adding a new product group
    /// </summary>
    public void PrepareForAdd()
    {
        IsEditMode = false;
        SelectedTabIndex = 0;
        ResetForm();
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    /// <summary>
    /// Prepare form for editing an existing product group
    /// </summary>
    public void PrepareForEdit(ProductGroupDetailDto details)
    {
        IsEditMode = true;
        SelectedTabIndex = 0;
        Id = details.Id;
        Name = details.Name ?? string.Empty;
        NameAr = details.NameAr ?? string.Empty;
        Description = details.Description ?? string.Empty;
        DescriptionAr = details.DescriptionAr ?? string.Empty;
        SkuPrefix = details.SkuPrefix ?? string.Empty;
        IsActive = details.Status == "Active";

        // Set selected items
        SelectedDiscount = Discounts.FirstOrDefault(d => d.Id == details.DiscountId);
        SelectedTaxType = TaxTypes.FirstOrDefault(t => t.Id == details.TaxTypeId);
        SelectedPriceType = PriceTypes.FirstOrDefault(p => p.Id == details.PriceTypeId);

        // Clear Add Items tab selections
        SelectedProduct = null;
        SelectedProductUnit = null;
        SelectedProducts.Clear();
        SelectedProductUnits.Clear();

        // Load items
        GroupItems = new ObservableCollection<ProductGroupItemDto>(details.Items ?? new List<ProductGroupItemDto>());

        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        ClearValidation();
    }

    /// <summary>
    /// Save product group (Create or Update)
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateForm())
            return;

        try
        {
            int groupId = Id;

            if (IsEditMode)
            {
                var dto = new UpdateProductGroupDto
                {
                    Id = Id,
                    Name = Name.Trim(),
                    NameAr = string.IsNullOrWhiteSpace(NameAr) ? null : NameAr.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    DescriptionAr = string.IsNullOrWhiteSpace(DescriptionAr) ? null : DescriptionAr.Trim(),
                    DiscountId = SelectedDiscount?.Id,
                    TaxTypeId = SelectedTaxType?.Id,
                    PriceTypeId = SelectedPriceType?.Id,
                    SkuPrefix = string.IsNullOrWhiteSpace(SkuPrefix) ? null : SkuPrefix.Trim(),
                    Status = Status
                };

                await _productGroupService.UpdateAsync(dto);
                MessageBox.Show("Product group updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var dto = new CreateProductGroupDto
                {
                    Name = Name.Trim(),
                    NameAr = string.IsNullOrWhiteSpace(NameAr) ? null : NameAr.Trim(),
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                    DescriptionAr = string.IsNullOrWhiteSpace(DescriptionAr) ? null : DescriptionAr.Trim(),
                    DiscountId = SelectedDiscount?.Id,
                    TaxTypeId = SelectedTaxType?.Id,
                    PriceTypeId = SelectedPriceType?.Id,
                    SkuPrefix = string.IsNullOrWhiteSpace(SkuPrefix) ? null : SkuPrefix.Trim(),
                    Status = Status
                };

                var createdGroup = await _productGroupService.CreateAsync(dto);
                groupId = createdGroup.Id;
                MessageBox.Show("Product group created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Create product group items based on selection
            await CreateProductGroupItemsAsync(groupId);

            await _refreshParent();
            _closeSidePanel();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving product group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Create product group items based on user selection
    /// </summary>
    private async Task CreateProductGroupItemsAsync(int groupId)
    {
        if (SelectedProducts.Count == 0 && SelectedProductUnits.Count == 0)
            return;

        try
        {
            List<int> productIdsToAdd = new();

            // Add selected products
            if (SelectedProducts.Count > 0)
            {
                productIdsToAdd.AddRange(SelectedProducts.Select(p => p.Id));
            }

            // Add products based on selected units
            if (SelectedProductUnits.Count > 0)
            {
                var allProducts = await _productService.GetAllProductsAsync();
                var productsByUnit = allProducts
                    .Where(p => p.ProductUnits.Any(pu => SelectedProductUnits.Any(su => su.Id == pu.Id)))
                    .Select(p => p.Id)
                    .ToList();
                
                // Add products that aren't already in the list
                foreach (var productId in productsByUnit)
                {
                    if (!productIdsToAdd.Contains(productId))
                    {
                        productIdsToAdd.Add(productId);
                    }
                }
            }

            // Remove duplicates
            productIdsToAdd = productIdsToAdd.Distinct().ToList();

            // Create product group items
            int itemsCreated = 0;
            foreach (var productId in productIdsToAdd)
            {
                // If units are selected, create an item for each unit combination
                if (SelectedProductUnits.Count > 0)
                {
                    foreach (var unit in SelectedProductUnits)
                    {
                        var itemDto = new CreateProductGroupItemDto
                        {
                            ProductGroupId = groupId,
                            ProductId = productId,
                            ProductUnitId = unit.Id,
                            Quantity = 1,
                            PriceAdjustment = 0,
                            DiscountId = SelectedDiscount?.Id,
                            TaxTypeId = SelectedTaxType?.Id,
                            SellingPriceTypeId = SelectedPriceType?.Id,
                            Status = "Active"
                        };

                        await _productGroupItemService.CreateAsync(itemDto);
                        itemsCreated++;
                    }
                }
                else
                {
                    // No units selected, create item with null unit
                    var itemDto = new CreateProductGroupItemDto
                    {
                        ProductGroupId = groupId,
                        ProductId = productId,
                        ProductUnitId = null,
                        Quantity = 1,
                        PriceAdjustment = 0,
                        DiscountId = SelectedDiscount?.Id,
                        TaxTypeId = SelectedTaxType?.Id,
                        SellingPriceTypeId = SelectedPriceType?.Id,
                        Status = "Active"
                    };

                    await _productGroupItemService.CreateAsync(itemDto);
                    itemsCreated++;
                }
            }

            if (itemsCreated > 0)
            {
                MessageBox.Show($"{itemsCreated} product group item(s) created successfully.", 
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating product group items: {ex.Message}");
            MessageBox.Show($"Product group created but error adding items: {ex.Message}", 
                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Cancel and close side panel
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _closeSidePanel();
    }

    /// <summary>
    /// Remove selected item from group
    /// </summary>
    [RelayCommand]
    private async Task RemoveItemAsync()
    {
        if (SelectedGroupItem == null)
            return;

        if (!IsEditMode)
        {
            // If not saved yet, just remove from list
            GroupItems.Remove(SelectedGroupItem);
            return;
        }

        var result = MessageBox.Show(
            "Are you sure you want to remove this item from the group?",
            "Confirm Remove",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _productGroupService.RemoveItemFromGroupAsync(SelectedGroupItem.Id);
                GroupItems.Remove(SelectedGroupItem);
                MessageBox.Show("Item removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Validate form fields
    /// </summary>
    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowValidationError("Name is required.");
            return false;
        }

        ClearValidation();
        return true;
    }

    /// <summary>
    /// Show validation error
    /// </summary>
    private void ShowValidationError(string message)
    {
        HasValidationErrors = true;
        ValidationMessage = message;
    }

    /// <summary>
    /// Clear validation errors
    /// </summary>
    private void ClearValidation()
    {
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
    }

    /// <summary>
    /// Reset form to default values
    /// </summary>
    private void ResetForm()
    {
        Id = 0;
        Name = string.Empty;
        NameAr = string.Empty;
        Description = string.Empty;
        DescriptionAr = string.Empty;
        SkuPrefix = string.Empty;
        IsActive = true;
        SelectedDiscount = null;
        SelectedTaxType = null;
        SelectedPriceType = null;
        SelectedProduct = null;
        SelectedProductUnit = null;
        SelectedProducts.Clear();
        SelectedProductUnits.Clear();
        GroupItems.Clear();
        ClearValidation();
    }
}
