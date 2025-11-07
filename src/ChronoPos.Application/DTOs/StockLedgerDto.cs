using System.ComponentModel.DataAnnotations;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for StockLedger operations
/// </summary>
public class StockLedgerDto
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int UnitId { get; set; }
    
    [Required]
    public StockMovementType MovementType { get; set; }
    
    [Required]
    public decimal Qty { get; set; }
    
    [Required]
    public decimal Balance { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    public StockReferenceType? ReferenceType { get; set; }
    
    public int? ReferenceId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string? Note { get; set; }
    
    // Navigation properties
    public string? ProductName { get; set; }
    public string? UnitName { get; set; }
    
    // Display properties for UI binding
    public string MovementTypeDisplay => MovementType.ToString();
    public string ReferenceTypeDisplay => ReferenceType?.ToString() ?? "-";
    public string ReferenceIdDisplay => ReferenceId?.ToString() ?? "-";
    public string LocationDisplay => Location ?? "-";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm:ss");
    public string QtyFormatted => Qty.ToString("N2");
    public string BalanceFormatted => Balance.ToString("N2");
    public string NoteDisplay => Note ?? "-";
    
    // Color coding for movement types
    public string MovementColor => MovementType switch
    {
        StockMovementType.Purchase => "Green",
        StockMovementType.Sale => "Red",
        StockMovementType.Adjustment => "Orange",
        StockMovementType.TransferIn => "Blue",
        StockMovementType.TransferOut => "Purple",
        StockMovementType.Return => "Teal",
        StockMovementType.Replace => "Brown",
        StockMovementType.Waste => "DarkRed",
        StockMovementType.Opening => "DarkGreen",
        StockMovementType.Closing => "DarkBlue",
        _ => "Black"
    };
    
    // Sign indicator for display
    public string QtySign => MovementType switch
    {
        StockMovementType.Purchase => "+",
        StockMovementType.Sale => "-",
        StockMovementType.TransferIn => "+",
        StockMovementType.TransferOut => "-",
        StockMovementType.Return => "+",
        StockMovementType.Waste => "-",
        StockMovementType.Opening => "+",
        _ => ""
    };
}

/// <summary>
/// DTO for creating a new stock ledger entry
/// </summary>
public class CreateStockLedgerDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int UnitId { get; set; }
    
    [Required]
    public StockMovementType MovementType { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Qty { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    public StockReferenceType? ReferenceType { get; set; }
    
    public int? ReferenceId { get; set; }
    
    public string? Note { get; set; }
}

/// <summary>
/// DTO for updating an existing stock ledger entry
/// </summary>
public class UpdateStockLedgerDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int UnitId { get; set; }
    
    [Required]
    public StockMovementType MovementType { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Qty { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    public StockReferenceType? ReferenceType { get; set; }
    
    public int? ReferenceId { get; set; }
    
    public string? Note { get; set; }
}
