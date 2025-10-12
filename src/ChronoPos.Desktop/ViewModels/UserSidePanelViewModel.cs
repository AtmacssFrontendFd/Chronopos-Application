using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Models;
using ChronoPos.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for User Side Panel (Add/Edit User)
/// </summary>
public partial class UserSidePanelViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly IUserPermissionOverrideService _userPermissionOverrideService;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly bool _isEditMode;
    private readonly UserDto? _originalUser;
    private readonly Action _onSaveCallback;
    private readonly Action _onCancelCallback;

    [ObservableProperty]
    private string _formTitle = "Add User";

    [ObservableProperty]
    private string _saveButtonText = "Create User";

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _phoneNo;

    [ObservableProperty]
    private string? _address;

    [ObservableProperty]
    private ObservableCollection<RoleDto> _availableRoles = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

    [ObservableProperty]
    private ObservableCollection<PermissionDto> _availablePermissions = new();

    [ObservableProperty]
    private PermissionDto? _selectedPermission;

    [ObservableProperty]
    private ObservableCollection<PermissionOverrideModel> _permissionOverrides = new();

    [ObservableProperty]
    private ObservableCollection<PermissionDto> _rolePermissions = new();

    [ObservableProperty]
    private DateTime? _validFrom;

    [ObservableProperty]
    private DateTime? _validTo;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    public bool CanSave => !string.IsNullOrWhiteSpace(FullName) && 
                           !string.IsNullOrWhiteSpace(Email) && 
                           SelectedRole != null &&
                           (!_isEditMode || string.IsNullOrWhiteSpace(Password) || Password == ConfirmPassword);

    public UserSidePanelViewModel(
        IUserService userService,
        IRoleService roleService,
        IPermissionService permissionService,
        IUserPermissionOverrideService userPermissionOverrideService,
        IRolePermissionRepository rolePermissionRepository,
        bool isEditMode,
        UserDto? userToEdit,
        Action onSaveCallback,
        Action onCancelCallback)
    {
        _userService = userService;
        _roleService = roleService;
        _permissionService = permissionService;
        _userPermissionOverrideService = userPermissionOverrideService;
        _rolePermissionRepository = rolePermissionRepository;
        _isEditMode = isEditMode;
        _originalUser = userToEdit;
        _onSaveCallback = onSaveCallback;
        _onCancelCallback = onCancelCallback;

        if (_isEditMode && _originalUser != null)
        {
            FormTitle = "Edit User";
            SaveButtonText = "Update User";
            LoadUserData(_originalUser);
        }
        else
        {
            // Clear password fields for new user
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        _ = LoadRolesAsync();
        _ = LoadPermissionsAsync();
    }

    private void LoadUserData(UserDto user)
    {
        FullName = user.FullName;
        Email = user.Email;
        PhoneNo = user.PhoneNo;
        Address = user.Address;
        
        // Don't load password for security reasons
        // Password fields should remain empty during edit
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        
        // Load existing permission overrides if editing
        if (_isEditMode)
        {
            _ = LoadUserPermissionOverridesAsync(user.Id);
        }
    }

    private async Task LoadPermissionsAsync()
    {
        try
        {
            var permissions = await _permissionService.GetAllAsync();
            
            AvailablePermissions.Clear();
            foreach (var permission in permissions)
            {
                AvailablePermissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading permissions: {ex.Message}");
        }
    }

    private async Task LoadUserPermissionOverridesAsync(int userId)
    {
        try
        {
            var overrides = await _userPermissionOverrideService.GetByUserIdAsync(userId);
            var permissions = await _permissionService.GetAllAsync();
            
            PermissionOverrides.Clear();
            foreach (var overrideDto in overrides)
            {
                var permission = permissions.FirstOrDefault(p => p.PermissionId == overrideDto.PermissionId);
                if (permission != null)
                {
                    var overrideModel = PermissionOverrideModel.FromPermissionDto(permission, overrideDto.ValidFrom, overrideDto.ValidTo);
                    overrideModel.IsAllowed = overrideDto.IsAllowed;
                    PermissionOverrides.Add(overrideModel);
                }
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading user permission overrides: {ex.Message}");
        }
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            var roles = await _roleService.GetAllAsync();
            
            AvailableRoles.Clear();
            foreach (var role in roles)
            {
                AvailableRoles.Add(role);
            }

            if (_isEditMode && _originalUser != null)
            {
                SelectedRole = AvailableRoles.FirstOrDefault(r => r.RoleId == _originalUser.RolePermissionId);
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading roles: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddPermissionOverride()
    {
        if (SelectedPermission == null)
        {
            ValidationMessage = "Please select a permission to add";
            return;
        }

        // Check if permission is already added
        if (PermissionOverrides.Any(p => p.PermissionId == SelectedPermission.PermissionId))
        {
            ValidationMessage = "This permission has already been added";
            return;
        }

        var overrideModel = PermissionOverrideModel.FromPermissionDto(SelectedPermission, ValidFrom, ValidTo);
        PermissionOverrides.Add(overrideModel);

        // Clear selection and dates
        SelectedPermission = null;
        ValidFrom = null;
        ValidTo = null;
        ValidationMessage = string.Empty;
    }

    [RelayCommand]
    private void RemovePermissionOverride(PermissionOverrideModel permissionOverride)
    {
        PermissionOverrides.Remove(permissionOverride);
    }

    partial void OnSelectedRoleChanged(RoleDto? value)
    {
        OnPropertyChanged(nameof(CanSave));
        if (value != null)
        {
            _ = LoadRolePermissionsAsync(value.RoleId);
        }
        else
        {
            RolePermissions.Clear();
        }
    }

    private async Task LoadRolePermissionsAsync(int roleId)
    {
        try
        {
            var rolePermissions = await _rolePermissionRepository.GetActiveByRoleIdAsync(roleId);
            var allPermissions = await _permissionService.GetAllAsync();
            
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            var permissions = allPermissions.Where(p => permissionIds.Contains(p.PermissionId));
            
            RolePermissions.Clear();
            foreach (var permission in permissions)
            {
                RolePermissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading role permissions: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (!ValidateForm())
            return;

        try
        {
            IsLoading = true;
            ValidationMessage = string.Empty;

            int userId;

            if (_isEditMode && _originalUser != null)
            {
                var updateDto = new UpdateUserDto
                {
                    FullName = FullName,
                    Email = Email,
                    Role = SelectedRole!.RoleName,
                    PhoneNo = PhoneNo,
                    RolePermissionId = SelectedRole!.RoleId,
                    Address = Address,
                    ShopId = _originalUser.ShopId,
                    ChangeAccess = _originalUser.ChangeAccess,
                    ShiftTypeId = _originalUser.ShiftTypeId,
                    AdditionalDetails = _originalUser.AdditionalDetails,
                    UaeId = _originalUser.UaeId,
                    Dob = _originalUser.Dob,
                    NationalityStatus = _originalUser.NationalityStatus,
                    Salary = _originalUser.Salary
                };

                await _userService.UpdateAsync(_originalUser.Id, updateDto);
                userId = _originalUser.Id;

                // Delete existing overrides and recreate
                await _userPermissionOverrideService.DeleteByUserIdAsync(userId);
            }
            else
            {
                var createDto = new CreateUserDto
                {
                    FullName = FullName,
                    Email = Email,
                    Password = Password,
                    Role = SelectedRole!.RoleName,
                    PhoneNo = PhoneNo,
                    RolePermissionId = SelectedRole!.RoleId,
                    Address = Address,
                    ShopId = 1, // Default shop
                    ChangeAccess = false
                };

                var createdUser = await _userService.CreateAsync(createDto);
                userId = createdUser.Id;
            }

            // Save permission overrides
            if (PermissionOverrides.Any())
            {
                var overrideDtos = PermissionOverrides.Select(po => new CreateUserPermissionOverrideDto
                {
                    UserId = userId,
                    PermissionId = po.PermissionId,
                    IsAllowed = po.IsAllowed,
                    ValidFrom = po.ValidFrom,
                    ValidTo = po.ValidTo,
                    CreatedBy = 1 // TODO: Get current user ID
                }).ToList();

                await _userPermissionOverrideService.CreateBulkAsync(overrideDtos);
            }

            _onSaveCallback?.Invoke();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error saving user: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancelCallback?.Invoke();
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            ValidationMessage = "Full name is required";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ValidationMessage = "Email is required";
            return false;
        }

        if (!_isEditMode && string.IsNullOrWhiteSpace(Password))
        {
            ValidationMessage = "Password is required";
            return false;
        }

        if (!_isEditMode && Password != ConfirmPassword)
        {
            ValidationMessage = "Passwords do not match";
            return false;
        }

        if (SelectedRole == null)
        {
            ValidationMessage = "Please select a role";
            return false;
        }

        return true;
    }

    partial void OnFullNameChanged(string value) => OnPropertyChanged(nameof(CanSave));
    partial void OnEmailChanged(string value) => OnPropertyChanged(nameof(CanSave));
    partial void OnPasswordChanged(string value) => OnPropertyChanged(nameof(CanSave));
    partial void OnConfirmPasswordChanged(string value) => OnPropertyChanged(nameof(CanSave));
}
