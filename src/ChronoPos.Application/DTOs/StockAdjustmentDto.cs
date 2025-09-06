namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for stock adjustment display and transfer
/// </summary>
public class StockAdjustmentDto
{
    public int AdjustmentId { get; set; }
    public string AdjustmentNo { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public int StoreLocationId { get; set; }
    public string StoreLocationName { get; set; } = string.Empty;
    public int ReasonId { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}
