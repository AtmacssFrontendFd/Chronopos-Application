using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents predefined reasons for stock adjustments
/// </summary>
public class StockAdjustmentReason
{
    public int StockAdjustmentReasonsId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? Description { get; set; }
    
    [StringLength(255)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
    public virtual ICollection<StockAdjustment> Adjustments { get; set; } = new List<StockAdjustment>();
}
