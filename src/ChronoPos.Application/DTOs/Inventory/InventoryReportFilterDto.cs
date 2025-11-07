using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// Enum for inventory report types
/// </summary>
public enum InventoryReportType
{
    StockSummary,
    StockLedger,
    StockValuation,
    LowStock,
    GoodsReceived,
    StockAdjustment,
    StockTransfer,
    StockAging,
    GoodsReturn,
    CategorySummary
}

/// <summary>
/// Common filter DTO for inventory reports
/// </summary>
public class InventoryReportFilterDto
{
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);
    
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; } = DateTime.Today;
    
    [Display(Name = "Product")]
    public int? ProductId { get; set; }
    
    [Display(Name = "Category")]
    public int? CategoryId { get; set; }
    
    [Display(Name = "Location")]
    public string? Location { get; set; }
    
    [Display(Name = "Supplier")]
    public int? SupplierId { get; set; }
    
    [Display(Name = "Report Type")]
    public string ReportType { get; set; } = "StockSummary";
    
    [Display(Name = "Status")]
    public string? Status { get; set; }
    
    [Display(Name = "Show Only Low Stock")]
    public bool ShowOnlyLowStock { get; set; }
    
    [Display(Name = "Show Only Out of Stock")]
    public bool ShowOnlyOutOfStock { get; set; }
    
    [Display(Name = "Include Zero Stock")]
    public bool IncludeZeroStock { get; set; } = true;
}

/// <summary>
/// Available report types for inventory reports
/// </summary>
public static class InventoryReportTypes
{
    public const string StockSummary = "Stock Summary";
    public const string StockLedger = "Stock Ledger";
    public const string StockValuation = "Stock Valuation";
    public const string LowStock = "Low Stock / Reorder";
    public const string GoodsReceived = "Goods Received (GRN)";
    public const string StockAdjustment = "Stock Adjustment";
    public const string StockTransfer = "Stock Transfer";
    public const string StockAging = "Stock Aging";
    public const string GoodsReturn = "Goods Return/Replace";
    public const string InventorySummary = "Category Summary";
    
    public static List<string> GetAll() => new()
    {
        StockSummary,
        StockLedger,
        StockValuation,
        LowStock,
        GoodsReceived,
        StockAdjustment,
        StockTransfer,
        StockAging,
        GoodsReturn,
        InventorySummary
    };
}
