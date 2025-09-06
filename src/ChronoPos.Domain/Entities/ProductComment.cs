using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product comment
/// </summary>
public class ProductComment
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string CreatedBy { get; set; } = "System";
    
    // Navigation property
    public virtual Product Product { get; set; } = null!;
}
