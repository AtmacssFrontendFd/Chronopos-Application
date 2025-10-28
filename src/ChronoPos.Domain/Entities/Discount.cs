using System.ComponentModel.DataAnnotations;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a discount entity (matches discounts table)
/// </summary>
public class Discount
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(150)]
    public string DiscountName { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string? DiscountNameAr { get; set; }
    
    [StringLength(150)]
    public string? DiscountDescription { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DiscountCode { get; set; } = string.Empty;
    
    // % or fixed amount
    public DiscountType DiscountType { get; set; }
    
    [Required]
    public decimal DiscountValue { get; set; }
    
    // Business rules
    public decimal? MaxDiscountAmount { get; set; }       // cap per discount
    public decimal? MinPurchaseAmount { get; set; }       // eligibility condition
    
    // Validity
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    // Scope of discount - now handled via many-to-many relationships
    public DiscountApplicableOn ApplicableOn { get; set; }
    
    // Multiple discount handling
    public int Priority { get; set; } = 0;              // higher = applied first
    public bool IsStackable { get; set; } = false;      // can combine with others?
    
    // System flags
    public bool IsActive { get; set; } = true;
    
    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    
    // Optional for multi-store/currency
    public int? StoreId { get; set; }
    
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    
    // Navigation Properties
    public virtual Store? Store { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual User? DeletedByUser { get; set; }
    
    // Many-to-Many relationship navigation properties
    public virtual ICollection<ProductDiscount> ProductDiscounts { get; set; } = new List<ProductDiscount>();
    public virtual ICollection<CategoryDiscount> CategoryDiscounts { get; set; } = new List<CategoryDiscount>();
    public virtual ICollection<CustomerDiscount> CustomerDiscounts { get; set; } = new List<CustomerDiscount>();
    
    // Helper properties for UI
    public string FormattedDiscountValue =>
        DiscountType == DiscountType.Percentage 
            ? $"{DiscountValue}%" 
            : $"{CurrencyCode} {DiscountValue:F2}";
    
    public bool IsCurrentlyActive =>
        IsActive && 
        DateTime.UtcNow >= StartDate && 
        DateTime.UtcNow <= EndDate &&
        DeletedAt == null;
    
    public string StatusDisplay =>
        !IsActive ? "Inactive" :
        DeletedAt != null ? "Deleted" :
        DateTime.UtcNow < StartDate ? "Scheduled" :
        DateTime.UtcNow > EndDate ? "Expired" :
        "Active";
}