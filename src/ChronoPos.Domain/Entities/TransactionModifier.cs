using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents modifiers applied to transaction products (e.g., extra cheese, no onions)
/// </summary>
public class TransactionModifier
{
    public int Id { get; set; }
    
    [Required]
    public int TransactionProductId { get; set; }
    
    [Required]
    public int ProductModifierId { get; set; }
    
    [Required]
    public decimal ExtraPrice { get; set; } = 0;
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual TransactionProduct TransactionProduct { get; set; } = null!;
    
    public virtual ProductModifier ProductModifier { get; set; } = null!;
    
    public virtual User? Creator { get; set; }
    
    /// <summary>
    /// Returns formatted extra price
    /// </summary>
    public string ExtraPriceDisplay => ExtraPrice > 0 ? $"+{ExtraPrice:N2}" : "No extra charge";
}
