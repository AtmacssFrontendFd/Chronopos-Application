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
    private readonly IDiscountService _discountService;
    private readonly ITaxTypeService _taxTypeService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IProductService _productService;
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

    public ProductGroupSidePanelViewModel(
        IProductGroupService productGroupService,
        IDiscountService discountService,
        ITaxTypeService taxTypeService,
        ISellingPriceTypeService sellingPriceTypeService,
        IProductService productService,
        Action closeSidePanel,
        Func<Task> refreshParent)
    {
        _productGroupService = productGroupService;
        _discountService = discountService;
        _taxTypeService = taxTypeService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _productService = productService;
        _closeSidePanel = closeSidePanel;
        _refreshParent = refreshParent;

        // Load dropdowns
        _ = LoadDropdownsAsync();
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

            await Task.WhenAll(discountsTask, taxTypesTask, priceTypesTask);

            Discounts = discountsTask.Result.ToList();
            TaxTypes = taxTypesTask.Result.ToList();
            PriceTypes = priceTypesTask.Result.ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading dropdowns: {ex.Message}");
        }
    }

    /// <summary>
    /// Prepare form for adding a new product group
    /// </summary>
    public void PrepareForAdd()
    {
        IsEditMode = false;
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

                await _productGroupService.CreateAsync(dto);
                MessageBox.Show("Product group created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            await _refreshParent();
            _closeSidePanel();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving product group: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        GroupItems.Clear();
        ClearValidation();
    }
}
