using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents service charges applied to a transaction (e.g., 10% service charge)
/// </summary>
public class TransactionServiceCharge
{
    public int Id { get; set; }
    
    [Required]
    public int TransactionId { get; set; }
    
    // Nullable to support manual/custom service charges not linked to predefined service charge
    public int? ServiceChargeId { get; set; }
    
    public decimal TotalAmount { get; set; } = 0;
    
    public decimal TotalVat { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Transaction Transaction { get; set; } = null!;
    
    public virtual ServiceCharge ServiceCharge { get; set; } = null!;
    
    public virtual User? Creator { get; set; }
    
    /// <summary>
    /// Returns formatted total amount
    /// </summary>
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
}
