using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product combination item linking ProductUnit and AttributeValue (matches product_combination_items table)
/// </summary>
public class ProductCombinationItem
{
    public int Id { get; set; }
    
    [Required]
    public int ProductUnitId { get; set; }
    
    [Required]
    public int AttributeValueId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ProductUnit ProductUnit { get; set; } = null!;
    public virtual ProductAttributeValue AttributeValue { get; set; } = null!;
}
