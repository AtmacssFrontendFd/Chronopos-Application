namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for stock adjustment item display with financial calculations and adjustment details
/// </summary>
public class StockAdjustmentItemDto
{
    public int Id { get; set; }
    public int AdjustmentId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public decimal DifferenceQty { get; set; }
    public string AdjustmentType { get; set; } = string.Empty; // "Increase" or "Decrease"
    public string UomName { get; set; } = string.Empty;
    public string? ReasonLine { get; set; }
    public string? RemarksLine { get; set; }
    public string ShopLocation { get; set; } = string.Empty;
    
    // Financial Fields for Table Display
    public decimal CostPrice { get; set; } = 0; // Product.Cost
    public decimal TaxRate { get; set; } = 0; // Product.TaxRate (as decimal, e.g., 0.1 for 10%)
    
    // Adjustment-level data for flattened display
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    
    // Calculated Properties
    public decimal CostInclusive => CostPrice * (1 + TaxRate); // Cost with tax
    public decimal ValueAfter => QuantityAfter * CostPrice; // Quantity * Cost Price
    public decimal InclusiveValue => QuantityAfter * CostPrice * (1 + TaxRate); // Quantity * Cost Price with tax
}
