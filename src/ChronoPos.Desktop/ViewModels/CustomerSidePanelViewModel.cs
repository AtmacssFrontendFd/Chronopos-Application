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

    /// <summary>
    /// Title for the side panel form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Customer" : "Add Customer";

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
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address,
            IsActive = customer.IsActive
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
            FirstName = string.Empty,
            LastName = string.Empty,
            Email = string.Empty,
            PhoneNumber = string.Empty,
            Address = string.Empty,
            IsActive = true
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

        if (string.IsNullOrWhiteSpace(EditingCustomer.FirstName))
        {
            SetValidationError("First name is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EditingCustomer.LastName))
        {
            SetValidationError("Last name is required.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(EditingCustomer.Email))
        {
            // Basic email validation
            if (!EditingCustomer.Email.Contains("@") || !EditingCustomer.Email.Contains("."))
            {
                SetValidationError("Please enter a valid email address.");
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