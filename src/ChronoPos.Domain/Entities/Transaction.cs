using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a sales transaction in the Point of Sale system
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    
    [Required]
    public int ShiftId { get; set; }
    
    public int? CustomerId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public int? ShopLocationId { get; set; }
    
    // Restaurant/cafe specific
    public int? TableId { get; set; }
    
    public int? ReservationId { get; set; }
    
    [Required]
    public DateTime SellingTime { get; set; } = DateTime.UtcNow;
    
    // Totals
    [Required]
    public decimal TotalAmount { get; set; } = 0;
    
    [Required]
    public decimal TotalVat { get; set; } = 0;
    
    [Required]
    public decimal TotalDiscount { get; set; } = 0;
    
    public decimal TotalAppliedVat { get; set; } = 0;
    
    public decimal TotalAppliedDiscountValue { get; set; } = 0;
    
    // Payment info
    public decimal AmountPaidCash { get; set; } = 0;
    
    public decimal AmountCreditRemaining { get; set; } = 0;
    
    public int CreditDays { get; set; } = 0;
    
    // Discounts
    public bool IsPercentageDiscount { get; set; } = false;
    
    public decimal DiscountValue { get; set; } = 0;
    
    public decimal DiscountMaxValue { get; set; } = 0;
    
    public decimal Vat { get; set; } = 0;
    
    public string? DiscountNote { get; set; }
    
    // Invoice
    [StringLength(50)]
    public string? InvoiceNumber { get; set; }
    
    // Status
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "draft"; // draft, billed, settled, hold, cancelled, pending_payment, partial_payment, refunded, exchanged
    
    public int CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual Shift Shift { get; set; } = null!;
    
    public virtual Customer? Customer { get; set; }
    
    public virtual User User { get; set; } = null!;
    
    public virtual ShopLocation? ShopLocation { get; set; }
    
    public virtual RestaurantTable? Table { get; set; }
    
    public virtual Reservation? Reservation { get; set; }
    
    public virtual User Creator { get; set; } = null!;
    
    public virtual User? Updater { get; set; }
    
    public virtual User? Deleter { get; set; }
    
    public virtual ICollection<TransactionProduct> TransactionProducts { get; set; } = new List<TransactionProduct>();
    
    public virtual ICollection<TransactionServiceCharge> TransactionServiceCharges { get; set; } = new List<TransactionServiceCharge>();
    
    public virtual ICollection<RefundTransaction> RefundTransactions { get; set; } = new List<RefundTransaction>();
    
    public virtual ICollection<ExchangeTransaction> ExchangeTransactions { get; set; } = new List<ExchangeTransaction>();
    
    /// <summary>
    /// Returns formatted total amount
    /// </summary>
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
    
    /// <summary>
    /// Returns formatted transaction date and time
    /// </summary>
    public string SellingTimeDisplay => SellingTime.ToString("dd/MM/yyyy HH:mm");
    
    /// <summary>
    /// Returns status display
    /// </summary>
    public string StatusDisplay => Status switch
    {
        "draft" => "Draft",
        "billed" => "Billed",
        "settled" => "Settled",
        "hold" => "Hold",
        "cancelled" => "Cancelled",
        "pending_payment" => "Pending Payment",
        "partial_payment" => "Partial Payment",
        "refunded" => "Refunded",
        "exchanged" => "Exchanged",
        _ => Status
    };
    
    /// <summary>
    /// Generates invoice number based on date and transaction ID
    /// Format: DDMMYY + transactionID
    /// </summary>
    public void GenerateInvoiceNumber()
    {
        if (string.IsNullOrEmpty(InvoiceNumber))
        {
            InvoiceNumber = $"{SellingTime:ddMMyy}{Id}";
        }
    }
}
