using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a service charge type (matches service_charge_type table)
/// Defines different categories or types of service charges (e.g., delivery, maintenance, setup fees)
/// </summary>
public class ServiceChargeType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ChargeOptionScope { get; set; }
    
    public bool IsDefault { get; set; } = false;
    
    public bool Status { get; set; } = true;
    
    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual User? DeletedByUser { get; set; }
    public virtual ICollection<ServiceChargeOption> ServiceChargeOptions { get; set; } = new List<ServiceChargeOption>();
}
