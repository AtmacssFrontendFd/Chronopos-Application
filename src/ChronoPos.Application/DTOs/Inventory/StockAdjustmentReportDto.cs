using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Stock Adjustment Report
/// </summary>
public class StockAdjustmentReportDto
{
    public int AdjustmentId { get; set; }
    
    [Display(Name = "Adjustment No")]
    public string AdjustmentNo { get; set; } = string.Empty;
    
    [Display(Name = "Date")]
    public DateTime Date { get; set; }
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product")]
    public string Product { get; set; } = string.Empty;
    
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;
    
    [Display(Name = "Previous Qty")]
    public decimal PreviousQty { get; set; }
    
    [Display(Name = "New Qty")]
    public decimal NewQty { get; set; }
    
    [Display(Name = "Difference")]
    public decimal Difference { get; set; }
    
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;
    
    [Display(Name = "User")]
    public string User { get; set; } = string.Empty;
    
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;
    
    [Display(Name = "Note")]
    public string? Note { get; set; }
    
    // Display properties
    public string DateDisplay => Date.ToString("dd/MM/yyyy HH:mm");
    public string PreviousQtyDisplay => PreviousQty.ToString("N2");
    public string NewQtyDisplay => NewQty.ToString("N2");
    public string DifferenceDisplay => Difference > 0 ? $"+{Difference:N2}" : Difference.ToString("N2");
    public string NoteDisplay => Note ?? "-";
    
    // Color based on adjustment type
    public string DifferenceColor => Difference > 0 ? "Green" : Difference < 0 ? "Red" : "Gray";
    public string StatusColor => Status.ToLower() switch
    {
        "approved" => "Green",
        "pending" => "Orange",
        "rejected" => "Red",
        _ => "Gray"
    };
}
