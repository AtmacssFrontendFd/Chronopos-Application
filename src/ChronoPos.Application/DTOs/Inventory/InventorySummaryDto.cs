using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Inventory Summary by Category Report
/// </summary>
public class InventorySummaryDto
{
    public int CategoryId { get; set; }
    
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;
    
    [Display(Name = "No of Items")]
    public int NoOfItems { get; set; }
    
    [Display(Name = "Total Qty")]
    public decimal TotalQty { get; set; }
    
    [Display(Name = "Total Value")]
    public decimal TotalValue { get; set; }
    
    [Display(Name = "Avg Cost")]
    public decimal AvgCost { get; set; }
    
    [Display(Name = "% of Total Value")]
    public decimal PercentageOfTotal { get; set; }
    
    [Display(Name = "Low Stock Items")]
    public int LowStockItems { get; set; }
    
    [Display(Name = "Out of Stock Items")]
    public int OutOfStockItems { get; set; }
    
    // Display properties
    public string TotalQtyDisplay => TotalQty.ToString("N2");
    public string TotalValueDisplay => TotalValue.ToString("C2");
    public string AvgCostDisplay => AvgCost.ToString("C2");
    public string PercentageDisplay => $"{PercentageOfTotal:N2}%";
    
    // Warning indicators
    public bool HasLowStock => LowStockItems > 0;
    public bool HasOutOfStock => OutOfStockItems > 0;
    public string WarningColor => OutOfStockItems > 0 ? "Red" : LowStockItems > 0 ? "Orange" : "Green";
}
