using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for displaying current user's profile
/// </summary>
public partial class ProfileViewModel : ObservableObject
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IRoleService _roleService;

    [ObservableProperty]
    private UserDto? _currentUser;

    [ObservableProperty]
    private string _roleName = "N/A";

    [ObservableProperty]
    private string _initials = "";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    public ProfileViewModel(ICurrentUserService currentUserService, IRoleService roleService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));

        _ = LoadUserProfileAsync();
    }

    private async Task LoadUserProfileAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading profile...";

            ChronoPos.Application.Logging.AppLogger.Log("ProfileViewModel: Loading current user profile");

            // Get current user from CurrentUserService
            CurrentUser = _currentUserService.CurrentUser;

            if (CurrentUser == null)
            {
                StatusMessage = "No user logged in";
                ChronoPos.Application.Logging.AppLogger.Log("ProfileViewModel: No user logged in");
                return;
            }

            ChronoPos.Application.Logging.AppLogger.Log($"ProfileViewModel: Loaded user {CurrentUser.FullName}, RolePermissionId={CurrentUser.RolePermissionId}");

            // Generate initials from full name
            Initials = GenerateInitials(CurrentUser.FullName);

            // Load role name
            await LoadRoleNameAsync();

            StatusMessage = "Profile loaded successfully";
            ChronoPos.Application.Logging.AppLogger.Log("ProfileViewModel: Profile loaded successfully");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading profile: {ex.Message}";
            ChronoPos.Application.Logging.AppLogger.Log($"ProfileViewModel Error: {ex.Message}");
            ChronoPos.Application.Logging.AppLogger.Log($"ProfileViewModel Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadRoleNameAsync()
    {
        try
        {
            if (CurrentUser == null || CurrentUser.RolePermissionId == 0)
            {
                RoleName = "No Role Assigned";
                return;
            }

            var role = await _roleService.GetByIdAsync(CurrentUser.RolePermissionId);
            if (role != null)
            {
                RoleName = role.RoleName;
                ChronoPos.Application.Logging.AppLogger.Log($"ProfileViewModel: Role name loaded: {RoleName}");
            }
            else
            {
                RoleName = "Unknown Role";
                ChronoPos.Application.Logging.AppLogger.Log("ProfileViewModel: Role not found");
            }
        }
        catch (Exception ex)
        {
            RoleName = "Error loading role";
            ChronoPos.Application.Logging.AppLogger.Log($"ProfileViewModel: Error loading role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RefreshProfile()
    {
        await _currentUserService.RefreshCurrentUserAsync();
        await LoadUserProfileAsync();
    }

    /// <summary>
    /// Generate initials from full name
    /// Examples: "manoj" -> "M", "manoj kumar" -> "MK"
    /// </summary>
    private string GenerateInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "";

        // Split name by spaces and take first letter of each word
        var words = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Join("", words.Select(w => char.ToUpper(w[0])));
        
        // Limit to 2 characters maximum
        return initials.Length > 2 ? initials.Substring(0, 2) : initials;
    }
}
