using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents stock alerts for low stock notifications
/// </summary>
public class StockAlert
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    public StockAlertType AlertType { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Message { get; set; } = string.Empty;
    
    public int CurrentStock { get; set; }
    
    public int TriggerLevel { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReadDate { get; set; }
    
    [StringLength(100)]
    public string? ReadBy { get; set; }
    
    // Navigation property
    public virtual Product Product { get; set; } = null!;
}

/// <summary>
/// Types of stock alerts
/// </summary>
public enum StockAlertType
{
    LowStock = 1,           // Stock below reorder level
    OutOfStock = 2,         // Stock is zero
    Overstock = 3,          // Stock above maximum level
    ExpiryWarning = 4,      // Items nearing expiry
    NegativeStock = 5       // Stock went negative
}
