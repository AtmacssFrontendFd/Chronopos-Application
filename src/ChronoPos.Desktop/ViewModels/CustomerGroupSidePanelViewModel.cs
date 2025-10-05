using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Customer Group side panel (Add/Edit)
/// </summary>
public partial class CustomerGroupSidePanelViewModel : ObservableObject
{
    private readonly ICustomerGroupService _customerGroupService;
    private readonly ISellingPriceTypeService _sellingPriceTypeService;
    private readonly IDiscountService _discountService;
    private readonly Action _closeSidePanel;
    private readonly Func<Task> _refreshParent;

    [ObservableProperty]
    private CustomerGroupDto _editingCustomerGroup = new();

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private List<SellingPriceTypeDto> _sellingPriceTypes = new();

    [ObservableProperty]
    private List<DiscountDto> _discounts = new();

    [ObservableProperty]
    private SellingPriceTypeDto? _selectedSellingPriceType;

    [ObservableProperty]
    private DiscountDto? _selectedDiscount;

    [ObservableProperty]
    private bool _isPercentageDiscount = false;

    [ObservableProperty]
    private bool _isActive = true;

    /// <summary>
    /// Title for the side panel form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Customer Group" : "Add Customer Group";

    /// <summary>
    /// Save button text
    /// </summary>
    public string SaveButtonText => IsEditMode ? "Update" : "Create";

    public CustomerGroupSidePanelViewModel(
        ICustomerGroupService customerGroupService,
        ISellingPriceTypeService sellingPriceTypeService,
        IDiscountService discountService,
        Action closeSidePanel,
        Func<Task> refreshParent)
    {
        _customerGroupService = customerGroupService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _discountService = discountService;
        _closeSidePanel = closeSidePanel;
        _refreshParent = refreshParent;

        // Initialize new customer group
        ResetForm();
        
        // Load dropdowns
        _ = LoadSellingPriceTypesAsync();
        _ = LoadDiscountsAsync();
    }

    /// <summary>
    /// Load selling price types for dropdown
    /// </summary>
    private async Task LoadSellingPriceTypesAsync()
    {
        try
        {
            var priceTypes = await _sellingPriceTypeService.GetAllAsync();
            SellingPriceTypes = priceTypes.ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading selling price types: {ex.Message}");
            SellingPriceTypes = new List<SellingPriceTypeDto>();
        }
    }

    /// <summary>
    /// Load discounts for dropdown
    /// </summary>
    private async Task LoadDiscountsAsync()
    {
        try
        {
            var discounts = await _discountService.GetAllAsync();
            Discounts = discounts.ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading discounts: {ex.Message}");
            Discounts = new List<DiscountDto>();
        }
    }

    /// <summary>
    /// Open side panel for adding new customer group
    /// </summary>
    public void OpenForAdd()
    {
        IsEditMode = false;
        ResetForm();
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    /// <summary>
    /// Open side panel for editing existing customer group
    /// </summary>
    public void OpenForEdit(CustomerGroupDto customerGroup)
    {
        IsEditMode = true;
        EditingCustomerGroup = new CustomerGroupDto
        {
            Id = customerGroup.Id,
            Name = customerGroup.Name,
            NameAr = customerGroup.NameAr,
            SellingPriceTypeId = customerGroup.SellingPriceTypeId,
            DiscountId = customerGroup.DiscountId,
            DiscountValue = customerGroup.DiscountValue,
            DiscountMaxValue = customerGroup.DiscountMaxValue,
            IsPercentage = customerGroup.IsPercentage,
            Status = customerGroup.Status
        };

        // Set the selected items in dropdowns
        SelectedSellingPriceType = SellingPriceTypes.FirstOrDefault(pt => pt.Id == customerGroup.SellingPriceTypeId);
        SelectedDiscount = Discounts.FirstOrDefault(d => d.Id == customerGroup.DiscountId);
        
        IsPercentageDiscount = customerGroup.IsPercentage;
        IsActive = customerGroup.Status == "Active";

        ClearValidation();
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    /// <summary>
    /// Reset form to initial state
    /// </summary>
    private void ResetForm()
    {
        EditingCustomerGroup = new CustomerGroupDto
        {
            Name = string.Empty,
            NameAr = string.Empty,
            Status = "Active",
            IsPercentage = false
        };

        SelectedSellingPriceType = null;
        SelectedDiscount = null;
        IsPercentageDiscount = false;
        IsActive = true;

        ClearValidation();
    }

    /// <summary>
    /// Validate form inputs
    /// </summary>
    private bool ValidateForm()
    {
        // Clear previous validation
        ClearValidation();

        // Check required fields
        if (string.IsNullOrWhiteSpace(EditingCustomerGroup.Name))
        {
            SetValidationError("Group Name is required");
            return false;
        }

        // Validate discount value if provided
        if (EditingCustomerGroup.DiscountValue.HasValue)
        {
            if (IsPercentageDiscount && EditingCustomerGroup.DiscountValue > 100)
            {
                SetValidationError("Percentage discount cannot exceed 100%");
                return false;
            }

            if (EditingCustomerGroup.DiscountValue < 0)
            {
                SetValidationError("Discount value cannot be negative");
                return false;
            }
        }

        // Validate discount max value
        if (EditingCustomerGroup.DiscountMaxValue.HasValue && EditingCustomerGroup.DiscountMaxValue < 0)
        {
            SetValidationError("Discount max value cannot be negative");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Set validation error message
    /// </summary>
    private void SetValidationError(string message)
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
    /// Handle selected selling price type change
    /// </summary>
    partial void OnSelectedSellingPriceTypeChanged(SellingPriceTypeDto? value)
    {
        if (value != null)
        {
            EditingCustomerGroup.SellingPriceTypeId = value.Id;
        }
        else
        {
            EditingCustomerGroup.SellingPriceTypeId = null;
        }
    }

    /// <summary>
    /// Handle selected discount change
    /// </summary>
    partial void OnSelectedDiscountChanged(DiscountDto? value)
    {
        if (value != null)
        {
            EditingCustomerGroup.DiscountId = value.Id;
        }
        else
        {
            EditingCustomerGroup.DiscountId = null;
        }
    }

    /// <summary>
    /// Handle percentage discount toggle
    /// </summary>
    partial void OnIsPercentageDiscountChanged(bool value)
    {
        EditingCustomerGroup.IsPercentage = value;
    }

    /// <summary>
    /// Handle active status toggle
    /// </summary>
    partial void OnIsActiveChanged(bool value)
    {
        EditingCustomerGroup.Status = value ? "Active" : "Inactive";
    }

    /// <summary>
    /// Save customer group (create or update)
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateForm())
        {
            return;
        }

        try
        {
            if (IsEditMode)
            {
                // Update existing customer group
                var updateDto = new UpdateCustomerGroupDto
                {
                    Id = EditingCustomerGroup.Id,
                    Name = EditingCustomerGroup.Name.Trim(),
                    NameAr = string.IsNullOrWhiteSpace(EditingCustomerGroup.NameAr) ? null : EditingCustomerGroup.NameAr.Trim(),
                    SellingPriceTypeId = EditingCustomerGroup.SellingPriceTypeId,
                    DiscountId = EditingCustomerGroup.DiscountId,
                    DiscountValue = EditingCustomerGroup.DiscountValue,
                    DiscountMaxValue = EditingCustomerGroup.DiscountMaxValue,
                    IsPercentage = EditingCustomerGroup.IsPercentage,
                    Status = EditingCustomerGroup.Status
                };

                await _customerGroupService.UpdateAsync(updateDto);
                MessageBox.Show("Customer group updated successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new customer group
                var createDto = new CreateCustomerGroupDto
                {
                    Name = EditingCustomerGroup.Name.Trim(),
                    NameAr = string.IsNullOrWhiteSpace(EditingCustomerGroup.NameAr) ? null : EditingCustomerGroup.NameAr.Trim(),
                    SellingPriceTypeId = EditingCustomerGroup.SellingPriceTypeId,
                    DiscountId = EditingCustomerGroup.DiscountId,
                    DiscountValue = EditingCustomerGroup.DiscountValue,
                    DiscountMaxValue = EditingCustomerGroup.DiscountMaxValue,
                    IsPercentage = EditingCustomerGroup.IsPercentage,
                    Status = EditingCustomerGroup.Status
                };

                await _customerGroupService.CreateAsync(createDto);
                MessageBox.Show("Customer group created successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Refresh parent list
            await _refreshParent();

            // Close side panel
            _closeSidePanel();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving customer group: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
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
}
