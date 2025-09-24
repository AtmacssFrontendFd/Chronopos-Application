using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ChronoPos.Desktop.ViewModels;

public partial class PriceTypeSidePanelViewModel : ObservableObject
{
    private readonly ISellingPriceTypeService _priceTypeService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    private SellingPriceTypeDto? _originalPriceType;
    private bool _isEditMode;

    [ObservableProperty]
    private string typeName = string.Empty;

    [ObservableProperty]
    private string arabicName = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool status = true;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Price Type";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }

    // Constructor for adding a new price type
    public PriceTypeSidePanelViewModel(
        ISellingPriceTypeService priceTypeService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _priceTypeService = priceTypeService ?? throw new ArgumentNullException(nameof(priceTypeService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        _isEditMode = false;
        _originalPriceType = null;
        
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
    }

    // Constructor for editing an existing price type
    public PriceTypeSidePanelViewModel(
        ISellingPriceTypeService priceTypeService,
        SellingPriceTypeDto originalPriceType,
        Action<bool> onSaved,
        Action onCancelled) : this(priceTypeService, onSaved, onCancelled)
    {
        _isEditMode = true;
        _originalPriceType = originalPriceType ?? throw new ArgumentNullException(nameof(originalPriceType));
        
        FormTitle = "Edit Price Type";
        SaveButtonText = "Update";
        
        LoadForEdit(originalPriceType);
    }

    public void LoadForEdit(SellingPriceTypeDto priceType)
    {
        TypeName = priceType.TypeName;
        ArabicName = priceType.ArabicName ?? string.Empty;
        Description = priceType.Description ?? string.Empty;
        Status = priceType.Status;
        FormTitle = "Edit Price Type";
        SaveButtonText = "Update";
    }

    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;

        try
        {
            IsLoading = true;
            CanSave = false;
            ValidationMessage = string.Empty;

            if (_isEditMode && _originalPriceType != null)
            {
                await UpdatePriceType();
            }
            else
            {
                await CreatePriceType();
            }

            ValidationMessage = _isEditMode ? "Price type updated successfully!" : "Price type created successfully!";
            
            // Delay before closing to show success message
            await Task.Delay(1000);
            
            _onSaved.Invoke(true);
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error: {ex.Message}";
            CanSave = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreatePriceType()
    {
        var createDto = new CreateSellingPriceTypeDto
        {
            TypeName = TypeName.Trim(),
            ArabicName = ArabicName.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status,
            CreatedBy = 1 // TODO: Get from current user context
        };

        await _priceTypeService.CreateAsync(createDto);
    }

    private async Task UpdatePriceType()
    {
        if (_originalPriceType == null) return;

        var updateDto = new UpdateSellingPriceTypeDto
        {
            Id = _originalPriceType.Id,
            TypeName = TypeName.Trim(),
            ArabicName = ArabicName.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status,
            UpdatedBy = 1 // TODO: Get from current user context
        };

        await _priceTypeService.UpdateAsync(updateDto);
    }

    private bool ValidateForm()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(TypeName))
        {
            ValidationMessage = "Type name is required.";
            return false;
        }

        if (TypeName.Trim().Length < 2)
        {
            ValidationMessage = "Type name must be at least 2 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ArabicName))
        {
            ValidationMessage = "Arabic name is required.";
            return false;
        }

        if (ArabicName.Trim().Length < 2)
        {
            ValidationMessage = "Arabic name must be at least 2 characters.";
            return false;
        }

        return true;
    }

    private void Close()
    {
        _onCancelled.Invoke();
    }
}