using System.ComponentModel.DataAnnotations;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a sale transaction in the Point of Sale system
/// </summary>
public class Sale
{
    public int Id { get; set; }
    
    [Required]
    public string TransactionNumber { get; set; } = string.Empty;
    
    public int? CustomerId { get; set; }
    
    public Customer? Customer { get; set; }
    
    [Required]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public decimal SubTotal { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    [Required]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    public SaleStatus Status { get; set; } = SaleStatus.Settled;
    
    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
}
