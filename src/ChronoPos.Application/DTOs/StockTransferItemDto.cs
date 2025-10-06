using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for stock transfer item display
/// </summary>
public class StockTransferItemDto
{
    public int Id { get; set; }
    public int TransferId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantitySent { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal DamagedQty { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RemarksLine { get; set; }
    
    // Display properties for UI binding
    public string BatchDisplay => BatchNo ?? "-";
    public string ExpiryDisplay => ExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
    public string StatusDisplay => Status;
    public string RemarksDisplay => RemarksLine ?? "-";
    public decimal PendingQuantity => Math.Max(0, QuantitySent - QuantityReceived - DamagedQty);
    public string QuantityStatusDisplay => $"{QuantityReceived}/{QuantitySent}";
    public bool IsFullyReceived => QuantityReceived + DamagedQty >= QuantitySent;
    public bool HasDamage => DamagedQty > 0;
}

/// <summary>
/// DTO for updating an existing stock transfer item
/// </summary>
public class UpdateStockTransferItemDto
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
    public decimal QuantitySent { get; set; }
    
    public decimal QuantityReceived { get; set; } = 0;
    
    public decimal DamagedQty { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending";
    
    public string? RemarksLine { get; set; }
}

/// <summary>
/// DTO for updating stock transfer item status
/// </summary>
public class StockTransferItemStatusDto
{
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;
    
    public string? RemarksLine { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for updating stock transfer item quantities
/// </summary>
public class StockTransferItemQuantityDto
{
    [Range(0, double.MaxValue, ErrorMessage = "Quantity received must be non-negative")]
    public decimal QuantityReceived { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Damaged quantity must be non-negative")]
    public decimal DamagedQty { get; set; } = 0;
    
    public string? RemarksLine { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO for detailed stock transfer item information
/// </summary>
public class StockTransferItemDetailDto
{
    public int Id { get; set; }
    public int TransferId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantitySent { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal DamagedQty { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RemarksLine { get; set; }
    public string FromStoreName { get; set; } = string.Empty;
    public string ToStoreName { get; set; } = string.Empty;
    public string TransferStatus { get; set; } = string.Empty;
    
    // Display properties
    public string BatchDisplay => BatchNo ?? "-";
    public string ExpiryDisplay => ExpiryDate?.ToString("dd/MM/yyyy") ?? "-";
    public string StatusDisplay => Status;
    public string RemarksDisplay => RemarksLine ?? "-";
    public decimal PendingQuantity => Math.Max(0, QuantitySent - QuantityReceived - DamagedQty);
    public string QuantityStatusDisplay => $"{QuantityReceived}/{QuantitySent}";
    public bool IsFullyReceived => QuantityReceived + DamagedQty >= QuantitySent;
    public bool HasDamage => DamagedQty > 0;
}

/// <summary>
/// DTO for bulk status updates
/// </summary>
public class StockTransferItemBulkStatusDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RemarksLine { get; set; }
}

/// <summary>
/// DTO for receiving stock transfer items
/// </summary>
public class StockTransferItemReceiveDto
{
    public int Id { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Quantity received must be non-negative")]
    public decimal QuantityReceived { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Damaged quantity must be non-negative")]
    public decimal DamagedQty { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Received";
    
    public string? RemarksLine { get; set; }
}

/// <summary>
/// DTO for stock transfer item summary statistics
/// </summary>
public class StockTransferItemSummaryDto
{
    public int TotalItems { get; set; }
    public int PendingItems { get; set; }
    public int ReceivedItems { get; set; }
    public int DamagedItems { get; set; }
    public decimal TotalQuantitySent { get; set; }
    public decimal TotalQuantityReceived { get; set; }
    public decimal TotalDamagedQuantity { get; set; }
    public decimal ReceivePercentage { get; set; }
    public decimal DamagePercentage { get; set; }
}