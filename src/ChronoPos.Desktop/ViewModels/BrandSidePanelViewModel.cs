using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ChronoPos.Desktop.ViewModels;

public partial class BrandSidePanelViewModel : ObservableObject
{
    private readonly IBrandService _brandService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    private BrandDto? _originalBrand;
    private bool _isEditMode;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string nameArabic = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string logoUrl = string.Empty;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Brand";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }

    // Constructor for adding a new brand
    public BrandSidePanelViewModel(
        IBrandService brandService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        _isEditMode = false;
        _originalBrand = null;
        
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
    }

    // Constructor for editing an existing brand
    public BrandSidePanelViewModel(
        IBrandService brandService,
        BrandDto originalBrand,
        Action<bool> onSaved,
        Action onCancelled) : this(brandService, onSaved, onCancelled)
    {
        _isEditMode = true;
        _originalBrand = originalBrand ?? throw new ArgumentNullException(nameof(originalBrand));
        
        FormTitle = "Edit Brand";
        SaveButtonText = "Update";
        
        LoadForEdit(originalBrand);
    }

    public void LoadForEdit(BrandDto brand)
    {
        Name = brand.Name;
        NameArabic = brand.NameArabic ?? string.Empty;
        Description = brand.Description ?? string.Empty;
        LogoUrl = brand.LogoUrl ?? string.Empty;
        IsActive = brand.IsActive;
        FormTitle = "Edit Brand";
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

            if (_isEditMode && _originalBrand != null)
            {
                await UpdateBrand();
            }
            else
            {
                await CreateBrand();
            }

            ValidationMessage = _isEditMode ? "Brand updated successfully!" : "Brand created successfully!";
            
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

    private async Task CreateBrand()
    {
        var createDto = new CreateBrandDto
        {
            Name = Name.Trim(),
            NameArabic = string.IsNullOrWhiteSpace(NameArabic) ? null : NameArabic.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(LogoUrl) ? null : LogoUrl.Trim(),
            IsActive = IsActive
        };

        await _brandService.CreateAsync(createDto);
    }

    private async Task UpdateBrand()
    {
        if (_originalBrand == null) return;

        var updateDto = new UpdateBrandDto
        {
            Name = Name.Trim(),
            NameArabic = string.IsNullOrWhiteSpace(NameArabic) ? null : NameArabic.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            LogoUrl = string.IsNullOrWhiteSpace(LogoUrl) ? null : LogoUrl.Trim(),
            IsActive = IsActive
        };

        await _brandService.UpdateAsync(_originalBrand.Id, updateDto);
    }

    private bool ValidateForm()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Brand name is required.";
            return false;
        }

        if (Name.Trim().Length < 2)
        {
            ValidationMessage = "Brand name must be at least 2 characters.";
            return false;
        }

        return true;
    }

    private void Close()
    {
        _onCancelled.Invoke();
    }
}
