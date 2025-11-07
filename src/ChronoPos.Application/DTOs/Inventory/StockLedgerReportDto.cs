using System.ComponentModel.DataAnnotations;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs.Inventory;

/// <summary>
/// DTO for Stock Ledger/Movement Report - Shows detailed transaction history
/// </summary>
public class StockLedgerReportDto
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Display(Name = "Product")]
    public string ProductName { get; set; } = string.Empty;
    
    [Display(Name = "Date")]
    public DateTime Date { get; set; }
    
    [Display(Name = "Transaction Type")]
    public StockMovementType TransactionType { get; set; }
    
    [Display(Name = "Reference Type")]
    public StockReferenceType? ReferenceType { get; set; }
    
    [Display(Name = "Ref No")]
    public string RefNo { get; set; } = string.Empty;
    
    [Display(Name = "Location")]
    public string? Location { get; set; }
    
    [Display(Name = "In Qty")]
    public decimal InQty { get; set; }
    
    [Display(Name = "Out Qty")]
    public decimal OutQty { get; set; }
    
    [Display(Name = "Balance")]
    public decimal Balance { get; set; }
    
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }
    
    // Display properties
    public string DateDisplay => Date.ToString("dd/MM/yyyy HH:mm");
    public string TransactionTypeDisplay => TransactionType.ToString();
    public string ReferenceTypeDisplay => ReferenceType?.ToString() ?? "-";
    public string InQtyDisplay => InQty > 0 ? $"+{InQty:N2}" : "-";
    public string OutQtyDisplay => OutQty > 0 ? $"-{OutQty:N2}" : "-";
    public string BalanceDisplay => Balance.ToString("N2");
    public string LocationDisplay => Location ?? "-";
    public string RemarksDisplay => Remarks ?? "-";
    
    // Color coding
    public string TransactionColor => TransactionType switch
    {
        StockMovementType.Purchase => "Green",
        StockMovementType.Sale => "Red",
        StockMovementType.Adjustment => "Orange",
        StockMovementType.TransferIn => "Blue",
        StockMovementType.TransferOut => "Purple",
        StockMovementType.Return => "Teal",
        StockMovementType.Waste => "DarkRed",
        _ => "Black"
    };
}
