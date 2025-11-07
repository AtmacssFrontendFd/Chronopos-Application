using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Stock Transfer Report
/// </summary>
public class StockTransferReportDto
{
    public int TransferId { get; set; }
    
    [Display(Name = "Transfer No")]
    public string TransferNo { get; set; } = string.Empty;
    
    [Display(Name = "Date")]
    public DateTime Date { get; set; }
    
    [Display(Name = "From Location")]
    public string FromLocation { get; set; } = string.Empty;
    
    [Display(Name = "To Location")]
    public string ToLocation { get; set; } = string.Empty;
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product")]
    public string Product { get; set; } = string.Empty;
    
    [Display(Name = "Qty")]
    public decimal Qty { get; set; }
    
    [Display(Name = "Unit")]
    public string Unit { get; set; } = string.Empty;
    
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;
    
    [Display(Name = "Sent By")]
    public string? SentBy { get; set; }
    
    [Display(Name = "Received By")]
    public string? ReceivedBy { get; set; }
    
    [Display(Name = "Note")]
    public string? Note { get; set; }
    
    // Display properties
    public string DateDisplay => Date.ToString("dd/MM/yyyy");
    public string QtyDisplay => Qty.ToString("N2");
    public string SentByDisplay => SentBy ?? "-";
    public string ReceivedByDisplay => ReceivedBy ?? "-";
    public string NoteDisplay => Note ?? "-";
    
    // Status color
    public string StatusColor => Status.ToLower() switch
    {
        "completed" => "Green",
        "in_transit" => "Blue",
        "sent" => "Orange",
        "pending" => "Gray",
        "cancelled" => "Red",
        _ => "Gray"
    };
}
