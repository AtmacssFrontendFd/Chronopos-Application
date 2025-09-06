using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an individual product adjustment within a stock adjustment transaction
/// </summary>
public class StockAdjustmentItem
{
    public int Id { get; set; }
    
    [Required]
    public int AdjustmentId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int UomId { get; set; }
    
    [StringLength(50)]
    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    public decimal QuantityBefore { get; set; }
    
    [Required]
    public decimal QuantityAfter { get; set; }
    
    [Required]
    public decimal DifferenceQty { get; set; }
    
    [StringLength(100)]
    public string? ReasonLine { get; set; }
    
    public string? RemarksLine { get; set; }
    
    // Navigation Properties
    public virtual StockAdjustment? Adjustment { get; set; }
    public virtual Product? Product { get; set; }
    public virtual UnitOfMeasurement? Uom { get; set; }
}
