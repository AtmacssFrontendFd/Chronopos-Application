using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Stock Summary Report - Shows stock movement summary by product
/// </summary>
public class StockSummaryDto
{
    public int ProductId { get; set; }
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;
    
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;
    
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;
    
    [Display(Name = "Opening Stock")]
    public decimal OpeningStock { get; set; }
    
    [Display(Name = "Inward Qty")]
    public decimal InwardQty { get; set; }
    
    [Display(Name = "Outward Qty")]
    public decimal OutwardQty { get; set; }
    
    [Display(Name = "Closing Stock")]
    public decimal ClosingStock { get; set; }
    
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; }
    
    [Display(Name = "Stock Value")]
    public decimal StockValue { get; set; }
    
    // Display properties
    public string OpeningStockDisplay => OpeningStock.ToString("N2");
    public string InwardQtyDisplay => InwardQty.ToString("N2");
    public string OutwardQtyDisplay => OutwardQty.ToString("N2");
    public string ClosingStockDisplay => ClosingStock.ToString("N2");
    public string UnitCostDisplay => UnitCost.ToString("C2");
    public string StockValueDisplay => StockValue.ToString("C2");
    
    // Status color
    public string StockColor => ClosingStock <= 0 ? "Red" : ClosingStock < 10 ? "Orange" : "Green";
}
