using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for RolePermission operations
/// </summary>
public class RolePermissionDto
{
    public int RolePermissionId { get; set; }
    
    public int RoleId { get; set; }
    
    public int PermissionId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public string? RoleName { get; set; }
    
    public string? PermissionName { get; set; }
    
    // Display properties for UI binding
    public string RoleDisplay => RoleName ?? "-";
    public string PermissionDisplay => PermissionName ?? "-";
    public string StatusDisplay => Status;
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
}

/// <summary>
/// DTO for creating a new role-permission assignment
/// </summary>
public class CreateRolePermissionDto
{
    [Required]
    public int RoleId { get; set; }
    
    [Required]
    public int PermissionId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing role-permission assignment
/// </summary>
public class UpdateRolePermissionDto
{
    public string Status { get; set; } = "Active";
    
    public int? UpdatedBy { get; set; }
}
