using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Threading.Tasks;
using System;

namespace ChronoPos.Desktop.ViewModels;

public partial class SupplierSidePanelViewModel : ObservableObject
{
    private readonly ISupplierService _supplierService;
    private readonly Action _closeSidePanel;
    private readonly Func<Task> _refreshParent;

    [ObservableProperty]
    private SupplierDto _editingSupplier = new();

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    /// <summary>
    /// Title for the side panel form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Supplier" : "Add Supplier";

    public SupplierSidePanelViewModel(
        ISupplierService supplierService,
        Action closeSidePanel,
        Func<Task> refreshParent)
    {
        _supplierService = supplierService;
        _closeSidePanel = closeSidePanel;
        _refreshParent = refreshParent;

        // Initialize new supplier
        ResetForm();
    }

    /// <summary>
    /// Initialize form for adding a new supplier
    /// </summary>
    public void InitializeForAdd()
    {
        IsEditMode = false;
        ResetForm();
        ClearValidation();
    }

    /// <summary>
    /// Initialize form for editing an existing supplier
    /// </summary>
    public void InitializeForEdit(SupplierDto supplier)
    {
        IsEditMode = true;
        EditingSupplier = new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            CompanyName = supplier.CompanyName,
            ContactName = supplier.ContactName,
            Email = supplier.Email,
            PhoneNumber = supplier.PhoneNumber,
            Address = supplier.Address,
            City = supplier.City,
            Country = supplier.Country,
            VatTrnNumber = supplier.VatTrnNumber,
            Website = supplier.Website,
            CreditLimit = supplier.CreditLimit,
            PaymentTerms = supplier.PaymentTerms,
            IsActive = supplier.IsActive
        };
        ClearValidation();
    }

    /// <summary>
    /// Reset form to default state
    /// </summary>
    private void ResetForm()
    {
        EditingSupplier = new SupplierDto
        {
            CompanyName = string.Empty,
            ContactName = string.Empty,
            Email = string.Empty,
            PhoneNumber = string.Empty,
            Address = string.Empty,
            City = string.Empty,
            Country = string.Empty,
            VatTrnNumber = string.Empty,
            Website = string.Empty,
            CreditLimit = 0,
            PaymentTerms = string.Empty,
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
    /// Validate the current supplier data
    /// </summary>
    private bool ValidateSupplier()
    {
        ClearValidation();

        if (string.IsNullOrWhiteSpace(EditingSupplier.CompanyName))
        {
            SetValidationError("Company name is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EditingSupplier.ContactName))
        {
            SetValidationError("Contact name is required.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(EditingSupplier.Email))
        {
            // Basic email validation
            if (!EditingSupplier.Email.Contains("@") || !EditingSupplier.Email.Contains("."))
            {
                SetValidationError("Please enter a valid email address.");
                return false;
            }
        }

        if (EditingSupplier.CreditLimit < 0)
        {
            SetValidationError("Credit limit cannot be negative.");
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
    /// Close the side panel
    /// </summary>
    [RelayCommand]
    private void CloseSidePanel()
    {
        _closeSidePanel?.Invoke();
    }

    /// <summary>
    /// Save the supplier (add or update)
    /// </summary>
    [RelayCommand]
    private async Task SaveSupplier()
    {
        if (!ValidateSupplier())
            return;

        try
        {
            if (IsEditMode)
            {
                await _supplierService.UpdateSupplierAsync(EditingSupplier);
            }
            else
            {
                await _supplierService.CreateSupplierAsync(EditingSupplier);
            }

            // Refresh parent view
            await _refreshParent?.Invoke();

            // Close side panel
            CloseSidePanel();
        }
        catch (Exception ex)
        {
            SetValidationError($"Error saving supplier: {ex.Message}");
        }
    }
}