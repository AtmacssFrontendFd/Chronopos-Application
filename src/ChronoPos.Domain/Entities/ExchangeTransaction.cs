using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an exchange transaction header
/// </summary>
public class ExchangeTransaction
{
    public int Id { get; set; }
    
    public int? CustomerId { get; set; }
    
    [Required]
    public int SellingTransactionId { get; set; }
    
    public int? ShiftId { get; set; }
    
    public decimal TotalExchangedAmount { get; set; } = 0;
    
    public decimal TotalExchangedVat { get; set; } = 0;
    
    public decimal ProductExchangedQuantity { get; set; } = 0;
    
    public DateTime ExchangeTime { get; set; } = DateTime.UtcNow;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Customer? Customer { get; set; }
    
    public virtual Transaction SellingTransaction { get; set; } = null!;
    
    public virtual Shift? Shift { get; set; }
    
    public virtual User? Creator { get; set; }
    
    public virtual ICollection<ExchangeTransactionProduct> ExchangeTransactionProducts { get; set; } = new List<ExchangeTransactionProduct>();
    
    /// <summary>
    /// Returns formatted total amount
    /// </summary>
    public string TotalExchangedAmountDisplay => $"{TotalExchangedAmount:N2}";
    
    /// <summary>
    /// Returns formatted exchange time
    /// </summary>
    public string ExchangeTimeDisplay => ExchangeTime.ToString("dd/MM/yyyy HH:mm");
    
    /// <summary>
    /// Returns formatted quantity
    /// </summary>
    public string ProductExchangedQuantityDisplay => $"{ProductExchangedQuantity:N2}";
}
