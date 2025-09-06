namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for stock adjustment item display
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
}
