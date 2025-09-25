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
    
    // Display properties for UI binding
    public string DisplayName => Name;
    public string NameArabicDisplay => NameArabic ?? "-";
    public string DescriptionDisplay => Description ?? "-";
    public string StatusDisplay => IsActive ? "Active" : "Inactive";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    public string ProductCountDisplay => $"{ProductCount} products";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// DTO for creating a new brand
/// </summary>
public class CreateBrandDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameArabic { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing brand
/// </summary>
public class UpdateBrandDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameArabic { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
