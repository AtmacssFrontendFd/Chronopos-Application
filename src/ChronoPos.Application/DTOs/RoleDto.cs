using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Role operations
/// </summary>
public class RoleDto
{
    public int RoleId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public int PermissionCount { get; set; } = 0;
    
    // Display properties for UI binding
    public string DisplayName => RoleName;
    public string DescriptionDisplay => Description ?? "-";
    public string StatusDisplay => Status;
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    public string PermissionCountDisplay => $"{PermissionCount} permissions";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return RoleName;
    }
}

/// <summary>
/// DTO for creating a new role
/// </summary>
public class CreateRoleDto
{
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing role
/// </summary>
public class UpdateRoleDto
{
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? UpdatedBy { get; set; }
}
