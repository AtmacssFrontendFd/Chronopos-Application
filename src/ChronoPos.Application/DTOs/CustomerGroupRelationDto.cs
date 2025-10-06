using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for CustomerGroupRelation operations
/// </summary>
public class CustomerGroupRelationDto
{
    public int Id { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? CustomerGroupId { get; set; }
    
    public string? Status { get; set; }
    
    public int? CreatedBy { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties for display
    public string? CustomerName { get; set; }
    public string? CustomerGroupName { get; set; }
    
    // Display properties for UI binding
    public string StatusDisplay => Status ?? "Active";
    public string CreatedAtFormatted => CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
    public string UpdatedAtFormatted => UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
    public bool IsActive => Status?.ToLower() == "active";
}

/// <summary>
/// DTO for creating a new customer group relation
/// </summary>
public class CreateCustomerGroupRelationDto
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int CustomerGroupId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing customer group relation
/// </summary>
public class UpdateCustomerGroupRelationDto
{
    public int? CustomerId { get; set; }
    
    public int? CustomerGroupId { get; set; }
    
    public string? Status { get; set; }
    
    public int? UpdatedBy { get; set; }
}
