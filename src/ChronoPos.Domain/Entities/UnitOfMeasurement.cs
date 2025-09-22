using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents units of measurement for products with enhanced schema
/// </summary>
public class UnitOfMeasurement
{
    public long Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(10)]
    public string? Abbreviation { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Type { get; set; } = string.Empty; // Base or Derived
    
    [StringLength(50)]
    public string? CategoryTitle { get; set; }
    
    public long? BaseUomId { get; set; }
    
    public decimal? ConversionFactor { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public bool IsActive { get; set; } = true;
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual UnitOfMeasurement? BaseUom { get; set; }
    public virtual ICollection<UnitOfMeasurement> DerivedUnits { get; set; } = new List<UnitOfMeasurement>();
    
    // User navigation properties
    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
    public virtual User? Deleter { get; set; }
}
