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
/// ViewModel for Users Management screen
/// </summary>
public partial class UsersManagementViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IUserPermissionOverrideService _userPermissionOverrideService;

    [ObservableProperty]
    private ObservableCollection<UserWithPermissionsModel> _users = new();

    [ObservableProperty]
    private ObservableCollection<PermissionDto> _allPermissions = new();

    [ObservableProperty]
    private bool _isSidePanelOpen = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private UserSidePanelViewModel? _userSidePanelViewModel;

    public UsersManagementViewModel(
        IUserService userService,
        IRoleService roleService,
        IPermissionService permissionService,
        IRolePermissionRepository rolePermissionRepository,
        IUserPermissionOverrideService userPermissionOverrideService)
    {
        _userService = userService;
        _roleService = roleService;
        _permissionService = permissionService;
        _rolePermissionRepository = rolePermissionRepository;
        _userPermissionOverrideService = userPermissionOverrideService;

        _ = LoadUsersAsync();
        _ = LoadPermissionsAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            var users = await _userService.GetUsersWithRolesAsync();
            
            Users.Clear();
            foreach (var user in users)
            {
                var userModel = UserWithPermissionsModel.FromUserDto(user);
                
                // Load role permissions for this user
                if (user.RolePermissionId > 0)
                {
                    await LoadRolePermissionsForUserAsync(userModel, user.RolePermissionId);
                }
                
                // Load permission overrides for this user
                await LoadPermissionOverridesForUserAsync(userModel, user.Id);
                
                Users.Add(userModel);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading users: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRolePermissionsForUserAsync(UserWithPermissionsModel userModel, int roleId)
    {
        try
        {
            // Get all RolePermissions for this role
            var rolePermissions = await _rolePermissionRepository.GetActiveByRoleIdAsync(roleId);
            
            // Get all permissions
            var allPermissions = await _permissionService.GetAllAsync();
            
            // Filter permissions that are assigned to this role
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToHashSet();
            var userRolePermissions = allPermissions.Where(p => permissionIds.Contains(p.PermissionId));
            
            userModel.RolePermissions.Clear();
            userModel.Permissions.Clear();
            foreach (var permission in userRolePermissions)
            {
                userModel.RolePermissions.Add(permission);
                userModel.Permissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading role permissions for user {userModel.FullName}: {ex.Message}");
        }
    }

    private async Task LoadPermissionOverridesForUserAsync(UserWithPermissionsModel userModel, int userId)
    {
        try
        {
            // Get ALL permission overrides for this user (not just active ones)
            // We want to show past, present, and future overrides in the UI
            var overrides = await _userPermissionOverrideService.GetByUserIdAsync(userId);
            
            userModel.PermissionOverrides.Clear();
            foreach (var overrideDto in overrides)
            {
                userModel.PermissionOverrides.Add(overrideDto);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading permission overrides for user {userModel.FullName}: {ex.Message}");
        }
    }

    private async Task LoadPermissionsAsync()
    {
        try
        {
            var permissions = await _permissionService.GetAllAsync();
            
            AllPermissions.Clear();
            foreach (var permission in permissions)
            {
                AllPermissions.Add(permission);
            }
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"Error loading permissions: {ex.Message}");
        }
    }

    [RelayCommand]
    private void AddUser()
    {
        UserSidePanelViewModel = new UserSidePanelViewModel(
            _userService,
            _roleService,
            _permissionService,
            _userPermissionOverrideService,
            _rolePermissionRepository,
            isEditMode: false,
            userToEdit: null,
            onSaveCallback: async () =>
            {
                await LoadUsersAsync();
                CloseSidePanel();
            },
            onCancelCallback: CloseSidePanel
        );

        IsSidePanelOpen = true;
    }

    [RelayCommand]
    private void EditUser(UserWithPermissionsModel userModel)
    {
        // Convert back to UserDto for the side panel with ALL user data
        var userDto = new UserDto
        {
            Id = userModel.Id,
            FullName = userModel.FullName,
            Username = userModel.Username,
            Email = userModel.Email,
            PhoneNo = userModel.PhoneNo,
            Address = userModel.Address,
            RolePermissionId = userModel.RolePermissionId ?? 0,
            RolePermissionName = userModel.RolePermissionName,
            ShopId = userModel.ShopId,
            ChangeAccess = userModel.ChangeAccess,
            ShiftTypeId = userModel.ShiftTypeId,
            AdditionalDetails = userModel.AdditionalDetails,
            UaeId = userModel.UaeId,
            Dob = userModel.Dob,
            NationalityStatus = userModel.NationalityStatus,
            Salary = userModel.Salary
        };

        UserSidePanelViewModel = new UserSidePanelViewModel(
            _userService,
            _roleService,
            _permissionService,
            _userPermissionOverrideService,
            _rolePermissionRepository,
            isEditMode: true,
            userToEdit: userDto,
            onSaveCallback: async () =>
            {
                await LoadUsersAsync();
                CloseSidePanel();
            },
            onCancelCallback: CloseSidePanel
        );

        IsSidePanelOpen = true;
    }

    [RelayCommand]
    private async Task DeleteUser(UserWithPermissionsModel userModel)
    {
        var result = MessageBox.Show(
            $"Are you sure you want to delete user '{userModel.FullName}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _userService.DeleteAsync(userModel.Id);
                await LoadUsersAsync();
                
                MessageBox.Show(
                    "User deleted successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error deleting user: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task RefreshUsers()
    {
        await LoadUsersAsync();
    }

    private void CloseSidePanel()
    {
        IsSidePanelOpen = false;
        UserSidePanelViewModel = null;
    }
}
