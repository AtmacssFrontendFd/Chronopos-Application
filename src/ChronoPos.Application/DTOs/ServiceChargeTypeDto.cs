using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ServiceChargeType operations
/// </summary>
public class ServiceChargeTypeDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ChargeOptionScope { get; set; }
    
    public bool IsDefault { get; set; } = false;
    
    public bool Status { get; set; } = true;
    
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    
    public int OptionsCount { get; set; } = 0;
    
    // Display properties for UI binding
    public string DisplayName => Name;
    public string CodeDisplay => Code;
    public string ChargeOptionScopeDisplay => ChargeOptionScope ?? "-";
    public string StatusDisplay => Status ? "Active" : "Inactive";
    public string IsDefaultDisplay => IsDefault ? "Yes" : "No";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
    public string OptionsCountDisplay => $"{OptionsCount} option(s)";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// DTO for creating a new service charge type
/// </summary>
public class CreateServiceChargeTypeDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ChargeOptionScope { get; set; }
    
    public bool IsDefault { get; set; } = false;
    
    public bool Status { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing service charge type
/// </summary>
public class UpdateServiceChargeTypeDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ChargeOptionScope { get; set; }
    
    public bool IsDefault { get; set; } = false;
    
    public bool Status { get; set; } = true;
}
