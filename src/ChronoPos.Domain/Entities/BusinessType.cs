using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a business type for business customers
/// </summary>
public class BusinessType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string BusinessTypeName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string BusinessTypeNameAr { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}