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
/// DTO for stock transfer item display
/// </summary>
public class StockTransferItemDto
{
    public int Id { get; set; }
    public int TransferId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantitySent { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal DamagedQty { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RemarksLine { get; set; }
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
