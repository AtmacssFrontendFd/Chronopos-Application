using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents multiple units of measurement for a single product with pricing and cost information
/// This table allows products to have multiple UOMs with different pricing structures
/// </summary>
public class ProductUnit
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public long UnitId { get; set; }
    
    [StringLength(100)]
    public string Sku { get; set; } = string.Empty;
    
    [Required]
    public int QtyInUnit { get; set; } = 1;
    
    [Required]
    public decimal CostOfUnit { get; set; }
    
    [Required]
    public decimal PriceOfUnit { get; set; }
    
    public int? SellingPriceId { get; set; }
    
    [StringLength(50)]
    public string PriceType { get; set; } = "Retail";
    
    [Required]
    public bool DiscountAllowed { get; set; } = false;
    
    [Required]
    public bool IsBase { get; set; } = false;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual UnitOfMeasurement Unit { get; set; } = null!;
    
    // Computed Properties
    public decimal Markup => CostOfUnit > 0 ? ((PriceOfUnit - CostOfUnit) / CostOfUnit) * 100 : 0;
    public string DisplayName => $"{Unit?.Name} ({QtyInUnit})";
    public string PriceDisplay => $"{PriceOfUnit:C}";
    public string CostDisplay => $"{CostOfUnit:C}";
}