using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents current stock levels for products per store
/// </summary>
public class StockLevel
{
    public int Id { get; set; }
    
    public int StoreId { get; set; }
    
    public int ProductId { get; set; }
    
    // Stock Quantities
    public decimal CurrentStock { get; set; } = 0;
    
    public decimal ReservedStock { get; set; } = 0;
    
    // Computed property for available stock
    public decimal AvailableStock => CurrentStock - ReservedStock;
    
    // Cost Information
    public decimal AverageCost { get; set; } = 0;
    
    public decimal LastCost { get; set; } = 0;
    
    // Audit
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
