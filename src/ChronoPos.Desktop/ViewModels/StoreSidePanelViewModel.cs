using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ChronoPos.Desktop.ViewModels;

public partial class StoreSidePanelViewModel : ObservableObject
{
    private readonly IStoreService _storeService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    private StoreDto? _originalStore;
    private bool _isEditMode;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string managerName = string.Empty;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private bool isDefault = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Store";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }

    // Constructor for adding a new store
    public StoreSidePanelViewModel(
        IStoreService storeService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _storeService = storeService ?? throw new ArgumentNullException(nameof(storeService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        _isEditMode = false;
        _originalStore = null;
        
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
    }

    // Constructor for editing an existing store
    public StoreSidePanelViewModel(
        IStoreService storeService,
        StoreDto originalStore,
        Action<bool> onSaved,
        Action onCancelled) : this(storeService, onSaved, onCancelled)
    {
        _isEditMode = true;
        _originalStore = originalStore ?? throw new ArgumentNullException(nameof(originalStore));
        
        FormTitle = "Edit Store";
        SaveButtonText = "Update";
        
        LoadForEdit(originalStore);
    }

    public void LoadForEdit(StoreDto store)
    {
        Name = store.Name;
        Address = store.Address ?? string.Empty;
        PhoneNumber = store.PhoneNumber ?? string.Empty;
        Email = store.Email ?? string.Empty;
        ManagerName = store.ManagerName ?? string.Empty;
        IsActive = store.IsActive;
        IsDefault = store.IsDefault;
        FormTitle = "Edit Store";
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

            if (_isEditMode && _originalStore != null)
            {
                await UpdateStore();
            }
            else
            {
                await CreateStore();
            }

            ValidationMessage = _isEditMode ? "Store updated successfully!" : "Store created successfully!";
            
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

    private async Task CreateStore()
    {
        var createDto = new CreateStoreDto
        {
            Name = Name.Trim(),
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            ManagerName = string.IsNullOrWhiteSpace(ManagerName) ? null : ManagerName.Trim(),
            IsActive = IsActive,
            IsDefault = IsDefault
        };

        await _storeService.CreateAsync(createDto);
    }

    private async Task UpdateStore()
    {
        if (_originalStore == null) return;

        var updateDto = new UpdateStoreDto
        {
            Name = Name.Trim(),
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
            ManagerName = string.IsNullOrWhiteSpace(ManagerName) ? null : ManagerName.Trim(),
            IsActive = IsActive,
            IsDefault = IsDefault
        };

        await _storeService.UpdateAsync(_originalStore.Id, updateDto);
    }

    private bool ValidateForm()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Store name is required.";
            return false;
        }

        if (Name.Trim().Length < 2)
        {
            ValidationMessage = "Store name must be at least 2 characters.";
            return false;
        }

        // Validate email format if provided
        if (!string.IsNullOrWhiteSpace(Email))
        {
            if (!IsValidEmail(Email.Trim()))
            {
                ValidationMessage = "Please enter a valid email address.";
                return false;
            }
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private void Close()
    {
        _onCancelled.Invoke();
    }
}