using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Stock Valuation Report - Shows current stock value
/// </summary>
public class StockValuationDto
{
    public int ProductId { get; set; }
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product")]
    public string Product { get; set; } = string.Empty;
    
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;
    
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;
    
    [Display(Name = "Quantity")]
    public decimal Quantity { get; set; }
    
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; }
    
    [Display(Name = "Total Value")]
    public decimal TotalValue { get; set; }
    
    [Display(Name = "% of Total")]
    public decimal PercentageOfTotal { get; set; }
    
    // Display properties
    public string QuantityDisplay => Quantity.ToString("N2");
    public string UnitCostDisplay => UnitCost.ToString("C2");
    public string TotalValueDisplay => TotalValue.ToString("C2");
    public string PercentageDisplay => $"{PercentageOfTotal:N2}%";
}
