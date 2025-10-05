using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents goods replacement/transfer return transaction
/// </summary>
public class GoodsReplace
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(30)]
    public string ReplaceNo { get; set; } = string.Empty;
    
    [Required]
    public int SupplierId { get; set; }
    
    [Required]
    public int StoreId { get; set; }
    
    /// <summary>
    /// Links to GoodsReturns.Id if replacement is against a return
    /// </summary>
    public int? ReferenceReturnId { get; set; }
    
    [Required]
    public DateTime ReplaceDate { get; set; }
    
    public decimal TotalAmount { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending / Posted / Cancelled
    
    public string? Remarks { get; set; }
    
    [Required]
    public int CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Supplier? Supplier { get; set; }
    public virtual Store? Store { get; set; }
    public virtual GoodsReturn? ReferenceReturn { get; set; }
    public virtual User? Creator { get; set; }
    public virtual ICollection<GoodsReplaceItem> Items { get; set; } = new List<GoodsReplaceItem>();
}
