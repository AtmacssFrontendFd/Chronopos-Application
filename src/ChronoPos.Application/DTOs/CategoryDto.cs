using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Category operations
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public int? ParentCategoryId { get; set; }
    
    public string ParentCategoryName { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; } = 0;
    
    public int ProductCount { get; set; }
    
    // Arabic name for translation
    public string NameArabic { get; set; } = string.Empty;
    
    // Selected discount IDs for CategoryDiscounts mapping
    public List<int> SelectedDiscountIds { get; set; } = new();
}
