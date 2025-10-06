namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for stock transfer display and transfer
/// </summary>
public class StockTransferDto
{
    public int TransferId { get; set; }
    public string TransferNo { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public int FromStoreId { get; set; }
    public string FromStoreName { get; set; } = string.Empty;
    public int ToStoreId { get; set; }
    public string ToStoreName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
}



/// <summary>
/// DTO for shop location in transfers
/// </summary>
public class ShopLocationDto
{
    public int Id { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string? City { get; set; }
    public string Status { get; set; } = string.Empty;
}
