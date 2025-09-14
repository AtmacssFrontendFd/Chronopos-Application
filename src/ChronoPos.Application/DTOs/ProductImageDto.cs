using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProductImage operations
/// </summary>
public class ProductImageDto
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? AltText { get; set; }
    
    public int SortOrder { get; set; } = 0;
    
    public bool IsPrimary { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // For UI binding
    public bool IsNew { get; set; } = true;
}
