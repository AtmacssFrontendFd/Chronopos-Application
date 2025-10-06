using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product batch for inventory tracking with expiry dates and cost information
/// </summary>
public class ProductBatch
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string BatchNo { get; set; } = string.Empty;
    
    public DateTime? ManufactureDate { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    public decimal Quantity { get; set; } = 0;
    
    [Required]
    public long UomId { get; set; }
    
    public decimal? CostPrice { get; set; }
    
    public decimal? LandedCost { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Navigation Properties
    public virtual Product? Product { get; set; }
    public virtual UnitOfMeasurement? Uom { get; set; }
}