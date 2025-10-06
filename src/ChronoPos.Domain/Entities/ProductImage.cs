using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product image (matches product_images table)
/// </summary>
public class ProductImage
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    /// <summary>
    /// Optional ProductUnit ID - allows images to be specific to a product unit (UOM)
    /// </summary>
    public int? ProductUnitId { get; set; }
    
    /// <summary>
    /// Optional ProductGroup ID - allows images to be associated with product groups
    /// </summary>
    public int? ProductGroupId { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? AltText { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ProductUnit? ProductUnit { get; set; }
}
