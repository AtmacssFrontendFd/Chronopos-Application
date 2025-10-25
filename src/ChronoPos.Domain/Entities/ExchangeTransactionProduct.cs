using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents line items for exchange transactions
/// </summary>
public class ExchangeTransactionProduct
{
    public int Id { get; set; }
    
    [Required]
    public int ExchangeTransactionId { get; set; }
    
    public int? OriginalTransactionProductId { get; set; } // Product being returned
    
    public int? NewProductId { get; set; } // Product given in exchange
    
    public decimal ReturnedQuantity { get; set; } = 0; // Qty returned
    
    public decimal NewQuantity { get; set; } = 0; // Qty given in exchange
    
    public decimal PriceDifference { get; set; } = 0; // +ve = pay more, -ve = refund
    
    public decimal OldProductAmount { get; set; } = 0;
    
    public decimal NewProductAmount { get; set; } = 0;
    
    public decimal VatDifference { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ExchangeTransaction ExchangeTransaction { get; set; } = null!;
    
    public virtual TransactionProduct? OriginalTransactionProduct { get; set; }
    
    public virtual Product? NewProduct { get; set; }
    
    public virtual User? Creator { get; set; }
    
    /// <summary>
    /// Returns formatted price difference with sign
    /// </summary>
    public string PriceDifferenceDisplay
    {
        get
        {
            if (PriceDifference > 0)
                return $"+{PriceDifference:N2} (Customer pays)";
            else if (PriceDifference < 0)
                return $"{PriceDifference:N2} (Refund to customer)";
            else
                return "0.00 (Even exchange)";
        }
    }
    
    /// <summary>
    /// Returns formatted returned quantity
    /// </summary>
    public string ReturnedQuantityDisplay => $"{ReturnedQuantity:N2}";
    
    /// <summary>
    /// Returns formatted new quantity
    /// </summary>
    public string NewQuantityDisplay => $"{NewQuantity:N2}";
}
