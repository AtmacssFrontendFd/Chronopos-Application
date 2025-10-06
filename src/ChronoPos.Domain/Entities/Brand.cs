using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product brand (matches brand table)
/// </summary>
public class Brand
{
    public int Id { get; set; }
    
    public int? Deleted { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameArabic { get; set; }
    
    public string? Description { get; set; }
    
    public string? LogoUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
