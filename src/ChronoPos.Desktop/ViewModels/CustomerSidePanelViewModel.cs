using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Threading.Tasks;
using System;

namespace ChronoPos.Desktop.ViewModels;

public partial class CustomerSidePanelViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly Action _closeSidePanel;
    private readonly Func<Task> _refreshParent;

    [ObservableProperty]
    private CustomerDto _editingCustomer = new();

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private List<BusinessTypeDto> _businessTypes = new();

    /// <summary>
    /// Title for the side panel form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Customer" : "Add Customer";

    /// <summary>
    /// Property for individual customer radio button binding
    /// </summary>
    public bool IsIndividualCustomer
    {
        get => !EditingCustomer.IsBusiness;
        set
        {
            if (value && EditingCustomer.IsBusiness)
            {
                EditingCustomer.IsBusiness = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditingCustomer));
            }
        }
    }

    /// <summary>
    /// Handle when editing customer IsBusiness property changes
    /// </summary>
    partial void OnEditingCustomerChanged(CustomerDto value)
    {
        OnPropertyChanged(nameof(IsIndividualCustomer));
    }

    public CustomerSidePanelViewModel(
        ICustomerService customerService,
        Action closeSidePanel,
        Func<Task> refreshParent)
    {
        _customerService = customerService;
        _closeSidePanel = closeSidePanel;
        _refreshParent = refreshParent;

        // Initialize new customer
        ResetForm();
        
        // Initialize business types (for now, add some sample data)
        InitializeBusinessTypes();
    }

    /// <summary>
    /// Initialize business types for the dropdown
    /// </summary>
    private void InitializeBusinessTypes()
    {
        BusinessTypes = new List<BusinessTypeDto>
        {
            new BusinessTypeDto { Id = 1, BusinessTypeName = "Corporation", BusinessTypeNameAr = "شركة" },
            new BusinessTypeDto { Id = 2, BusinessTypeName = "Partnership", BusinessTypeNameAr = "شراكة" },
            new BusinessTypeDto { Id = 3, BusinessTypeName = "Sole Proprietorship", BusinessTypeNameAr = "ملكية فردية" },
            new BusinessTypeDto { Id = 4, BusinessTypeName = "LLC", BusinessTypeNameAr = "ذات مسؤولية محدودة" },
            new BusinessTypeDto { Id = 5, BusinessTypeName = "Non-Profit", BusinessTypeNameAr = "غير ربحية" }
        };
    }

    /// <summary>
    /// Initialize form for adding a new customer
    /// </summary>
    public void InitializeForAdd()
    {
        IsEditMode = false;
        ResetForm();
        ClearValidation();
    }

    /// <summary>
    /// Initialize form for editing an existing customer
    /// </summary>
    public void InitializeForEdit(CustomerDto customer)
    {
        IsEditMode = true;
        EditingCustomer = new CustomerDto
        {
            Id = customer.Id,
            CustomerFullName = customer.CustomerFullName,
            BusinessFullName = customer.BusinessFullName,
            IsBusiness = customer.IsBusiness,
            BusinessTypeId = customer.BusinessTypeId,
            CustomerBalanceAmount = customer.CustomerBalanceAmount,
            LicenseNo = customer.LicenseNo,
            TrnNo = customer.TrnNo,
            MobileNo = customer.MobileNo,
            HomePhone = customer.HomePhone,
            OfficePhone = customer.OfficePhone,
            ContactMobileNo = customer.ContactMobileNo,
            OfficialEmail = customer.OfficialEmail,
            CreditAllowed = customer.CreditAllowed,
            CreditAmountMax = customer.CreditAmountMax,
            CreditDays = customer.CreditDays,
            CreditReference1Name = customer.CreditReference1Name,
            CreditReference2Name = customer.CreditReference2Name,
            KeyContactName = customer.KeyContactName,
            KeyContactMobile = customer.KeyContactMobile,
            KeyContactEmail = customer.KeyContactEmail,
            FinancePersonName = customer.FinancePersonName,
            FinancePersonMobile = customer.FinancePersonMobile,
            FinancePersonEmail = customer.FinancePersonEmail,
            PostDatedChequesAllowed = customer.PostDatedChequesAllowed,
            ShopId = customer.ShopId,
            Status = customer.Status
        };
        ClearValidation();
    }

    /// <summary>
    /// Reset form to default state
    /// </summary>
    private void ResetForm()
    {
        EditingCustomer = new CustomerDto
        {
            CustomerFullName = string.Empty,
            BusinessFullName = string.Empty,
            IsBusiness = false,
            MobileNo = string.Empty,
            HomePhone = string.Empty,
            OfficePhone = string.Empty,
            ContactMobileNo = string.Empty,
            OfficialEmail = string.Empty,
            CreditAllowed = false,
            CreditAmountMax = null,
            CreditDays = null,
            CreditReference1Name = string.Empty,
            CreditReference2Name = string.Empty,
            KeyContactName = string.Empty,
            KeyContactMobile = string.Empty,
            KeyContactEmail = string.Empty,
            FinancePersonName = string.Empty,
            FinancePersonMobile = string.Empty,
            FinancePersonEmail = string.Empty,
            LicenseNo = string.Empty,
            TrnNo = string.Empty,
            PostDatedChequesAllowed = false,
            Status = "Active"
        };
    }

    /// <summary>
    /// Clear validation messages
    /// </summary>
    private void ClearValidation()
    {
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
    }

    /// <summary>
    /// Validate the current customer data
    /// </summary>
    private bool ValidateCustomer()
    {
        ClearValidation();

        // Validate customer type and name
        if (EditingCustomer.IsBusiness)
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.BusinessFullName))
            {
                SetValidationError("Business name is required for business customers.");
                return false;
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.CustomerFullName))
            {
                SetValidationError("Customer name is required for individual customers.");
                return false;
            }
        }

        // Validate mobile number (required)
        if (string.IsNullOrWhiteSpace(EditingCustomer.MobileNo))
        {
            SetValidationError("Mobile number is required.");
            return false;
        }

        // Validate email if provided
        if (!string.IsNullOrWhiteSpace(EditingCustomer.OfficialEmail))
        {
            if (!EditingCustomer.OfficialEmail.Contains("@") || !EditingCustomer.OfficialEmail.Contains("."))
            {
                SetValidationError("Please enter a valid email address.");
                return false;
            }
        }

        // Validate key contact email if provided
        if (!string.IsNullOrWhiteSpace(EditingCustomer.KeyContactEmail))
        {
            if (!EditingCustomer.KeyContactEmail.Contains("@") || !EditingCustomer.KeyContactEmail.Contains("."))
            {
                SetValidationError("Please enter a valid key contact email address.");
                return false;
            }
        }

        // Validate finance person email if provided
        if (!string.IsNullOrWhiteSpace(EditingCustomer.FinancePersonEmail))
        {
            if (!EditingCustomer.FinancePersonEmail.Contains("@") || !EditingCustomer.FinancePersonEmail.Contains("."))
            {
                SetValidationError("Please enter a valid finance person email address.");
                return false;
            }
        }

        // Validate credit settings
        if (EditingCustomer.CreditAllowed)
        {
            if (!EditingCustomer.CreditAmountMax.HasValue || EditingCustomer.CreditAmountMax <= 0)
            {
                SetValidationError("Credit amount must be greater than zero when credit is allowed.");
                return false;
            }
            
            if (!EditingCustomer.CreditDays.HasValue || EditingCustomer.CreditDays <= 0)
            {
                SetValidationError("Credit days must be greater than zero when credit is allowed.");
                return false;
            }
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
    /// Close the side panel
    /// </summary>
    [RelayCommand]
    private void CloseSidePanel()
    {
        _closeSidePanel?.Invoke();
    }

    /// <summary>
    /// Save the customer (add or update)
    /// </summary>
    [RelayCommand]
    private async Task SaveCustomer()
    {
        if (!ValidateCustomer())
            return;

        try
        {
            if (IsEditMode)
            {
                await _customerService.UpdateCustomerAsync(EditingCustomer);
            }
            else
            {
                await _customerService.CreateCustomerAsync(EditingCustomer);
            }

            // Refresh parent view
            await _refreshParent?.Invoke();

            // Close side panel
            CloseSidePanel();
        }
        catch (Exception ex)
        {
            SetValidationError($"Error saving customer: {ex.Message}");
        }
    }
}