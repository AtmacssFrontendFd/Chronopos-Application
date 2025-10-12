using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;

namespace ChronoPos.Desktop.Models;

/// <summary>
/// Model for displaying user with their role permissions
/// </summary>
public class UserWithPermissionsModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? RolePermissionName { get; set; }
    public int? RolePermissionId { get; set; }
    
    /// <summary>
    /// Permissions from the user's role
    /// </summary>
    public ObservableCollection<PermissionDto> RolePermissions { get; set; } = new();
    
    /// <summary>
    /// Additional permission overrides for this user
    /// </summary>
    public ObservableCollection<UserPermissionOverrideDto> PermissionOverrides { get; set; } = new();
    
    /// <summary>
    /// All permissions combined (for backward compatibility)
    /// </summary>
    public ObservableCollection<PermissionDto> Permissions { get; set; } = new();

    /// <summary>
    /// Creates a UserWithPermissionsModel from a UserDto
    /// </summary>
    public static UserWithPermissionsModel FromUserDto(UserDto user)
    {
        return new UserWithPermissionsModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            RolePermissionName = user.RolePermissionName,
            RolePermissionId = user.RolePermissionId
        };
    }
}
