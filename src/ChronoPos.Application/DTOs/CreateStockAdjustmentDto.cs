using ChronoPos.Domain.Enums;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for creating new stock adjustments
/// </summary>
public class CreateStockAdjustmentDto
{
    public DateTime AdjustmentDate { get; set; }
    public int StoreLocationId { get; set; }
    public int ReasonId { get; set; }
    public string? Remarks { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockAdjustmentItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating stock adjustment items
/// </summary>
public class CreateStockAdjustmentItemDto
{
    public int ProductId { get; set; }
    public long UomId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public string? ReasonLine { get; set; }
    public string? RemarksLine { get; set; }
    
    /// <summary>
    /// The adjustment mode (Product or ProductUnit)
    /// </summary>
    public StockAdjustmentMode AdjustmentMode { get; set; } = StockAdjustmentMode.Product;
    
    /// <summary>
    /// For ProductUnit mode: the ProductUnit ID
    /// </summary>
    public int? ProductUnitId { get; set; }
    
    /// <summary>
    /// The conversion factor from UOM (default: 1 for Product mode, ProductUnit conversion factor for ProductUnit mode)
    /// </summary>
    public decimal ConversionFactor { get; set; } = 1;
    
    /// <summary>
    /// Whether the adjustment is an increment (true) or decrement (false)
    /// </summary>
    public bool IsIncrement { get; set; } = true;
    
    /// <summary>
    /// The change amount to apply (always positive, direction determined by IsIncrement)
    /// </summary>
    public decimal ChangeAmount { get; set; } = 0;
}
