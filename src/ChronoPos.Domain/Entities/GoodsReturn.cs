using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a goods return transaction in the system
/// </summary>
public class GoodsReturn
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string ReturnNo { get; set; } = string.Empty; // e.g., GR-2025-0001
    
    [Required]
    public long SupplierId { get; set; }
    
    [Required]
    public int StoreId { get; set; }
    
    public int? ReferenceGrnId { get; set; } // Optional: link to original GRN
    
    [Required]
    public DateTime ReturnDate { get; set; }
    
    public decimal TotalAmount { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Posted, Cancelled
    
    [StringLength(255)]
    public string? Remarks { get; set; }
    
    [Required]
    public int CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Supplier? Supplier { get; set; }
    public virtual Store? Store { get; set; }
    public virtual GoodsReceived? ReferenceGrn { get; set; }
    public virtual User? Creator { get; set; }
    public virtual ICollection<GoodsReturnItem> Items { get; set; } = new List<GoodsReturnItem>();
}