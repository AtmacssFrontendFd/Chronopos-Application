using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an item in a stock transfer transaction
/// </summary>
public class StockTransferItem
{
    public int Id { get; set; }
    
    [Required]
    public int TransferId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public long UomId { get; set; }
    
    [StringLength(50)]
    public string? BatchNo { get; set; }
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    public decimal QuantitySent { get; set; }
    
    public decimal QuantityReceived { get; set; } = 0;
    
    public decimal DamagedQty { get; set; } = 0;
    
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Received, Damaged
    
    public string? RemarksLine { get; set; }
    
    // Navigation Properties
    public virtual StockTransfer? Transfer { get; set; }
    public virtual Product? Product { get; set; }
    public virtual UnitOfMeasurement? Uom { get; set; }
}
