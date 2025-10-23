using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for goods replace item display
/// </summary>
public class GoodsReplaceItemDto
{
    public int Id { get; set; }
    public int ReplaceId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount => Quantity * Rate;
    public int? ReferenceReturnItemId { get; set; }
    public string? RemarksLine { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Display properties for UI binding
    public string BatchDisplay => BatchNo ?? "-";
    public string ExpiryDisplay => ExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
    public string RemarksDisplay => RemarksLine ?? "-";
    public string AmountDisplay => Amount.ToString("N2");
}

/// <summary>
/// DTO for updating an existing goods replace item
/// </summary>
public class UpdateGoodsReplaceItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [StringLength(50)]
    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0")]
    public decimal Rate { get; set; }
    
    public int? ReferenceReturnItemId { get; set; }
    
    public string? RemarksLine { get; set; }
}

/// <summary>
/// DTO for detailed goods replace item information
/// </summary>
public class GoodsReplaceItemDetailDto
{
    public int Id { get; set; }
    public int ReplaceId { get; set; }
    public string ReplaceNo { get; set; } = string.Empty;
    public DateTime ReplaceDate { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount => Quantity * Rate;
    public int? ReferenceReturnItemId { get; set; }
    public string? RemarksLine { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string ReplaceStatus { get; set; } = string.Empty;
    
    // Display properties
    public string BatchDisplay => BatchNo ?? "-";
    public string ExpiryDisplay => ExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
    public string RemarksDisplay => RemarksLine ?? "-";
    public string AmountDisplay => Amount.ToString("N2");
}

/// <summary>
/// DTO for goods replace item summary statistics
/// </summary>
public class GoodsReplaceItemSummaryDto
{
    public int TotalItems { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageRate { get; set; }
}
