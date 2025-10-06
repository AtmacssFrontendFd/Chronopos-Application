using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents the relationship between customers and customer groups (matches customers_group_relation table)
/// </summary>
public class CustomerGroupRelation
{
    public int Id { get; set; }
    
    public int? CustomerId { get; set; }
    
    public int? CustomerGroupId { get; set; }
    
    [StringLength(20)]
    public string? Status { get; set; }
    
    public int? CreatedBy { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties
    public virtual Customer? Customer { get; set; }
    public virtual CustomerGroup? CustomerGroup { get; set; }
}
