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
    public int UomId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public string? ReasonLine { get; set; }
    public string? RemarksLine { get; set; }
}
