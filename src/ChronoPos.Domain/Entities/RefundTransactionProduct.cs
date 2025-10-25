using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents products refunded in a transaction
/// </summary>
public class RefundTransactionProduct
{
    public int Id { get; set; }
    
    [Required]
    public int RefundTransactionId { get; set; }
    
    [Required]
    public int TransactionProductId { get; set; }
    
    public decimal TotalQuantityReturned { get; set; } = 0;
    
    public decimal TotalVat { get; set; } = 0;
    
    public decimal TotalAmount { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual RefundTransaction RefundTransaction { get; set; } = null!;
    
    public virtual TransactionProduct TransactionProduct { get; set; } = null!;
    
    public virtual User? Creator { get; set; }
    
    /// <summary>
    /// Returns formatted total amount
    /// </summary>
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
    
    /// <summary>
    /// Returns formatted quantity
    /// </summary>
    public string QuantityDisplay => $"{TotalQuantityReturned:N2}";
}
