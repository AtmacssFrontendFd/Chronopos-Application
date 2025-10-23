using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a goods received transaction (GRN) in the system
/// </summary>
public class GoodsReceived
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string GrnNo { get; set; } = string.Empty;
    
    [Required]
    public long SupplierId { get; set; }
    
    [Required]
    public int StoreId { get; set; }
    
    [StringLength(50)]
    public string? InvoiceNo { get; set; }
    
    public DateTime? InvoiceDate { get; set; }
    
    [Required]
    public DateTime ReceivedDate { get; set; } = DateTime.Today;
    
    [Column(TypeName = "decimal(12,2)")]
    public decimal TotalAmount { get; set; } = 0;
    
    [StringLength(255)]
    public string? Remarks { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Posted, Cancelled
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Supplier? Supplier { get; set; }
    public virtual Store? Store { get; set; }
    public virtual ICollection<GoodsReceivedItem> Items { get; set; } = new List<GoodsReceivedItem>();
}