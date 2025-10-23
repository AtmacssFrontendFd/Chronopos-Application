using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an item in a goods return transaction
/// </summary>
public class GoodsReturnItem
{
    public int Id { get; set; }
    
    [Required]
    public int ReturnId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    public int? BatchId { get; set; } // FK to product_batches.id (optional)
    
    [StringLength(50)]
    public string? BatchNo { get; set; } // For reference / manual entry
    
    public DateTime? ExpiryDate { get; set; } // Optional, auto-filled from batch
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public long UomId { get; set; } // Unit of Measure
    
    [Required]
    public decimal CostPrice { get; set; } // Per unit
    
    public decimal LineTotal { get; set; } // Will be calculated as quantity * cost_price
    
    [StringLength(255)]
    public string? Reason { get; set; } // Damaged, expired, etc.
    
    // Replacement tracking fields
    public decimal AlreadyReplacedQuantity { get; set; } = 0; // Tracks how much has been replaced
    
    public bool IsTotallyReplaced { get; set; } = false; // True when AlreadyReplacedQuantity == Quantity
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual GoodsReturn? Return { get; set; }
    public virtual Product? Product { get; set; }
    public virtual ProductBatch? Batch { get; set; }
    public virtual UnitOfMeasurement? Uom { get; set; }
}