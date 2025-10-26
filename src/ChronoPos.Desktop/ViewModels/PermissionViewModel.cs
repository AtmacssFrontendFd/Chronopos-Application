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

public partial class PermissionViewModel : ObservableObject
{
    private readonly IPermissionService _permissionService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<PermissionDto> permissions = new();

    [ObservableProperty]
    private PermissionDto? selectedPermission;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private PermissionSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool showActiveOnly = false;

    private readonly ICollectionView _filteredPermissionsView;

    public ICollectionView FilteredPermissions => _filteredPermissionsView;

    public bool HasPermissions => Permissions.Count > 0;
    public int TotalPermissions => Permissions.Count;

    public PermissionViewModel(IPermissionService permissionService, Action? navigateBack = null)
    {
        _permissionService = permissionService;
        _navigateBack = navigateBack ?? (() => { }); // Default empty action if not provided
        
        // Initialize filtered view
        _filteredPermissionsView = CollectionViewSource.GetDefaultView(Permissions);
        _filteredPermissionsView.Filter = FilterPermissions;

        // Initialize commands
        LoadPermissionsCommand = new AsyncRelayCommand(LoadPermissionsAsync);
        AddPermissionCommand = new RelayCommand(ShowAddPermissionPanel);
        EditPermissionCommand = new RelayCommand<PermissionDto?>(ShowEditPermissionPanel);
        DeletePermissionCommand = new AsyncRelayCommand<PermissionDto?>(DeletePermissionAsync);
        FilterActiveCommand = new RelayCommand(FilterActive);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshDataCommand = new AsyncRelayCommand(LoadPermissionsAsync);
        ToggleActiveCommand = new AsyncRelayCommand<PermissionDto?>(ToggleActiveAsync);
        ViewPermissionDetailsCommand = new RelayCommand<PermissionDto?>(ViewPermissionDetails);
        BackCommand = new RelayCommand(GoBack);
        CloseSidePanelCommand = new RelayCommand(CloseSidePanel);

        // Subscribe to search text changes
        PropertyChanged += OnPropertyChanged;
        
        // Load permissions on startup
        _ = LoadPermissionsAsync();
    }

    public IAsyncRelayCommand LoadPermissionsCommand { get; }
    public RelayCommand AddPermissionCommand { get; }
    public RelayCommand<PermissionDto?> EditPermissionCommand { get; }
    public IAsyncRelayCommand<PermissionDto?> DeletePermissionCommand { get; }
    public RelayCommand FilterActiveCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public IAsyncRelayCommand<PermissionDto?> ToggleActiveCommand { get; }
    public RelayCommand<PermissionDto?> ViewPermissionDetailsCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand CloseSidePanelCommand { get; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(ShowActiveOnly))
        {
            _filteredPermissionsView.Refresh();
        }
        else if (e.PropertyName == nameof(Permissions))
        {
            OnPropertyChanged(nameof(HasPermissions));
            OnPropertyChanged(nameof(TotalPermissions));
            _filteredPermissionsView.Refresh();
        }
    }

    private bool FilterPermissions(object obj)
    {
        if (obj is not PermissionDto permission) return false;
        
        // Apply active filter
        if (ShowActiveOnly && permission.Status != "Active") return false;
        
        // Apply search filter
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        
        return permission.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               permission.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrEmpty(permission.ScreenName) && permission.ScreenName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }

    private async Task LoadPermissionsAsync()
    {
        IsLoading = true;
        try
        {
            var allPermissions = await _permissionService.GetAllAsync();
            
            // Clear and repopulate the existing collection to maintain the filtered view
            Permissions.Clear();
            foreach (var permission in allPermissions)
            {
                Permissions.Add(permission);
            }
            
            StatusMessage = $"Loaded {Permissions.Count} permissions.";
            
            // Refresh the filtered view
            _filteredPermissionsView.Refresh();
            OnPropertyChanged(nameof(HasPermissions));
            OnPropertyChanged(nameof(TotalPermissions));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading permissions: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddPermissionPanel()
    {
        SidePanelViewModel = new PermissionSidePanelViewModel(
            _permissionService,
            OnPermissionSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = "Add new permission...";
    }

    private void ShowEditPermissionPanel(PermissionDto? permission)
    {
        if (permission == null) return;
        
        SidePanelViewModel = new PermissionSidePanelViewModel(
            _permissionService,
            permission,
            OnPermissionSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = $"Edit permission '{permission.Name}'...";
    }

    private void OnPermissionSaved(bool success)
    {
        if (success)
        {
            CloseSidePanel();
            // Reload permissions to reflect changes
            _ = LoadPermissionsAsync();
        }
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Permission editing cancelled.";
    }

    private async Task DeletePermissionAsync(PermissionDto? permission)
    {
        if (permission == null) return;
        
        var result = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the permission '{permission.Name}'?",
            ConfirmationDialog.DialogType.Warning).ShowDialog();
            
        if (result != true) return;
        
        try
        {
            var success = await _permissionService.DeleteAsync(permission.PermissionId);
            if (success)
            {
                Permissions.Remove(permission);
                StatusMessage = $"Permission '{permission.Name}' deleted.";
                _filteredPermissionsView.Refresh();
                OnPropertyChanged(nameof(HasPermissions));
                OnPropertyChanged(nameof(TotalPermissions));
            }
            else
            {
                StatusMessage = $"Failed to delete permission '{permission.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting permission: {ex.Message}";
            new MessageDialog("Delete Error", ex.Message, MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task ToggleActiveAsync(PermissionDto? permission)
    {
        if (permission == null) return;
        
        try
        {
            var newStatus = permission.Status == "Active" ? "Inactive" : "Active";
            
            var updateDto = new UpdatePermissionDto
            {
                Name = permission.Name,
                Code = permission.Code,
                ScreenName = permission.ScreenName,
                TypeMatrix = permission.TypeMatrix,
                IsParent = permission.IsParent,
                ParentPermissionId = permission.ParentPermissionId,
                Status = newStatus
            };
            
            var updatedPermission = await _permissionService.UpdateAsync(permission.PermissionId, updateDto);
            if (updatedPermission != null)
            {
                permission.Status = updatedPermission.Status;
                StatusMessage = $"Permission '{permission.Name}' {(permission.Status == "Active" ? "activated" : "deactivated")}.";
            }
            else
            {
                StatusMessage = $"Failed to update permission '{permission.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating permission: {ex.Message}";
        }
    }

    private void ViewPermissionDetails(PermissionDto? permission)
    {
        if (permission == null) return;
        StatusMessage = $"Viewing details for permission '{permission.Name}'...";
        // TODO: Implement permission details view
    }

    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        StatusMessage = ShowActiveOnly ? "Showing active permissions only." : "Showing all permissions.";
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared.";
    }

    private void GoBack()
    {
        if (_navigateBack != null)
        {
            _navigateBack.Invoke();
        }
        else
        {
            StatusMessage = "Navigation back not configured.";
        }
    }
}
