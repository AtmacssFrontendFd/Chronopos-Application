using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a currency (matches currency table)
/// </summary>
public class Currency
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CurrencyName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Currency logo image file path (relative to application data folder)
    /// </summary>
    [StringLength(500)]
    public string? ImagePath { get; set; }
    
    public decimal ExchangeRate { get; set; } = 1.0000m;
    
    public bool IsDefault { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
