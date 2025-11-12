using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a service charge that can be applied to transactions (e.g., restaurant 10% service)
/// </summary>
public class ServiceCharge
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? NameArabic { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    public bool IsPercentage { get; set; } = true;
    
    [Required]
    public decimal Value { get; set; } = 0;
    
    public int? TaxTypeId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool AutoApply { get; set; } = false; // Automatically apply to transactions
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual TaxType? TaxType { get; set; }
    
    public virtual ICollection<TransactionServiceCharge> TransactionServiceCharges { get; set; } = new List<TransactionServiceCharge>();
    
    /// <summary>
    /// Returns formatted service charge value
    /// </summary>
    public string ValueDisplay => IsPercentage ? $"{Value}%" : $"{Value:N2}";
    
    /// <summary>
    /// Calculates service charge amount for a given subtotal
    /// </summary>
    public decimal CalculateAmount(decimal subtotal)
    {
        if (IsPercentage)
        {
            return subtotal * (Value / 100);
        }
        return Value;
    }
}
