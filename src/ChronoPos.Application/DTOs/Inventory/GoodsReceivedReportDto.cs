using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Goods Received Note (GRN) Report
/// </summary>
public class GoodsReceivedReportDto
{
    public int GrnId { get; set; }
    
    [Display(Name = "GRN No")]
    public string GrnNo { get; set; } = string.Empty;
    
    [Display(Name = "Date")]
    public DateTime Date { get; set; }
    
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
    
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }
    
    [Display(Name = "Total")]
    public decimal Total { get; set; }
    
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;
    
    [Display(Name = "Invoice No")]
    public string? InvoiceNo { get; set; }
    
    // Display properties
    public string DateDisplay => Date.ToString("dd/MM/yyyy");
    public string QtyDisplay => Qty.ToString("N2");
    public string UnitPriceDisplay => UnitPrice.ToString("C2");
    public string TotalDisplay => Total.ToString("C2");
    public string InvoiceNoDisplay => InvoiceNo ?? "-";
    
    // Status color
    public string StatusColor => Status.ToLower() switch
    {
        "posted" => "Green",
        "pending" => "Orange",
        "cancelled" => "Red",
        _ => "Gray"
    };
}
