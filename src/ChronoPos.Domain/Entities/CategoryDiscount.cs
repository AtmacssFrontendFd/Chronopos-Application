using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a mapping between categories and discounts (Many-to-Many relationship)
/// Maps to the category_discount table
/// </summary>
public class CategoryDiscount
{
    public int Id { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    public int DiscountsId { get; set; }
    
    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    
    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual Discount Discount { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
    public virtual User? DeletedByUser { get; set; }
    
    // Helper properties for validation and business logic
    public bool IsActive => DeletedAt == null;
    
    /// <summary>
    /// Check if this discount mapping is currently valid
    /// </summary>
    public bool IsCurrentlyValid =>
        IsActive && 
        Discount?.IsCurrentlyActive == true;
}