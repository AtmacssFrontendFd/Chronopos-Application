using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an individual item within a sale transaction
/// </summary>
public class SaleItem
{
    public int Id { get; set; }
    
    [Required]
    public int SaleId { get; set; }
    
    public Sale Sale { get; set; } = null!;
    
    [Required]
    public int ProductId { get; set; }
    
    public Product Product { get; set; } = null!;
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public decimal UnitPrice { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    /// <summary>
    /// Calculates the total amount for this sale item
    /// </summary>
    public decimal TotalAmount => (UnitPrice * Quantity) - DiscountAmount;
}
