using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product-tax relationship
/// </summary>
public class ProductTax
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    public int TaxId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual Tax Tax { get; set; } = null!;
}
