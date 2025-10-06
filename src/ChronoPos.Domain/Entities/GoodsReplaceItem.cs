using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an item in a goods replacement transaction
/// </summary>
public class GoodsReplaceItem
{
    public int Id { get; set; }
    
    [Required]
    public int ReplaceId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [StringLength(50)]
    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    public decimal Rate { get; set; }
    
    // Amount is calculated as Quantity * Rate
    public decimal Amount => Quantity * Rate;
    
    /// <summary>
    /// Link to GoodsReturnItems.Id (optional)
    /// </summary>
    public int? ReferenceReturnItemId { get; set; }
    
    public string? RemarksLine { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual GoodsReplace? Replace { get; set; }
    public virtual Product? Product { get; set; }
    public virtual UnitOfMeasurement? Uom { get; set; }
    public virtual GoodsReturnItem? ReferenceReturnItem { get; set; }
}
