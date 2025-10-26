using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class RoleViewModel : ObservableObject
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<RoleDto> roles = new();

    [ObservableProperty]
    private RoleDto? selectedRole;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private RoleSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool showActiveOnly = false;

    private readonly ICollectionView _filteredRolesView;

    public ICollectionView FilteredRoles => _filteredRolesView;

    public bool HasRoles => Roles.Count > 0;
    public int TotalRoles => Roles.Count;

    public RoleViewModel(IRoleService roleService, IPermissionService permissionService, Action? navigateBack = null)
    {
        _roleService = roleService;
        _permissionService = permissionService;
        _navigateBack = navigateBack;
        
        // Initialize filtered view
        _filteredRolesView = CollectionViewSource.GetDefaultView(Roles);
        _filteredRolesView.Filter = FilterRoles;

        // Initialize commands
        LoadRolesCommand = new AsyncRelayCommand(LoadRolesAsync);
        AddRoleCommand = new RelayCommand(ShowAddRolePanel);
        EditRoleCommand = new RelayCommand<RoleDto?>(ShowEditRolePanel);
        DeleteRoleCommand = new AsyncRelayCommand<RoleDto?>(DeleteRoleAsync);
        FilterActiveCommand = new RelayCommand(FilterActive);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshDataCommand = new AsyncRelayCommand(LoadRolesAsync);
        ToggleActiveCommand = new AsyncRelayCommand<RoleDto?>(ToggleActiveAsync);
        BackCommand = new RelayCommand(GoBack);
        CloseSidePanelCommand = new RelayCommand(CloseSidePanel);

        // Subscribe to search text changes
        PropertyChanged += OnPropertyChanged;
        
        // Load roles on startup
        _ = LoadRolesAsync();
    }

    public IAsyncRelayCommand LoadRolesCommand { get; }
    public RelayCommand AddRoleCommand { get; }
    public RelayCommand<RoleDto?> EditRoleCommand { get; }
    public IAsyncRelayCommand<RoleDto?> DeleteRoleCommand { get; }
    public RelayCommand FilterActiveCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public IAsyncRelayCommand<RoleDto?> ToggleActiveCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand CloseSidePanelCommand { get; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(ShowActiveOnly))
        {
            _filteredRolesView.Refresh();
        }
        else if (e.PropertyName == nameof(Roles))
        {
            OnPropertyChanged(nameof(HasRoles));
            OnPropertyChanged(nameof(TotalRoles));
        }
    }

    private bool FilterRoles(object obj)
    {
        if (obj is not RoleDto role) return false;

        // Filter by active status
        if (ShowActiveOnly && role.Status != "Active")
            return false;

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            return role.RoleName.ToLower().Contains(searchLower) ||
                   (role.Description?.ToLower().Contains(searchLower) ?? false);
        }

        return true;
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;

            var rolesData = await _roleService.GetAllAsync();
            Roles.Clear();
            
            foreach (var role in rolesData.OrderBy(r => r.RoleName))
            {
                Roles.Add(role);
            }

            OnPropertyChanged(nameof(HasRoles));
            OnPropertyChanged(nameof(TotalRoles));
            
            StatusMessage = $"Loaded {Roles.Count} role(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading roles: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddRolePanel()
    {
        SidePanelViewModel = new RoleSidePanelViewModel(_roleService, _permissionService, OnSidePanelClosed);
        IsSidePanelVisible = true;
    }

    private void ShowEditRolePanel(RoleDto? role)
    {
        if (role == null) return;

        SidePanelViewModel = new RoleSidePanelViewModel(_roleService, _permissionService, OnSidePanelClosed, role);
        IsSidePanelVisible = true;
    }

    private async Task DeleteRoleAsync(RoleDto? role)
    {
        if (role == null) return;

        var result = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the role '{role.RoleName}'?",
            ConfirmationDialog.DialogType.Warning).ShowDialog();

        if (result != true) return;

        try
        {
            IsLoading = true;
            StatusMessage = "Deleting role...";

            await _roleService.DeleteAsync(role.RoleId);

            StatusMessage = $"Role '{role.RoleName}' deleted successfully";
            await LoadRolesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting role: {ex.Message}";
            new MessageDialog("Error", ex.Message, MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        _filteredRolesView.Refresh();
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        _filteredRolesView.Refresh();
    }

    private async Task ToggleActiveAsync(RoleDto? role)
    {
        if (role == null) return;

        try
        {
            IsLoading = true;
            var newStatus = role.Status == "Active" ? "Inactive" : "Active";

            var updateDto = new UpdateRoleDto
            {
                RoleName = role.RoleName,
                Description = role.Description,
                Status = newStatus
            };

            await _roleService.UpdateAsync(role.RoleId, updateDto);
            StatusMessage = $"Role status updated to {newStatus}";
            await LoadRolesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating status: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GoBack()
    {
        _navigateBack?.Invoke();
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
    }

    private void OnSidePanelClosed(bool shouldRefresh)
    {
        CloseSidePanel();
        if (shouldRefresh)
        {
            _ = LoadRolesAsync();
        }
    }
}
