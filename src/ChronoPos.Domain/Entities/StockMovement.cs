using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents all stock movements for audit trail purposes
/// </summary>
public class StockMovement
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public int? BatchId { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [Required]
    [StringLength(20)]
    public string MovementType { get; set; } = string.Empty; // 'Adjustment', 'Sale', 'Purchase', etc.
    
    [Required]
    public decimal Quantity { get; set; } // Can be positive or negative
    
    [Required]
    [StringLength(50)]
    public string ReferenceType { get; set; } = string.Empty; // 'Adjustment', 'Sale', etc.
    
    [Required]
    public int ReferenceId { get; set; }
    
    public int? LocationId { get; set; }
    
    public string? Notes { get; set; }
    
    [Required]
    public int CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Product? Product { get; set; }
    public virtual UnitOfMeasurement? Uom { get; set; }
    public virtual ShopLocation? Location { get; set; }
    public virtual User? Creator { get; set; }
}
