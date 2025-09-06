using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a stock movement/transaction
/// </summary>
public class StockTransaction
{
    public int Id { get; set; }
    
    public int StoreId { get; set; }
    
    public int ProductId { get; set; }
    
    // Movement Details
    [Required]
    public StockDirection MovementType { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    public decimal UnitCost { get; set; } = 0;
    
    // Reference Information
    [StringLength(200)]
    public string? ReferenceNumber { get; set; }
    
    [StringLength(50)]
    public string? ReferenceType { get; set; } // INITIAL, PURCHASE, SALE, ADJUSTMENT
    
    public int? ReferenceId { get; set; } // Related document ID
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    // Additional tracking
    public decimal PreviousStock { get; set; } = 0;
    
    public decimal NewStock { get; set; } = 0;
    
    [StringLength(100)]
    public string? SupplierName { get; set; }
    
    [StringLength(50)]
    public string? BatchNumber { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    // Audit
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}

/// <summary>
/// Direction of stock movement
/// </summary>
public enum StockDirection
{
    IN = 1,          // Stock increase
    OUT = 2,         // Stock decrease  
    ADJUSTMENT = 3   // Stock adjustment
}
