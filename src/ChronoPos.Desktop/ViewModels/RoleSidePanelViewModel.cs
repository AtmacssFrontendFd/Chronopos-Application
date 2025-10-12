using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class RoleSidePanelViewModel : ObservableObject
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly Action<bool> _onSaved;
    private RoleDto? _originalRole;
    private bool _isEditMode;

    [ObservableProperty]
    private string roleName = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string status = "Active";

    [ObservableProperty]
    private ObservableCollection<string> availableStatuses = new() { "Active", "Inactive" };

    [ObservableProperty]
    private ObservableCollection<PermissionDto> availablePermissions = new();

    [ObservableProperty]
    private PermissionDto? selectedPermission;

    [ObservableProperty]
    private ObservableCollection<PermissionDto> selectedPermissions = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add New Role";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    public RoleSidePanelViewModel(
        IRoleService roleService,
        IPermissionService permissionService,
        Action<bool> onSaved,
        RoleDto? originalRole = null)
    {
        _roleService = roleService;
        _permissionService = permissionService;
        _onSaved = onSaved;
        _originalRole = originalRole;
        _isEditMode = originalRole != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Cancel);
        AddPermissionCommand = new RelayCommand(AddPermission);
        RemovePermissionCommand = new RelayCommand<PermissionDto>(RemovePermission);

        // Load available permissions
        _ = LoadAvailablePermissionsAsync();

        if (originalRole != null)
        {
            LoadForEdit(originalRole);
        }
    }

    public IAsyncRelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand AddPermissionCommand { get; }
    public RelayCommand<PermissionDto> RemovePermissionCommand { get; }

    private async Task LoadAvailablePermissionsAsync()
    {
        try
        {
            var permissions = await _permissionService.GetAllAsync();
            AvailablePermissions.Clear();
            foreach (var permission in permissions.Where(p => p.Status == "Active").OrderBy(p => p.Name))
            {
                AvailablePermissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading permissions: {ex.Message}";
        }
    }

    private void AddPermission()
    {
        if (SelectedPermission == null) return;

        // Check if already added
        if (SelectedPermissions.Any(p => p.PermissionId == SelectedPermission.PermissionId))
        {
            ValidationMessage = "This permission is already added";
            return;
        }

        SelectedPermissions.Add(SelectedPermission);
        SelectedPermission = null;
        ValidationMessage = string.Empty;
    }

    private void RemovePermission(PermissionDto? permission)
    {
        if (permission == null) return;
        SelectedPermissions.Remove(permission);
    }

    public async void LoadForEdit(RoleDto role)
    {
        _originalRole = role;
        _isEditMode = true;
        
        RoleName = role.RoleName;
        Description = role.Description ?? string.Empty;
        Status = role.Status ?? "Active";
        FormTitle = "Edit Role";
        SaveButtonText = "Update";

        // Load permissions for this role
        await LoadRolePermissionsAsync(role.RoleId);
    }

    private async Task LoadRolePermissionsAsync(int roleId)
    {
        try
        {
            var rolePermissions = await _permissionService.GetPermissionsByRoleIdAsync(roleId);
            SelectedPermissions.Clear();
            foreach (var permission in rolePermissions)
            {
                SelectedPermissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading role permissions: {ex.Message}";
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(RoleName))
        {
            ValidationMessage = "Role name is required";
            return false;
        }

        if (RoleName.Length > 100)
        {
            ValidationMessage = "Role name must not exceed 100 characters";
            return false;
        }

        ValidationMessage = string.Empty;
        return true;
    }

    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;

        try
        {
            IsLoading = true;
            CanSave = false;
            ValidationMessage = string.Empty;

            if (_isEditMode && _originalRole != null)
            {
                await UpdateRole();
            }
            else
            {
                await CreateRole();
            }

            ValidationMessage = _isEditMode ? "Role updated successfully!" : "Role created successfully!";
            
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

    private async Task CreateRole()
    {
        var createDto = new CreateRoleDto
        {
            RoleName = RoleName.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status
        };

        var createdRole = await _roleService.CreateAsync(createDto);

        // Assign permissions to the role if any selected
        if (SelectedPermissions.Any())
        {
            var permissionIds = SelectedPermissions.Select(p => p.PermissionId).ToList();
            await _roleService.AssignPermissionsToRoleAsync(createdRole.RoleId, permissionIds);
        }
    }

    private async Task UpdateRole()
    {
        if (_originalRole == null) return;

        var updateDto = new UpdateRoleDto
        {
            RoleName = RoleName.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            Status = Status
        };

        await _roleService.UpdateAsync(_originalRole.RoleId, updateDto);

        // Sync permissions - remove old ones and add new ones
        var permissionIds = SelectedPermissions.Select(p => p.PermissionId).ToList();
        await _roleService.SyncRolePermissionsAsync(_originalRole.RoleId, permissionIds);
    }

    private void Cancel()
    {
        _onSaved.Invoke(false);
    }
}
