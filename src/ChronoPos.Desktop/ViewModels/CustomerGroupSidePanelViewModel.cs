using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Collections.ObjectModel;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Wrapper class for customer with selection state
/// </summary>
public partial class SelectableCustomer : ObservableObject
{
    public CustomerDto Customer { get; set; } = new();
    
    [ObservableProperty]
    private bool _isSelected;

    public int Id => Customer.Id;
    public string DisplayName => Customer.DisplayName;
    public string MobileNo => Customer.MobileNo ?? "-";
}

/// <summary>
/// ViewModel for Customer Group side panel (Add/Edit)
/// </summary>
public partial class CustomerGroupSidePanelViewModel : ObservableObject
{
    private readonly ICustomerGroupService _customerGroupService;
    private readonly ICustomerGroupRelationService _customerGroupRelationService;
    private readonly ICustomerService _customerService;
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

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private ObservableCollection<SelectableCustomer> _allCustomers = new();

    [ObservableProperty]
    private ObservableCollection<SelectableCustomer> _filteredCustomers = new();

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    [ObservableProperty]
    private int _selectedCustomersCount = 0;

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
        ICustomerGroupRelationService customerGroupRelationService,
        ICustomerService customerService,
        ISellingPriceTypeService sellingPriceTypeService,
        IDiscountService discountService,
        Action closeSidePanel,
        Func<Task> refreshParent)
    {
        _customerGroupService = customerGroupService;
        _customerGroupRelationService = customerGroupRelationService;
        _customerService = customerService;
        _sellingPriceTypeService = sellingPriceTypeService;
        _discountService = discountService;
        _closeSidePanel = closeSidePanel;
        _refreshParent = refreshParent;

        // Initialize new customer group
        ResetForm();
        
        // Load dropdowns
        _ = LoadSellingPriceTypesAsync();
        _ = LoadDiscountsAsync();
        _ = LoadCustomersAsync();
    }

    /// <summary>
    /// Load all customers
    /// </summary>
    private async Task LoadCustomersAsync()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            AllCustomers = new ObservableCollection<SelectableCustomer>(
                customers.Select(c => new SelectableCustomer { Customer = c })
            );
            FilterCustomers();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading customers: {ex.Message}");
            AllCustomers = new ObservableCollection<SelectableCustomer>();
        }
    }

    /// <summary>
    /// Filter customers based on search text
    /// </summary>
    private void FilterCustomers()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            FilteredCustomers = new ObservableCollection<SelectableCustomer>(AllCustomers);
        }
        else
        {
            var searchTerm = CustomerSearchText.ToLower();
            FilteredCustomers = new ObservableCollection<SelectableCustomer>(
                AllCustomers.Where(c => 
                    c.DisplayName.ToLower().Contains(searchTerm) ||
                    (c.MobileNo?.ToLower().Contains(searchTerm) ?? false)
                )
            );
        }
        UpdateSelectedCount();
    }

    /// <summary>
    /// Handle search text change
    /// </summary>
    partial void OnCustomerSearchTextChanged(string value)
    {
        FilterCustomers();
    }

    /// <summary>
    /// Toggle customer selection
    /// </summary>
    [RelayCommand]
    private void ToggleCustomerSelection(SelectableCustomer customer)
    {
        customer.IsSelected = !customer.IsSelected;
        UpdateSelectedCount();
    }

    /// <summary>
    /// Select all customers
    /// </summary>
    [RelayCommand]
    private void SelectAllCustomers()
    {
        foreach (var customer in FilteredCustomers)
        {
            customer.IsSelected = true;
        }
        UpdateSelectedCount();
    }

    /// <summary>
    /// Clear all customer selections
    /// </summary>
    [RelayCommand]
    private void ClearAllCustomers()
    {
        foreach (var customer in AllCustomers)
        {
            customer.IsSelected = false;
        }
        UpdateSelectedCount();
    }

    /// <summary>
    /// Update selected customers count
    /// </summary>
    private void UpdateSelectedCount()
    {
        SelectedCustomersCount = AllCustomers.Count(c => c.IsSelected);
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
    public async void OpenForEdit(CustomerGroupDto customerGroup)
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

        // Load existing customer relations
        await LoadExistingCustomerRelationsAsync(customerGroup.Id);

        ClearValidation();
        OnPropertyChanged(nameof(FormTitle));
        OnPropertyChanged(nameof(SaveButtonText));
    }

    /// <summary>
    /// Load existing customer relations for editing
    /// </summary>
    private async Task LoadExistingCustomerRelationsAsync(int customerGroupId)
    {
        try
        {
            var relations = await _customerGroupRelationService.GetByCustomerGroupIdAsync(customerGroupId);
            var relatedCustomerIds = relations.Select(r => r.CustomerId ?? 0).ToHashSet();

            // Mark customers as selected if they're in the group
            foreach (var customer in AllCustomers)
            {
                customer.IsSelected = relatedCustomerIds.Contains(customer.Id);
            }
            UpdateSelectedCount();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading customer relations: {ex.Message}");
        }
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
            int customerGroupId;

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
                customerGroupId = EditingCustomerGroup.Id;

                // Update customer relations
                await UpdateCustomerRelationsAsync(customerGroupId);

                new MessageDialog("Success", "Customer group updated successfully!", MessageDialog.MessageType.Success).ShowDialog();
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

                var created = await _customerGroupService.CreateAsync(createDto);
                customerGroupId = created.Id;

                // Create customer relations
                await CreateCustomerRelationsAsync(customerGroupId);

                new MessageDialog("Success", "Customer group created successfully!", MessageDialog.MessageType.Success).ShowDialog();
            }

            // Refresh parent list
            await _refreshParent();

            // Close side panel
            _closeSidePanel();
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error saving customer group: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    /// <summary>
    /// Create customer relations for new customer group
    /// </summary>
    private async Task CreateCustomerRelationsAsync(int customerGroupId)
    {
        var selectedCustomers = AllCustomers.Where(c => c.IsSelected).ToList();
        
        foreach (var customer in selectedCustomers)
        {
            try
            {
                var relationDto = new CreateCustomerGroupRelationDto
                {
                    CustomerId = customer.Id,
                    CustomerGroupId = customerGroupId,
                    Status = "Active"
                };

                await _customerGroupRelationService.CreateAsync(relationDto);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating relation for customer {customer.Id}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Update customer relations for existing customer group
    /// </summary>
    private async Task UpdateCustomerRelationsAsync(int customerGroupId)
    {
        try
        {
            // Get existing relations
            var existingRelations = await _customerGroupRelationService.GetByCustomerGroupIdAsync(customerGroupId);
            var existingCustomerIds = existingRelations.Select(r => r.CustomerId ?? 0).ToHashSet();
            var selectedCustomerIds = AllCustomers.Where(c => c.IsSelected).Select(c => c.Id).ToHashSet();

            // Remove unselected customers
            foreach (var relation in existingRelations)
            {
                if (relation.CustomerId.HasValue && !selectedCustomerIds.Contains(relation.CustomerId.Value))
                {
                    await _customerGroupRelationService.DeleteAsync(relation.Id);
                }
            }

            // Add newly selected customers
            foreach (var customerId in selectedCustomerIds)
            {
                if (!existingCustomerIds.Contains(customerId))
                {
                    var relationDto = new CreateCustomerGroupRelationDto
                    {
                        CustomerId = customerId,
                        CustomerGroupId = customerGroupId,
                        Status = "Active"
                    };

                    await _customerGroupRelationService.CreateAsync(relationDto);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating customer relations: {ex.Message}");
            throw;
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
