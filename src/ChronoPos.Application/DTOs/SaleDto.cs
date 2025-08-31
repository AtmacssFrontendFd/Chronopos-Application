using System.ComponentModel.DataAnnotations;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Sale operations
/// </summary>
public class SaleDto
{
    public int Id { get; set; }
    
    public string TransactionNumber { get; set; } = string.Empty;
    
    public int? CustomerId { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    
    public DateTime SaleDate { get; set; }
    
    public decimal SubTotal { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public PaymentMethod PaymentMethod { get; set; }
    
    public SaleStatus Status { get; set; }
    
    public string Notes { get; set; } = string.Empty;
    
    public List<SaleItemDto> SaleItems { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for creating a new sale
/// </summary>
public class CreateSaleDto
{
    public int? CustomerId { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public string Notes { get; set; } = string.Empty;
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one sale item is required")]
    public List<CreateSaleItemDto> SaleItems { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for Sale Item operations
/// </summary>
public class SaleItemDto
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    public string ProductName { get; set; } = string.Empty;
    
    public string ProductSku { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// Data Transfer Object for creating a new sale item
/// </summary>
public class CreateSaleItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    public decimal DiscountAmount { get; set; }
}
