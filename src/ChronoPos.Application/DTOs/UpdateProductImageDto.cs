using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for updating ProductImage
/// </summary>
public class UpdateProductImageDto
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
}