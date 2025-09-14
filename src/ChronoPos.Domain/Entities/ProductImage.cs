using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product image (matches product_images table)
/// </summary>
public class ProductImage
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? AltText { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Product Product { get; set; } = null!;
}
