namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for creating new goods returns
/// </summary>
public class CreateGoodsReturnDto
{
    public long SupplierId { get; set; }
    public int StoreId { get; set; }
    public int? ReferenceGrnId { get; set; }
    public DateTime ReturnDate { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Remarks { get; set; }
    public List<CreateGoodsReturnItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating goods return items
/// </summary>
public class CreateGoodsReturnItemDto
{
    public int ReturnId { get; set; }
    public int ProductId { get; set; }
    public int? BatchId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public long UomId { get; set; }
    public decimal CostPrice { get; set; }
    public string? Reason { get; set; }
}