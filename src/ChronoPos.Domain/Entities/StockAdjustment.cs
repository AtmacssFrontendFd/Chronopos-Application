using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a stock adjustment transaction in the system
/// </summary>
public class StockAdjustment
{
    public int AdjustmentId { get; set; }
    
    [Required]
    [StringLength(30)]
    public string AdjustmentNo { get; set; } = string.Empty;
    
    [Required]
    public DateTime AdjustmentDate { get; set; }
    
    [Required]
    public int StoreLocationId { get; set; }
    
    [Required]
    public int ReasonId { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Cancelled
    
    public string? Remarks { get; set; }
    
    [Required]
    public int CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual ShopLocation? StoreLocation { get; set; }
    public virtual StockAdjustmentReason? Reason { get; set; }
    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
    public virtual ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
