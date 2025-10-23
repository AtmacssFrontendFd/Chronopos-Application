using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a permission in the system
/// </summary>
public class Permission
{
    public int PermissionId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ScreenName { get; set; }
    
    [StringLength(20)]
    public string? TypeMatrix { get; set; }
    
    public bool IsParent { get; set; } = false;
    
    public int? ParentPermissionId { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
    public virtual User? Deleter { get; set; }
    public virtual Permission? ParentPermission { get; set; }
    public virtual ICollection<Permission> ChildPermissions { get; set; } = new List<Permission>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserPermissionOverride> UserPermissionOverrides { get; set; } = new List<UserPermissionOverride>();
}
