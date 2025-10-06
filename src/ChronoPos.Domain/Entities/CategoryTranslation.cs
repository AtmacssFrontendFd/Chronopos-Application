using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a category translation for multilingual support
/// </summary>
public class CategoryTranslation
{
    public int Id { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [Required]
    [StringLength(10)]
    public string LanguageCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Category Category { get; set; } = null!;
}
