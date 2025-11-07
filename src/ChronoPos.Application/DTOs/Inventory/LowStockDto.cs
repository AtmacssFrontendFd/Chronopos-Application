using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Low Stock/Reorder Report - Shows products that need reordering
/// </summary>
public class LowStockDto
{
    public int ProductId { get; set; }
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product")]
    public string Product { get; set; } = string.Empty;
    
    [Display(Name = "Category")]
    public string Category { get; set; } = string.Empty;
    
    [Display(Name = "Current Qty")]
    public decimal CurrentQty { get; set; }
    
    [Display(Name = "Reorder Level")]
    public decimal ReorderLevel { get; set; }
    
    [Display(Name = "Shortage")]
    public decimal Shortage { get; set; }
    
    [Display(Name = "Supplier")]
    public string? Supplier { get; set; }
    
    [Display(Name = "Last Purchase Date")]
    public DateTime? LastPurchaseDate { get; set; }
    
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; }
    
    [Display(Name = "Est. Reorder Value")]
    public decimal EstimatedReorderValue { get; set; }
    
    // Display properties
    public string CurrentQtyDisplay => CurrentQty.ToString("N2");
    public string ReorderLevelDisplay => ReorderLevel.ToString("N2");
    public string ShortageDisplay => Shortage.ToString("N2");
    public string SupplierDisplay => Supplier ?? "-";
    public string LastPurchaseDateDisplay => LastPurchaseDate?.ToString("dd/MM/yyyy") ?? "Never";
    public string UnitCostDisplay => UnitCost.ToString("C2");
    public string EstimatedReorderValueDisplay => EstimatedReorderValue.ToString("C2");
    
    // Status color - Red for critical, Orange for low
    public string StatusColor => CurrentQty <= 0 ? "DarkRed" : CurrentQty < (ReorderLevel / 2) ? "Red" : "Orange";
    public string StatusText => CurrentQty <= 0 ? "Out of Stock" : "Low Stock";
}
