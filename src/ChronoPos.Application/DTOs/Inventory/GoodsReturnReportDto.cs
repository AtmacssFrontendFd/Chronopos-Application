using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Goods Return/Replacement Report
/// </summary>
public class GoodsReturnReportDto
{
    public int ReturnId { get; set; }
    
    [Display(Name = "Return No")]
    public string ReturnNo { get; set; } = string.Empty;
    
    [Display(Name = "Date")]
    public DateTime Date { get; set; }
    
    [Display(Name = "Type")]
    public string Type { get; set; } = string.Empty; // Return or Replace
    
    [Display(Name = "Supplier")]
    public string Supplier { get; set; } = string.Empty;
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product")]
    public string Product { get; set; } = string.Empty;
    
    [Display(Name = "Qty")]
    public decimal Qty { get; set; }
    
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;
    
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;
    
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
    
    [Display(Name = "Replacement Status")]
    public string ReplacementStatus { get; set; } = string.Empty;
    
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;
    
    [Display(Name = "Note")]
    public string? Note { get; set; }
    
    // Display properties
    public string DateDisplay => Date.ToString("dd/MM/yyyy");
    public string QtyDisplay => Qty.ToString("N2");
    public string AmountDisplay => Amount.ToString("C2");
    public string NoteDisplay => Note ?? "-";
    
    // Color coding
    public string TypeColor => Type.ToLower() switch
    {
        "return" => "Red",
        "replace" => "Orange",
        _ => "Gray"
    };
    
    public string ReplacementStatusColor => ReplacementStatus.ToLower() switch
    {
        "replaced" => "Green",
        "partial" => "Orange",
        "pending" => "Gray",
        "not_applicable" => "LightGray",
        _ => "Gray"
    };
    
    public string StatusColor => Status.ToLower() switch
    {
        "approved" => "Green",
        "pending" => "Orange",
        "rejected" => "Red",
        _ => "Gray"
    };
}
