using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class PermissionSidePanelViewModel : ObservableObject
{
    private readonly IPermissionService _permissionService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    private PermissionDto? _originalPermission;
    private bool _isEditMode;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string code = string.Empty;

    [ObservableProperty]
    private string screenName = string.Empty;

    [ObservableProperty]
    private string typeMatrix = string.Empty;

    [ObservableProperty]
    private bool isParent = false;

    [ObservableProperty]
    private int? parentPermissionId = null;

    [ObservableProperty]
    private string status = "Active";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Permission";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private ObservableCollection<PermissionDto> availableParentPermissions = new();

    [ObservableProperty]
    private PermissionDto? selectedParentPermission;

    [ObservableProperty]
    private ObservableCollection<string> availableScreenNames = new();

    [ObservableProperty]
    private ObservableCollection<string> availableTypeMatrix = new();

    [ObservableProperty]
    private ObservableCollection<string> availableStatuses = new();

    // Multi-select properties for Screen Names
    [ObservableProperty]
    private string? selectedScreenName;

    [ObservableProperty]
    private ObservableCollection<string> selectedScreenNames = new();

    // Multi-select properties for Type Matrix
    [ObservableProperty]
    private string? selectedTypeMatrix;

    [ObservableProperty]
    private ObservableCollection<string> selectedTypeMatrixList = new();

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }
    public IRelayCommand AddScreenNameCommand { get; }
    public IRelayCommand<string> RemoveScreenNameCommand { get; }
    public IRelayCommand AddTypeMatrixCommand { get; }
    public IRelayCommand<string> RemoveTypeMatrixCommand { get; }

    // Constructor for adding a new permission
    public PermissionSidePanelViewModel(
        IPermissionService permissionService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        _isEditMode = false;
        _originalPermission = null;
        
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
        AddScreenNameCommand = new RelayCommand(AddScreenName);
        RemoveScreenNameCommand = new RelayCommand<string>(RemoveScreenName);
        AddTypeMatrixCommand = new RelayCommand(AddTypeMatrix);
        RemoveTypeMatrixCommand = new RelayCommand<string>(RemoveTypeMatrix);

        InitializeDropdowns();
        _ = LoadParentPermissionsAsync();
    }

    // Constructor for editing an existing permission
    public PermissionSidePanelViewModel(
        IPermissionService permissionService,
        PermissionDto originalPermission,
        Action<bool> onSaved,
        Action onCancelled) : this(permissionService, onSaved, onCancelled)
    {
        _isEditMode = true;
        _originalPermission = originalPermission ?? throw new ArgumentNullException(nameof(originalPermission));
        
        FormTitle = "Edit Permission";
        SaveButtonText = "Update";
        
        LoadForEdit(originalPermission);
    }

    public void LoadForEdit(PermissionDto permission)
    {
        _originalPermission = permission;
        _isEditMode = true;
        
        Name = permission.Name;
        Code = permission.Code;
        
        // Parse screen names (comma-separated)
        SelectedScreenNames.Clear();
        if (!string.IsNullOrWhiteSpace(permission.ScreenName))
        {
            var screens = permission.ScreenName.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var screen in screens)
            {
                var trimmedScreen = screen.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedScreen))
                {
                    SelectedScreenNames.Add(trimmedScreen);
                }
            }
        }
        
        // Parse type matrix (comma-separated)
        SelectedTypeMatrixList.Clear();
        if (!string.IsNullOrWhiteSpace(permission.TypeMatrix))
        {
            var types = permission.TypeMatrix.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var type in types)
            {
                var trimmedType = type.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedType))
                {
                    SelectedTypeMatrixList.Add(trimmedType);
                }
            }
        }
        
        IsParent = permission.IsParent;
        ParentPermissionId = permission.ParentPermissionId;
        Status = permission.Status ?? "Active";
        FormTitle = "Edit Permission";
        SaveButtonText = "Update";

        // Set selected parent if applicable
        if (ParentPermissionId.HasValue)
        {
            SelectedParentPermission = AvailableParentPermissions.FirstOrDefault(p => p.PermissionId == ParentPermissionId.Value);
        }
    }

    private void AddScreenName()
    {
        if (!string.IsNullOrWhiteSpace(SelectedScreenName))
        {
            var screenName = SelectedScreenName.Trim();
            if (!SelectedScreenNames.Contains(screenName))
            {
                SelectedScreenNames.Add(screenName);
                SelectedScreenName = string.Empty; // Clear selection
            }
            else
            {
                ValidationMessage = "This screen name is already added.";
            }
        }
    }

    private void RemoveScreenName(string? screenName)
    {
        if (!string.IsNullOrWhiteSpace(screenName))
        {
            SelectedScreenNames.Remove(screenName);
        }
    }

    private void AddTypeMatrix()
    {
        if (!string.IsNullOrWhiteSpace(SelectedTypeMatrix))
        {
            var typeMatrix = SelectedTypeMatrix.Trim();
            if (!SelectedTypeMatrixList.Contains(typeMatrix))
            {
                SelectedTypeMatrixList.Add(typeMatrix);
                SelectedTypeMatrix = string.Empty; // Clear selection
            }
            else
            {
                ValidationMessage = "This type matrix is already added.";
            }
        }
    }

    private void RemoveTypeMatrix(string? typeMatrix)
    {
        if (!string.IsNullOrWhiteSpace(typeMatrix))
        {
            SelectedTypeMatrixList.Remove(typeMatrix);
        }
    }

    private async Task LoadParentPermissionsAsync()
    {
        try
        {
            var parentPermissions = await _permissionService.GetParentPermissionsAsync();
            AvailableParentPermissions.Clear();
            
            // Add a null option for no parent
            AvailableParentPermissions.Add(new PermissionDto 
            { 
                PermissionId = 0, 
                Name = "-- No Parent --", 
                Code = "" 
            });
            
            foreach (var parent in parentPermissions)
            {
                // Don't add the current permission to the list if editing
                if (_isEditMode && _originalPermission != null && parent.PermissionId == _originalPermission.PermissionId)
                    continue;
                    
                AvailableParentPermissions.Add(parent);
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading parent permissions: {ex.Message}";
        }
    }

    partial void OnSelectedParentPermissionChanged(PermissionDto? value)
    {
        ParentPermissionId = value?.PermissionId == 0 ? null : value?.PermissionId;
    }

    partial void OnIsParentChanged(bool value)
    {
        // If this is a parent permission, clear the parent selection
        if (value)
        {
            SelectedParentPermission = AvailableParentPermissions.FirstOrDefault();
            ParentPermissionId = null;
        }
    }

    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;

        try
        {
            IsLoading = true;
            CanSave = false;
            ValidationMessage = string.Empty;

            if (_isEditMode && _originalPermission != null)
            {
                await UpdatePermission();
            }
            else
            {
                await CreatePermission();
            }

            ValidationMessage = _isEditMode ? "Permission updated successfully!" : "Permission created successfully!";
            
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

    private async Task CreatePermission()
    {
        var createDto = new CreatePermissionDto
        {
            Name = Name.Trim(),
            Code = Code.Trim().ToUpperInvariant(),
            ScreenName = SelectedScreenNames.Count > 0 ? string.Join(", ", SelectedScreenNames) : null,
            TypeMatrix = SelectedTypeMatrixList.Count > 0 ? string.Join(", ", SelectedTypeMatrixList) : null,
            IsParent = IsParent,
            ParentPermissionId = ParentPermissionId,
            Status = Status
        };

        await _permissionService.CreateAsync(createDto);
    }

    private async Task UpdatePermission()
    {
        if (_originalPermission == null) return;

        var updateDto = new UpdatePermissionDto
        {
            Name = Name.Trim(),
            Code = Code.Trim().ToUpperInvariant(),
            ScreenName = SelectedScreenNames.Count > 0 ? string.Join(", ", SelectedScreenNames) : null,
            TypeMatrix = SelectedTypeMatrixList.Count > 0 ? string.Join(", ", SelectedTypeMatrixList) : null,
            IsParent = IsParent,
            ParentPermissionId = ParentPermissionId,
            Status = Status
        };

        await _permissionService.UpdateAsync(_originalPermission.PermissionId, updateDto);
    }

    private bool ValidateForm()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Permission name is required.";
            return false;
        }

        if (Name.Trim().Length < 2)
        {
            ValidationMessage = "Permission name must be at least 2 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Code))
        {
            ValidationMessage = "Permission code is required.";
            return false;
        }

        if (Code.Trim().Length < 2)
        {
            ValidationMessage = "Permission code must be at least 2 characters.";
            return false;
        }

        // Validate that a non-parent permission has a parent selected
        if (!IsParent && !ParentPermissionId.HasValue)
        {
            ValidationMessage = "Child permissions must have a parent selected.";
            return false;
        }

        return true;
    }

    private void Close()
    {
        _onCancelled.Invoke();
    }

    private void InitializeDropdowns()
    {
        // Available Screen Names based on the application screens
        AvailableScreenNames.Clear();
        AvailableScreenNames.Add("Dashboard");
        AvailableScreenNames.Add("Products");
        AvailableScreenNames.Add("Categories");
        AvailableScreenNames.Add("Brands");
        AvailableScreenNames.Add("Units");
        AvailableScreenNames.Add("Customers");
        AvailableScreenNames.Add("Suppliers");
        AvailableScreenNames.Add("Sales");
        AvailableScreenNames.Add("Purchase");
        AvailableScreenNames.Add("Stock");
        AvailableScreenNames.Add("Reports");
        AvailableScreenNames.Add("Settings");
        AvailableScreenNames.Add("Users");
        AvailableScreenNames.Add("Roles");
        AvailableScreenNames.Add("Permissions");
        AvailableScreenNames.Add("Warehouse");
        AvailableScreenNames.Add("POS");
        AvailableScreenNames.Add("Invoices");
        AvailableScreenNames.Add("Payments");
        AvailableScreenNames.Add("Expenses");

        // Available Type Matrix (CRUD operations)
        AvailableTypeMatrix.Clear();
        AvailableTypeMatrix.Add("Create");
        AvailableTypeMatrix.Add("Read");
        AvailableTypeMatrix.Add("Update");
        AvailableTypeMatrix.Add("Delete");
        AvailableTypeMatrix.Add("View");
        AvailableTypeMatrix.Add("Export");
        AvailableTypeMatrix.Add("Import");
        AvailableTypeMatrix.Add("Print");

        // Available Statuses
        AvailableStatuses.Clear();
        AvailableStatuses.Add("Active");
        AvailableStatuses.Add("Inactive");
    }
}
