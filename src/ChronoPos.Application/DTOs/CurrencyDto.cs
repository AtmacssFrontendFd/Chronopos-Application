using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Currency operations
/// </summary>
public class CurrencyDto
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
    /// Currency logo image file path
    /// </summary>
    public string? ImagePath { get; set; }
    
    public decimal ExchangeRate { get; set; } = 1.0000m;
    
    public bool IsDefault { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Display properties for UI binding
    public string DisplayName => $"{CurrencyName} ({CurrencyCode})";
    public string CurrencyCodeDisplay => CurrencyCode.ToUpper();
    public string ExchangeRateDisplay => ExchangeRate.ToString("N4");
    public string StatusDisplay => IsDefault ? "Default" : "Active";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    public string FullDisplay => $"{Symbol} {CurrencyName} ({CurrencyCode})";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return DisplayName;
    }
}

/// <summary>
/// DTO for creating a new currency
/// </summary>
public class CreateCurrencyDto
{
    [Required(ErrorMessage = "Currency name is required")]
    [StringLength(100)]
    public string CurrencyName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Currency symbol is required")]
    [StringLength(10)]
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Currency logo image file path
    /// </summary>
    public string? ImagePath { get; set; }
    
    [Range(0.0001, 999999.9999, ErrorMessage = "Exchange rate must be between 0.0001 and 999999.9999")]
    public decimal ExchangeRate { get; set; } = 1.0000m;
    
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// DTO for updating an existing currency
/// </summary>
public class UpdateCurrencyDto
{
    [Required(ErrorMessage = "Currency name is required")]
    [StringLength(100)]
    public string CurrencyName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Currency symbol is required")]
    [StringLength(10)]
    public string Symbol { get; set; } = string.Empty;
    
    /// <summary>
    /// Currency logo image file path
    /// </summary>
    public string? ImagePath { get; set; }
    
    [Range(0.0001, 999999.9999, ErrorMessage = "Exchange rate must be between 0.0001 and 999999.9999")]
    public decimal ExchangeRate { get; set; } = 1.0000m;
    
    public bool IsDefault { get; set; } = false;
}
