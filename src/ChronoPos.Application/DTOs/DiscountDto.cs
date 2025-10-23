using System.ComponentModel.DataAnnotations;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Discount operations
/// </summary>
public class DiscountDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(150)]
    public string DiscountName { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string? DiscountNameAr { get; set; }
    
    [StringLength(150)]
    public string? DiscountDescription { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DiscountCode { get; set; } = string.Empty;
    
    public DiscountType DiscountType { get; set; }
    public string DiscountTypeDisplay => DiscountType.ToString();
    
    [Required]
    public decimal DiscountValue { get; set; }
    
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinPurchaseAmount { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public DiscountApplicableOn ApplicableOn { get; set; }
    public string ApplicableOnDisplay => ApplicableOn.ToString();
    
    // Selected product and category IDs (populated from many-to-many relationships)
    public List<int> SelectedProductIds { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
    public List<string> SelectedProductNames { get; set; } = new(); // For display
    public List<string> SelectedCategoryNames { get; set; } = new(); // For display
    
    public int Priority { get; set; } = 0;
    public bool IsStackable { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public string? DeletedByName { get; set; }
    
    public int? StoreId { get; set; }
    public string? StoreName { get; set; }
    
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    
    // Computed Properties for UI
    public string FormattedDiscountValue =>
        DiscountType == DiscountType.Percentage 
            ? $"{DiscountValue}%" 
            : $"{CurrencyCode} {DiscountValue:F2}";
    
    public bool IsCurrentlyActive =>
        IsActive && 
        DateTime.UtcNow >= StartDate && 
        DateTime.UtcNow <= EndDate &&
        DeletedAt == null;
    
    public string StatusDisplay =>
        !IsActive ? "Inactive" :
        DeletedAt != null ? "Deleted" :
        DateTime.UtcNow < StartDate ? "Scheduled" :
        DateTime.UtcNow > EndDate ? "Expired" :
        "Active";
        
    public string ValidityPeriod =>
        $"{StartDate:MMM dd, yyyy} - {EndDate:MMM dd, yyyy}";
    
    // Display property for UI binding
    public string DisplayName => $"{DiscountName} ({DiscountCode})";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return DisplayName;
    }
}

/// <summary>
/// DTO for creating a new discount
/// </summary>
public class CreateDiscountDto
{
    [Required]
    [StringLength(150)]
    public string DiscountName { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string? DiscountNameAr { get; set; }
    
    [StringLength(150)]
    public string? DiscountDescription { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DiscountCode { get; set; } = string.Empty;
    
    [Required]
    public DiscountType DiscountType { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
    public decimal DiscountValue { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Max discount amount must be 0 or greater")]
    public decimal? MaxDiscountAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Min purchase amount must be 0 or greater")]
    public decimal? MinPurchaseAmount { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public DiscountApplicableOn ApplicableOn { get; set; }
    
    // Product and Category selection arrays for multi-select functionality
    public List<int> SelectedProductIds { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
    
    [Range(0, int.MaxValue, ErrorMessage = "Priority must be 0 or greater")]
    public int Priority { get; set; } = 0;
    
    public bool IsStackable { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public int? StoreId { get; set; }
    
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing discount
/// </summary>
public class UpdateDiscountDto
{
    [Required]
    [StringLength(150)]
    public string DiscountName { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string? DiscountNameAr { get; set; }
    
    [StringLength(150)]
    public string? DiscountDescription { get; set; }
    
    [Required]
    [StringLength(50)]
    public string DiscountCode { get; set; } = string.Empty;
    
    [Required]
    public DiscountType DiscountType { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
    public decimal DiscountValue { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Max discount amount must be 0 or greater")]
    public decimal? MaxDiscountAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Min purchase amount must be 0 or greater")]
    public decimal? MinPurchaseAmount { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public DiscountApplicableOn ApplicableOn { get; set; }
    
    // Product and Category selection arrays for multi-select functionality
    public List<int> SelectedProductIds { get; set; } = new();
    public List<int> SelectedCategoryIds { get; set; } = new();
    
    [Range(0, int.MaxValue, ErrorMessage = "Priority must be 0 or greater")]
    public int Priority { get; set; } = 0;
    
    public bool IsStackable { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public int? StoreId { get; set; }
    
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "USD";
    
    public int? UpdatedBy { get; set; }
}

/// <summary>
/// DTO for discount search and filtering
/// </summary>
public class DiscountSearchDto
{
    public string? SearchTerm { get; set; }
    public DiscountType? DiscountType { get; set; }
    public DiscountApplicableOn? ApplicableOn { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsCurrentlyActive { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public int? StoreId { get; set; }
    public int? CreatedBy { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

/// <summary>
/// DTO for discount validation result
/// </summary>
public class DiscountValidationDto
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }
    
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}