using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a refund transaction header
/// </summary>
public class RefundTransaction
{
    public int Id { get; set; }
    
    public int? CustomerId { get; set; }
    
    [Required]
    public int SellingTransactionId { get; set; }
    
    public int? ShiftId { get; set; }
    
    public int? UserId { get; set; }
    
    public decimal TotalAmount { get; set; } = 0;
    
    public decimal TotalVat { get; set; } = 0;
    
    public bool IsCash { get; set; } = true;
    
    public DateTime RefundTime { get; set; } = DateTime.UtcNow;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Customer? Customer { get; set; }
    
    public virtual Transaction SellingTransaction { get; set; } = null!;
    
    public virtual Shift? Shift { get; set; }
    
    public virtual User? User { get; set; }
    
    public virtual User? Creator { get; set; }
    
    public virtual ICollection<RefundTransactionProduct> RefundTransactionProducts { get; set; } = new List<RefundTransactionProduct>();
    
    /// <summary>
    /// Returns formatted total amount
    /// </summary>
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
    
    /// <summary>
    /// Returns formatted refund time
    /// </summary>
    public string RefundTimeDisplay => RefundTime.ToString("dd/MM/yyyy HH:mm");
    
    /// <summary>
    /// Returns payment method display
    /// </summary>
    public string PaymentMethodDisplay => IsCash ? "Cash" : "Non-Cash";
}
