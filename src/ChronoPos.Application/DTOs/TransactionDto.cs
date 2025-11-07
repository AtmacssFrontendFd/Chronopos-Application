using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Transaction operations
/// </summary>
public class TransactionDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int? CustomerId { get; set; }
    public int UserId { get; set; }
    public int? ShopLocationId { get; set; }
    public int? TableId { get; set; }
    public int? ReservationId { get; set; }
    public DateTime SellingTime { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalAppliedVat { get; set; }
    public decimal TotalAppliedDiscountValue { get; set; }
    public decimal AmountPaidCash { get; set; }
    public decimal AmountCreditRemaining { get; set; }
    public int CreditDays { get; set; }
    public bool IsPercentageDiscount { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountMaxValue { get; set; }
    public decimal Vat { get; set; }
    public string? DiscountNote { get; set; }
    public string? InvoiceNumber { get; set; }
    public string Status { get; set; } = "draft";
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public string? CustomerName { get; set; }
    public string? UserName { get; set; }
    public string? TableNumber { get; set; }
    
    // Collection properties
    public List<TransactionProductDto> TransactionProducts { get; set; } = new();
    public List<TransactionServiceChargeDto> TransactionServiceCharges { get; set; } = new();
    
    // Display properties
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
    public string SellingTimeDisplay => SellingTime.ToString("dd/MM/yyyy HH:mm");
    public string StatusDisplay => Status switch
    {
        "draft" => "Draft",
        "billed" => "Billed",
        "settled" => "Settled",
        "hold" => "Hold",
        "cancelled" => "Cancelled",
        "pending_payment" => "Pending Payment",
        "partial_payment" => "Partial Payment",
        _ => Status
    };
}

/// <summary>
/// DTO for creating a new transaction
/// </summary>
public class CreateTransactionDto
{
    [Required]
    public int ShiftId { get; set; }
    
    public int? CustomerId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public int? ShopLocationId { get; set; }
    public int? TableId { get; set; }
    public int? ReservationId { get; set; }
    
    public DateTime SellingTime { get; set; } = DateTime.UtcNow;
    
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal AmountPaidCash { get; set; }
    public decimal AmountCreditRemaining { get; set; }
    public int CreditDays { get; set; }
    
    public bool IsPercentageDiscount { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountMaxValue { get; set; }
    public decimal Vat { get; set; }
    public string? DiscountNote { get; set; }
    
    public string Status { get; set; } = "draft";
    
    public List<CreateTransactionProductDto> Products { get; set; } = new();
    public List<int> ServiceChargeOptionIds { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing transaction
/// </summary>
public class UpdateTransactionDto
{
    public int? CustomerId { get; set; }
    public int? TableId { get; set; }
    public int? ReservationId { get; set; }
    
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal AmountPaidCash { get; set; }
    public decimal AmountCreditRemaining { get; set; }
    public int CreditDays { get; set; }
    
    public bool IsPercentageDiscount { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountMaxValue { get; set; }
    public decimal Vat { get; set; }
    public string? DiscountNote { get; set; }
    
    public string Status { get; set; } = "draft";
}

/// <summary>
/// DTO for Transaction Product
/// </summary>
public class TransactionProductDto
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public decimal BuyerCost { get; set; }
    public decimal SellingPrice { get; set; }
    public int? ProductUnitId { get; set; }
    public bool IsPercentageDiscount { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountMaxValue { get; set; }
    public decimal Vat { get; set; }
    public decimal Quantity { get; set; }
    public string Status { get; set; } = "active";
    
    // Navigation properties
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    
    // Calculated properties
    public decimal LineSubtotal { get; set; }
    public decimal LineTotal { get; set; }
    
    // Collection
    public List<TransactionModifierDto> Modifiers { get; set; } = new();
    
    // Display
    public string LineTotalDisplay => $"{LineTotal:N2}";
}

/// <summary>
/// DTO for creating a new transaction product
/// </summary>
public class CreateTransactionProductDto
{
    [Required]
    public int ProductId { get; set; }
    
    public decimal BuyerCost { get; set; }
    
    [Required]
    public decimal SellingPrice { get; set; }
    
    public int? ProductUnitId { get; set; }
    
    public bool IsPercentageDiscount { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal DiscountMaxValue { get; set; }
    public decimal Vat { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    
    public List<int> ModifierIds { get; set; } = new();
}

/// <summary>
/// DTO for Transaction Modifier
/// </summary>
public class TransactionModifierDto
{
    public int Id { get; set; }
    public int TransactionProductId { get; set; }
    public int ProductModifierId { get; set; }
    public decimal ExtraPrice { get; set; }
    
    public string? ModifierName { get; set; }
    public string ExtraPriceDisplay => ExtraPrice > 0 ? $"+{ExtraPrice:N2}" : "No extra charge";
}

/// <summary>
/// DTO for Transaction Service Charge
/// </summary>
public class TransactionServiceChargeDto
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int? ServiceChargeOptionId { get; set; } // Nullable to support manual/custom service charges
    public decimal TotalAmount { get; set; }
    public decimal TotalVat { get; set; }
    public string Status { get; set; } = "Active";
    
    public string? ServiceChargeName { get; set; }
    public string TotalAmountDisplay => $"{TotalAmount:N2}";
}
