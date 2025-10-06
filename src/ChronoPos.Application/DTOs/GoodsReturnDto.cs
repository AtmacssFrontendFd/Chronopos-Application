using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for goods return display and transfer
/// </summary>
public class GoodsReturnDto
{
    public int Id { get; set; }
    public string ReturnNo { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int? ReferenceGrnId { get; set; }
    public string? ReferenceGrnNo { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public bool IsTotallyReplaced { get; set; } = false; // True when all items are fully replaced
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public List<GoodsReturnItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for goods return item display
/// </summary>
public class GoodsReturnItemDto
{
    public int Id { get; set; }
    public int ReturnId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int? BatchId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Reason { get; set; }
    public decimal AlreadyReplacedQuantity { get; set; } = 0; // Tracks how much has been replaced
    public bool IsTotallyReplaced { get; set; } = false; // True when AlreadyReplacedQuantity >= Quantity
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for updating a GoodsReturnItem
/// </summary>
public class UpdateGoodsReturnItemDto
{
    public int? BatchId { get; set; }
    
    [StringLength(50)]
    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal? Quantity { get; set; }
    
    public long? UomId { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Cost price must be non-negative")]
    public decimal? CostPrice { get; set; }
    
    [StringLength(255)]
    public string? Reason { get; set; }
}