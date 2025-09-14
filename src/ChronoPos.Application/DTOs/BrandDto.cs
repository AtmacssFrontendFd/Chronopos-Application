using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Brand operations
/// </summary>
public class BrandDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameArabic { get; set; }
    
    public string? Description { get; set; }
    
    public string? LogoUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public int ProductCount { get; set; } = 0;
    
    // Display property for UI binding
    public string DisplayName => Name;
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}
