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
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNo { get; set; }
    public string? Address { get; set; }
    public string? RolePermissionName { get; set; }
    public int? RolePermissionId { get; set; }
    public int ShopId { get; set; }
    public bool ChangeAccess { get; set; }
    public int? ShiftTypeId { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? UaeId { get; set; }
    public DateTime? Dob { get; set; }
    public string? NationalityStatus { get; set; }
    public decimal? Salary { get; set; }
    
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
            Username = user.Username,
            Email = user.Email,
            PhoneNo = user.PhoneNo,
            Address = user.Address,
            RolePermissionName = user.RolePermissionName,
            RolePermissionId = user.RolePermissionId,
            ShopId = user.ShopId,
            ChangeAccess = user.ChangeAccess,
            ShiftTypeId = user.ShiftTypeId,
            AdditionalDetails = user.AdditionalDetails,
            UaeId = user.UaeId,
            Dob = user.Dob,
            NationalityStatus = user.NationalityStatus,
            Salary = user.Salary
        };
    }
}
