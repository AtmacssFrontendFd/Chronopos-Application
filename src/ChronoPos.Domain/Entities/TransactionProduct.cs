using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a line item in a sales transaction
/// </summary>
public class TransactionProduct
{
    public int Id { get; set; }
    
    [Required]
    public int TransactionId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public decimal BuyerCost { get; set; } = 0;
    
    [Required]
    public decimal SellingPrice { get; set; }
    
    public int? ProductUnitId { get; set; }
    
    public bool IsPercentageDiscount { get; set; } = false;
    
    public decimal DiscountValue { get; set; } = 0;
    
    public decimal DiscountMaxValue { get; set; } = 0;
    
    public decimal Vat { get; set; } = 0;
    
    [Required]
    public decimal Quantity { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "active"; // active, returned, exchanged
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual Transaction Transaction { get; set; } = null!;
    
    public virtual Product Product { get; set; } = null!;
    
    public virtual ProductUnit? ProductUnit { get; set; }
    
    public virtual User? Creator { get; set; }
    
    public virtual User? Updater { get; set; }
    
    public virtual User? Deleter { get; set; }
    
    public virtual ICollection<TransactionModifier> TransactionModifiers { get; set; } = new List<TransactionModifier>();
    
    public virtual ICollection<RefundTransactionProduct> RefundTransactionProducts { get; set; } = new List<RefundTransactionProduct>();
    
    public virtual ICollection<ExchangeTransactionProduct> OriginalExchangeTransactionProducts { get; set; } = new List<ExchangeTransactionProduct>();
    
    /// <summary>
    /// Calculates the line total before any discounts
    /// </summary>
    public decimal LineSubtotal => SellingPrice * Quantity;
    
    /// <summary>
    /// Calculates the discount amount
    /// </summary>
    public decimal DiscountAmount
    {
        get
        {
            if (IsPercentageDiscount)
            {
                var discountAmt = LineSubtotal * (DiscountValue / 100);
                return DiscountMaxValue > 0 ? Math.Min(discountAmt, DiscountMaxValue) : discountAmt;
            }
            return DiscountValue * Quantity;
        }
    }
    
    /// <summary>
    /// Calculates the line total after discount
    /// </summary>
    public decimal LineTotalAfterDiscount => LineSubtotal - DiscountAmount;
    
    /// <summary>
    /// Calculates the VAT amount
    /// </summary>
    public decimal VatAmount => LineTotalAfterDiscount * (Vat / 100);
    
    /// <summary>
    /// Calculates the final line total including VAT
    /// </summary>
    public decimal LineTotal => LineTotalAfterDiscount + VatAmount;
    
    /// <summary>
    /// Returns formatted line total
    /// </summary>
    public string LineTotalDisplay => $"{LineTotal:N2}";
}
