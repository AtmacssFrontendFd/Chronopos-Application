using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a user role in the system
/// </summary>
public class Role
{
    public int RoleId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
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
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
