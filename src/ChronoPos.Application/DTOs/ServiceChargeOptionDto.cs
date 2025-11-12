using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ServiceChargeOption operations
/// </summary>
public class ServiceChargeOptionDto
{
    public int Id { get; set; }
    
    [Required]
    public int ServiceChargeTypeId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public decimal? Cost { get; set; }
    
    public decimal? Price { get; set; }
    
    public int? LanguageId { get; set; }
    
    public bool Status { get; set; } = true;
    
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    // Related entity properties
    public string? ServiceChargeTypeName { get; set; }
    public string? LanguageName { get; set; }
    
    // Display properties for UI binding
    public string DisplayName => Name;
    public string ServiceChargeTypeDisplay => ServiceChargeTypeName ?? "-";
    public string LanguageDisplay => LanguageName ?? "-";
    public string CostDisplay => Cost.HasValue ? $"{Cost:C2}" : "-";
    public string PriceDisplay => Price.HasValue ? $"{Price:C2}" : "-";
    public string StatusDisplay => Status ? "Active" : "Inactive";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// DTO for creating a new service charge option
/// </summary>
public class CreateServiceChargeOptionDto
{
    [Required]
    public int ServiceChargeTypeId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public decimal? Cost { get; set; }
    
    public decimal? Price { get; set; }
    
    public int? LanguageId { get; set; }
    
    public bool Status { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing service charge option
/// </summary>
public class UpdateServiceChargeOptionDto
{
    [Required]
    public int ServiceChargeTypeId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public decimal? Cost { get; set; }
    
    public decimal? Price { get; set; }
    
    public int? LanguageId { get; set; }
    
    public bool Status { get; set; } = true;
}
