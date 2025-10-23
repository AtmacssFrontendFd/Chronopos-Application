using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ChronoPos.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for User Settings screen with tab navigation
/// </summary>
public partial class UserSettingsViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _pageTitle = "User Settings";

    [ObservableProperty]
    private string _selectedTab = "Profile"; // Profile, ManageAccess, Users, CompanyDetails, CompanySettings

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool _isProfileTabSelected = true;

    [ObservableProperty]
    private bool _isManageAccessTabSelected = false;

    [ObservableProperty]
    private bool _isUsersTabSelected = false;

    [ObservableProperty]
    private bool _isCompanyDetailsTabSelected = false;

    [ObservableProperty]
    private bool _isCompanySettingsTabSelected = false;

    [ObservableProperty]
    private ProfileViewModel? _profileViewModel;

    [ObservableProperty]
    private UsersManagementViewModel? _usersManagementViewModel;

    /// <summary>
    /// Action to navigate back to settings
    /// </summary>
    public Action? NavigateBackAction { get; set; }

    public UserSettingsViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        ChronoPos.Application.Logging.AppLogger.Log("=== UserSettingsViewModel Constructor STARTED ===");
        
        try
        {
            // Initialize ProfileViewModel
            var currentUserService = _serviceProvider.GetRequiredService<ICurrentUserService>();
            var roleService = _serviceProvider.GetRequiredService<IRoleService>();
            ProfileViewModel = new ProfileViewModel(currentUserService, roleService);

            // Initialize UsersManagementViewModel
            var userService = _serviceProvider.GetRequiredService<IUserService>();
            var permissionService = _serviceProvider.GetRequiredService<IPermissionService>();
            var rolePermissionRepository = _serviceProvider.GetRequiredService<ChronoPos.Domain.Interfaces.IRolePermissionRepository>();
            var userPermissionOverrideService = _serviceProvider.GetRequiredService<IUserPermissionOverrideService>();
            UsersManagementViewModel = new UsersManagementViewModel(userService, roleService, permissionService, rolePermissionRepository, userPermissionOverrideService);

            // Initialize with Profile tab
            ChronoPos.Application.Logging.AppLogger.Log("UserSettingsViewModel: Calling SelectTab(Profile)");
            SelectTab("Profile");
            ChronoPos.Application.Logging.AppLogger.Log("UserSettingsViewModel: SelectTab completed");
            
            ChronoPos.Application.Logging.AppLogger.Log("=== UserSettingsViewModel Constructor COMPLETED ===");
        }
        catch (Exception ex)
        {
            ChronoPos.Application.Logging.AppLogger.Log($"!!! UserSettingsViewModel Constructor ERROR !!!: {ex.Message}");
            ChronoPos.Application.Logging.AppLogger.Log($"!!! Stack Trace !!!: {ex.StackTrace}");
            throw;
        }
    }

    [RelayCommand]
    private void SelectTab(string tabName)
    {
        ChronoPos.Application.Logging.AppLogger.Log($"UserSettingsViewModel: SelectTab called with tabName={tabName}");
        
        SelectedTab = tabName;

        // Update tab selection flags
        IsProfileTabSelected = tabName == "Profile";
        IsManageAccessTabSelected = tabName == "ManageAccess";
        IsUsersTabSelected = tabName == "Users";
        IsCompanyDetailsTabSelected = tabName == "CompanyDetails";
        IsCompanySettingsTabSelected = tabName == "CompanySettings";
        
        ChronoPos.Application.Logging.AppLogger.Log($"UserSettingsViewModel: Tab selection updated. SelectedTab={SelectedTab}");
    }

    [RelayCommand]
    private void GoBack()
    {
        ChronoPos.Application.Logging.AppLogger.Log("UserSettingsViewModel: GoBack called");
        NavigateBackAction?.Invoke();
    }
}
