using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a customer group for pricing and discount management
/// Corresponds to customers_groups table
/// </summary>
public class CustomerGroup
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameAr { get; set; }
    
    public long? SellingPriceTypeId { get; set; }
    
    public int? DiscountId { get; set; }
    
    public decimal? DiscountValue { get; set; }
    
    public decimal? DiscountMaxValue { get; set; }
    
    public bool IsPercentage { get; set; } = false;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    // Audit fields
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation properties
    public virtual SellingPriceType? SellingPriceType { get; set; }
    
    public virtual Discount? Discount { get; set; }
    
    public virtual User? CreatedByUser { get; set; }
    
    public virtual User? UpdatedByUser { get; set; }
    
    public virtual User? DeletedByUser { get; set; }
    
    // Collection of customers in this group
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}