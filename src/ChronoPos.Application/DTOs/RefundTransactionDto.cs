using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Refund Transaction operations
/// </summary>
public class RefundTransactionDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public int SellingTransactionId { get; set; }
    public int? ShiftId { get; set; }
    public int? UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public bool IsCash { get; set; } = true;
    public DateTime RefundTime { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public string? CustomerName { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? UserName { get; set; }
    
    // Collection
    public List<RefundTransactionProductDto> RefundProducts { get; set; } = new();
    
    // Display properties
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
    public string RefundTimeDisplay => RefundTime.ToString("dd/MM/yyyy HH:mm");
    public string PaymentMethodDisplay => IsCash ? "Cash" : "Non-Cash";
}

/// <summary>
/// DTO for creating a new refund transaction
/// </summary>
public class CreateRefundTransactionDto
{
    public int? CustomerId { get; set; }
    
    [Required]
    public int SellingTransactionId { get; set; }
    
    public int? ShiftId { get; set; }
    public int? UserId { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public bool IsCash { get; set; } = true;
    
    public DateTime RefundTime { get; set; } = DateTime.UtcNow;
    
    public List<CreateRefundTransactionProductDto> Products { get; set; } = new();
}

/// <summary>
/// DTO for Refund Transaction Product
/// </summary>
public class RefundTransactionProductDto
{
    public int Id { get; set; }
    public int RefundTransactionId { get; set; }
    public int TransactionProductId { get; set; }
    public decimal TotalQuantityReturned { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Active";
    
    public string? ProductName { get; set; }
    
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
    public string QuantityDisplay => $"{TotalQuantityReturned:N2}";
}

/// <summary>
/// DTO for creating refund transaction product
/// </summary>
public class CreateRefundTransactionProductDto
{
    [Required]
    public int TransactionProductId { get; set; }
    
    [Required]
    public decimal TotalQuantityReturned { get; set; }
    
    public decimal TotalVat { get; set; }
    public decimal TotalAmount { get; set; }
}
