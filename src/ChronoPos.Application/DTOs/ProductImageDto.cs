using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProductImage operations
/// </summary>
public class ProductImageDto
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
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties for UI
    public string? ProductUnitName { get; set; }
    public string? ProductUnitSku { get; set; }
    
    // For UI binding
    public bool IsNew { get; set; } = true;
}
