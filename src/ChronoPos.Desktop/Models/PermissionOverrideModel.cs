using ChronoPos.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronoPos.Desktop.Models;

/// <summary>
/// Model for permission override in user form
/// </summary>
public partial class PermissionOverrideModel : ObservableObject
{
    [ObservableProperty]
    private int _permissionId;

    [ObservableProperty]
    private string _permissionName = string.Empty;

    [ObservableProperty]
    private string _screenName = string.Empty;

    [ObservableProperty]
    private DateTime? _validFrom;

    [ObservableProperty]
    private DateTime? _validTo;

    [ObservableProperty]
    private bool _isAllowed = true;

    public static PermissionOverrideModel FromPermissionDto(PermissionDto permission, DateTime? validFrom = null, DateTime? validTo = null)
    {
        return new PermissionOverrideModel
        {
            PermissionId = permission.PermissionId,
            PermissionName = permission.Name,
            ScreenName = permission.ScreenName ?? permission.Name,
            ValidFrom = validFrom,
            ValidTo = validTo,
            IsAllowed = true
        };
    }
}
