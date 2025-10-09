using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents user-specific permission overrides
/// </summary>
public class UserPermissionOverride
{
    public int UserPermissionOverrideId { get; set; }
    
    public int UserId { get; set; }
    
    public int PermissionId { get; set; }
    
    public bool IsAllowed { get; set; } = true;
    
    public string? Reason { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidTo { get; set; }
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
    public virtual User? Deleter { get; set; }
}
