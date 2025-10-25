using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Exchange Transaction operations
/// </summary>
public class ExchangeTransactionDto
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public int SellingTransactionId { get; set; }
    public int? ShiftId { get; set; }
    public decimal TotalExchangedAmount { get; set; }
    public decimal TotalExchangedVat { get; set; }
    public decimal ProductExchangedQuantity { get; set; }
    public DateTime ExchangeTime { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public string? CustomerName { get; set; }
    public string? InvoiceNumber { get; set; }
    
    // Collection
    public List<ExchangeTransactionProductDto> ExchangeProducts { get; set; } = new();
    
    // Display properties
    public string TotalExchangedAmountDisplay => $"{TotalExchangedAmount:N2}";
    public string ExchangeTimeDisplay => ExchangeTime.ToString("dd/MM/yyyy HH:mm");
    public string ProductExchangedQuantityDisplay => $"{ProductExchangedQuantity:N2}";
}

/// <summary>
/// DTO for creating a new exchange transaction
/// </summary>
public class CreateExchangeTransactionDto
{
    public int? CustomerId { get; set; }
    
    [Required]
    public int SellingTransactionId { get; set; }
    
    public int? ShiftId { get; set; }
    
    public decimal TotalExchangedAmount { get; set; }
    public decimal TotalExchangedVat { get; set; }
    public decimal ProductExchangedQuantity { get; set; }
    
    public DateTime ExchangeTime { get; set; } = DateTime.UtcNow;
    
    public List<CreateExchangeTransactionProductDto> Products { get; set; } = new();
}

/// <summary>
/// DTO for Exchange Transaction Product
/// </summary>
public class ExchangeTransactionProductDto
{
    public int Id { get; set; }
    public int ExchangeTransactionId { get; set; }
    public int? OriginalTransactionProductId { get; set; }
    public int? NewProductId { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public decimal PriceDifference { get; set; }
    public decimal OldProductAmount { get; set; }
    public decimal NewProductAmount { get; set; }
    public decimal VatDifference { get; set; }
    public string Status { get; set; } = "Active";
    
    public string? OldProductName { get; set; }
    public string? NewProductName { get; set; }
    
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
    
    public string ReturnedQuantityDisplay => $"{ReturnedQuantity:N2}";
    public string NewQuantityDisplay => $"{NewQuantity:N2}";
}

/// <summary>
/// DTO for creating exchange transaction product
/// </summary>
public class CreateExchangeTransactionProductDto
{
    public int? OriginalTransactionProductId { get; set; }
    
    [Required]
    public int NewProductId { get; set; }
    
    [Required]
    public decimal ReturnedQuantity { get; set; }
    
    [Required]
    public decimal NewQuantity { get; set; }
    
    public decimal PriceDifference { get; set; }
    public decimal OldProductAmount { get; set; }
    public decimal NewProductAmount { get; set; }
    public decimal VatDifference { get; set; }
}
