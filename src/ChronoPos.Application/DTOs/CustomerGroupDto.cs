using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Customer Group operations
/// </summary>
public class CustomerGroupDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameAr { get; set; }
    
    public long? SellingPriceTypeId { get; set; }
    
    public string? SellingPriceTypeName { get; set; }
    
    public int? DiscountId { get; set; }
    
    public string? DiscountName { get; set; }
    
    public decimal? DiscountValue { get; set; }
    
    public decimal? DiscountMaxValue { get; set; }
    
    public bool IsPercentage { get; set; } = false;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    // Audit fields
    public int? CreatedBy { get; set; }
    
    public string? CreatedByName { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public string? UpdatedByName { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public string? DeletedByName { get; set; }
    
    // Additional display properties
    public int CustomerCount { get; set; } = 0;
    
    public string DisplayName => !string.IsNullOrEmpty(NameAr) ? $"{Name} ({NameAr})" : Name;
    
    public string DiscountDisplay => IsPercentage 
        ? $"{DiscountValue}%" 
        : $"${DiscountValue:F2}";
}

/// <summary>
/// DTO for creating a new Customer Group
/// </summary>
public class CreateCustomerGroupDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameAr { get; set; }
    
    public long? SellingPriceTypeId { get; set; }
    
    public int? DiscountId { get; set; }
    
    public decimal? DiscountValue { get; set; }
    
    public decimal? DiscountMaxValue { get; set; }
    
    public bool IsPercentage { get; set; } = false;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for updating an existing Customer Group
/// </summary>
public class UpdateCustomerGroupDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameAr { get; set; }
    
    public long? SellingPriceTypeId { get; set; }
    
    public int? DiscountId { get; set; }
    
    public decimal? DiscountValue { get; set; }
    
    public decimal? DiscountMaxValue { get; set; }
    
    public bool IsPercentage { get; set; } = false;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
}