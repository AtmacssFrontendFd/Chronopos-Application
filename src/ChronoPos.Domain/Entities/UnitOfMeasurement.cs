using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents units of measurement for products
/// </summary>
public class UnitOfMeasurement
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string Abbreviation { get; set; } = string.Empty;
    
    public int? BaseUomId { get; set; }
    
    public decimal? ConversionFactor { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual UnitOfMeasurement? BaseUom { get; set; }
    public virtual ICollection<UnitOfMeasurement> DerivedUnits { get; set; } = new List<UnitOfMeasurement>();
}
