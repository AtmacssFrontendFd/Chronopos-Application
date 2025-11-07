using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Stock Aging Report - Shows how long stock has been in inventory
/// </summary>
public class StockAgingDto
{
    public int ProductId { get; set; }
    
    public int? BatchId { get; set; }
    
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;
    
    [Display(Name = "Product")]
    public string Product { get; set; } = string.Empty;
    
    [Display(Name = "Batch No")]
    public string? BatchNo { get; set; }
    
    [Display(Name = "Quantity")]
    public decimal Quantity { get; set; }
    
    [Display(Name = "Days in Stock")]
    public int DaysInStock { get; set; }
    
    [Display(Name = "Purchase Date")]
    public DateTime PurchaseDate { get; set; }
    
    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }
    
    [Display(Name = "Days to Expiry")]
    public int? DaysToExpiry { get; set; }
    
    [Display(Name = "Unit Cost")]
    public decimal UnitCost { get; set; }
    
    [Display(Name = "Total Value")]
    public decimal TotalValue { get; set; }
    
    [Display(Name = "Age Category")]
    public string AgeCategory { get; set; } = string.Empty;
    
    // Display properties
    public string BatchNoDisplay => BatchNo ?? "N/A";
    public string QuantityDisplay => Quantity.ToString("N2");
    public string PurchaseDateDisplay => PurchaseDate.ToString("dd/MM/yyyy");
    public string ExpiryDateDisplay => ExpiryDate?.ToString("dd/MM/yyyy") ?? "No Expiry";
    public string DaysToExpiryDisplay => DaysToExpiry.HasValue ? DaysToExpiry.Value.ToString() : "-";
    public string UnitCostDisplay => UnitCost.ToString("C2");
    public string TotalValueDisplay => TotalValue.ToString("C2");
    
    // Age color coding
    public string AgeColor => AgeCategory switch
    {
        "0-30 days" => "Green",
        "31-60 days" => "Blue",
        "61-90 days" => "Orange",
        "91-180 days" => "DarkOrange",
        "180+ days" => "Red",
        _ => "Gray"
    };
    
    // Expiry warning color
    public string ExpiryColor => DaysToExpiry.HasValue
        ? DaysToExpiry.Value <= 0 ? "DarkRed"
        : DaysToExpiry.Value <= 7 ? "Red"
        : DaysToExpiry.Value <= 30 ? "Orange"
        : "Green"
        : "Gray";
}
