namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for creating new stock transfers
/// </summary>
public class CreateStockTransferDto
{
    public DateTime TransferDate { get; set; }
    public int FromStoreId { get; set; }
    public int ToStoreId { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Remarks { get; set; }
    public List<CreateStockTransferItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating stock transfer items
/// </summary>
public class CreateStockTransferItemDto
{
    public int TransferId { get; set; }
    public int ProductId { get; set; }
    public long UomId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal QuantitySent { get; set; }
    public string? RemarksLine { get; set; }
}
