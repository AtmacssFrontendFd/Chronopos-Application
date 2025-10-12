using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
using System.Threading.Tasks;
using System;
using Microsoft.Win32;
using System.IO;
using System.Windows;

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

    [ObservableProperty]
    private string _logoFileName = string.Empty;

    /// <summary>
    /// Title for the side panel form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Supplier" : "Add Supplier";

    /// <summary>
    /// Check if supplier has a logo
    /// </summary>
    public bool HasLogo => !string.IsNullOrWhiteSpace(EditingSupplier?.LogoPicture);

    /// <summary>
    /// Check if supplier has location coordinates
    /// </summary>
    public bool HasLocation => EditingSupplier?.LocationLatitude.HasValue == true && 
                               EditingSupplier?.LocationLongitude.HasValue == true;

    /// <summary>
    /// Display text for location coordinates
    /// </summary>
    public string LocationDisplayText => HasLocation 
        ? $"Lat: {EditingSupplier.LocationLatitude:F6}, Long: {EditingSupplier.LocationLongitude:F6}"
        : string.Empty;

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
            LogoPicture = supplier.LogoPicture,
            LicenseNumber = supplier.LicenseNumber,
            VatTrnNumber = supplier.VatTrnNumber,
            Gstin = supplier.Gstin,
            Pan = supplier.Pan,
            Website = supplier.Website,
            OwnerName = supplier.OwnerName,
            OwnerMobile = supplier.OwnerMobile,
            KeyContactName = supplier.KeyContactName,
            KeyContactMobile = supplier.KeyContactMobile,
            Email = supplier.Email,
            KeyContactEmail = supplier.KeyContactEmail,
            CompanyPhoneNumber = supplier.CompanyPhoneNumber,
            Mobile = supplier.Mobile,
            AddressLine1 = supplier.AddressLine1,
            AddressLine2 = supplier.AddressLine2,
            Building = supplier.Building,
            Area = supplier.Area,
            City = supplier.City,
            State = supplier.State,
            Country = supplier.Country,
            PoBox = supplier.PoBox,
            LocationLatitude = supplier.LocationLatitude,
            LocationLongitude = supplier.LocationLongitude,
            CreditLimit = supplier.CreditLimit,
            PaymentTerms = supplier.PaymentTerms,
            OpeningBalance = supplier.OpeningBalance,
            BalanceType = supplier.BalanceType,
            IsActive = supplier.IsActive
        };
        
        UpdateLogoFileName();
        ClearValidation();
        OnPropertyChanged(nameof(HasLogo));
        OnPropertyChanged(nameof(HasLocation));
        OnPropertyChanged(nameof(LocationDisplayText));
    }

    /// <summary>
    /// Reset form to default state
    /// </summary>
    private void ResetForm()
    {
        EditingSupplier = new SupplierDto
        {
            CompanyName = string.Empty,
            LicenseNumber = string.Empty,
            VatTrnNumber = string.Empty,
            Gstin = string.Empty,
            Pan = string.Empty,
            Website = string.Empty,
            OwnerName = string.Empty,
            OwnerMobile = string.Empty,
            KeyContactName = string.Empty,
            KeyContactMobile = string.Empty,
            Email = string.Empty,
            KeyContactEmail = string.Empty,
            CompanyPhoneNumber = string.Empty,
            Mobile = string.Empty,
            AddressLine1 = string.Empty,
            AddressLine2 = string.Empty,
            Building = string.Empty,
            Area = string.Empty,
            City = string.Empty,
            State = string.Empty,
            Country = string.Empty,
            PoBox = string.Empty,
            CreditLimit = 0,
            PaymentTerms = string.Empty,
            OpeningBalance = 0,
            BalanceType = "credit",
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

        if (string.IsNullOrWhiteSpace(EditingSupplier.KeyContactName))
        {
            SetValidationError("Contact person name is required.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(EditingSupplier.AddressLine1))
        {
            SetValidationError("Address line 1 is required.");
            return false;
        }

        if (!string.IsNullOrWhiteSpace(EditingSupplier.Email))
        {
            // Basic email validation
            if (!EditingSupplier.Email.Contains("@") || !EditingSupplier.Email.Contains("."))
            {
                SetValidationError("Please enter a valid company email address.");
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(EditingSupplier.KeyContactEmail))
        {
            // Basic email validation
            if (!EditingSupplier.KeyContactEmail.Contains("@") || !EditingSupplier.KeyContactEmail.Contains("."))
            {
                SetValidationError("Please enter a valid contact email address.");
                return false;
            }
        }

        if (EditingSupplier.CreditLimit < 0)
        {
            SetValidationError("Credit limit cannot be negative.");
            return false;
        }

        if (EditingSupplier.OpeningBalance < 0)
        {
            SetValidationError("Opening balance cannot be negative.");
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

    /// <summary>
    /// Upload logo image
    /// </summary>
    [RelayCommand]
    private void UploadLogo()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Company Logo",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileName = Path.GetFileName(filePath);
                
                // For now, store the file path. In production, you'd upload to server/storage
                EditingSupplier.LogoPicture = filePath;
                LogoFileName = fileName;
                
                OnPropertyChanged(nameof(HasLogo));
            }
        }
        catch (Exception ex)
        {
            SetValidationError($"Error uploading logo: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove the uploaded logo
    /// </summary>
    [RelayCommand]
    private void RemoveLogo()
    {
        EditingSupplier.LogoPicture = null;
        LogoFileName = string.Empty;
        OnPropertyChanged(nameof(HasLogo));
    }

    /// <summary>
    /// Pick location on map (opens map dialog)
    /// </summary>
    [RelayCommand]
    private void PickLocation()
    {
        try
        {
            var dialog = new MapPickerDialog();
            
            // Set current values if available
            dialog.Latitude = EditingSupplier.LocationLatitude;
            dialog.Longitude = EditingSupplier.LocationLongitude;
            
            if (dialog.ShowDialog() == true && dialog.LocationSelected)
            {
                EditingSupplier.LocationLatitude = dialog.Latitude;
                EditingSupplier.LocationLongitude = dialog.Longitude;
                OnPropertyChanged(nameof(HasLocation));
                OnPropertyChanged(nameof(LocationDisplayText));
            }
        }
        catch (Exception ex)
        {
            SetValidationError($"Error picking location: {ex.Message}");
        }
    }

    /// <summary>
    /// Update logo file name display
    /// </summary>
    private void UpdateLogoFileName()
    {
        if (!string.IsNullOrWhiteSpace(EditingSupplier?.LogoPicture))
        {
            LogoFileName = Path.GetFileName(EditingSupplier.LogoPicture);
        }
        else
        {
            LogoFileName = string.Empty;
        }
    }
}