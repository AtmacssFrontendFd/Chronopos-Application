using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a service charge option (matches service_charge_option table)
/// Holds individual charge options linked to each service charge type (supports cost, price, and localization)
/// </summary>
public class ServiceChargeOption
{
    public int Id { get; set; }
    
    [Required]
    public int ServiceChargeTypeId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }
    
    public int? LanguageId { get; set; }
    
    public bool Status { get; set; } = true;
    
    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public virtual ServiceChargeType ServiceChargeType { get; set; } = null!;
    public virtual Language? Language { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual User? DeletedByUser { get; set; }
}
