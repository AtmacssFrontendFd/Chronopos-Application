using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Service Charge operations
/// </summary>
public class ServiceChargeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameArabic { get; set; }
    public string? Description { get; set; }
    public bool IsPercentage { get; set; } = true;
    public decimal Value { get; set; }
    public int? TaxTypeId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoApply { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public string? TaxTypeName { get; set; }
    
    // Display properties
    public string DisplayName => Name;
    public string NameArabicDisplay => NameArabic ?? "-";
    public string DescriptionDisplay => Description ?? "-";
    public string ValueDisplay => IsPercentage ? $"{Value}%" : $"{Value:N2}";
    public string StatusDisplay => IsActive ? "Active" : "Inactive";
    public string AutoApplyDisplay => AutoApply ? "Yes" : "No";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return $"{Name} ({ValueDisplay})";
    }
}

/// <summary>
/// DTO for creating a new service charge
/// </summary>
public class CreateServiceChargeDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameArabic { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    public bool IsPercentage { get; set; } = true;
    
    [Required]
    public decimal Value { get; set; }
    
    public int? TaxTypeId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool AutoApply { get; set; } = false;
}

/// <summary>
/// DTO for updating an existing service charge
/// </summary>
public class UpdateServiceChargeDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameArabic { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    public bool IsPercentage { get; set; } = true;
    
    [Required]
    public decimal Value { get; set; }
    
    public int? TaxTypeId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool AutoApply { get; set; } = false;
}
