using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Permission operations
/// </summary>
public class PermissionDto
{
    public int PermissionId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    public string? ScreenName { get; set; }
    
    public string? TypeMatrix { get; set; }
    
    public bool IsParent { get; set; } = false;
    
    public int? ParentPermissionId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public string? ParentPermissionName { get; set; }
    
    public int ChildPermissionCount { get; set; } = 0;
    
    // Display properties for UI binding
    public string DisplayName => Name;
    public string CodeDisplay => Code;
    public string ScreenNameDisplay => ScreenName ?? "-";
    public string TypeMatrixDisplay => TypeMatrix ?? "-";
    public string StatusDisplay => Status;
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    public string IsParentDisplay => IsParent ? "Parent" : "Child";
    public string ParentPermissionNameDisplay => ParentPermissionName ?? "-";
    public string ParentDisplay => ParentPermissionName ?? "-";
    public string ChildCountDisplay => IsParent ? $"{ChildPermissionCount} children" : "-";
    public bool IsActive => Status == "Active";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// DTO for creating a new permission
/// </summary>
public class CreatePermissionDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    public string? ScreenName { get; set; }
    
    public string? TypeMatrix { get; set; }
    
    public bool IsParent { get; set; } = false;
    
    public int? ParentPermissionId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing permission
/// </summary>
public class UpdatePermissionDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    public string? ScreenName { get; set; }
    
    public string? TypeMatrix { get; set; }
    
    public bool IsParent { get; set; } = false;
    
    public int? ParentPermissionId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? UpdatedBy { get; set; }
}
