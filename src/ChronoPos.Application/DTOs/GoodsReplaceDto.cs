namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for goods replace display and transfer
/// </summary>
public class GoodsReplaceDto
{
    public int Id { get; set; }
    public string ReplaceNo { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int? ReferenceReturnId { get; set; }
    public string? ReferenceReturnNo { get; set; }
    public DateTime ReplaceDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int TotalItems { get; set; }
    public List<GoodsReplaceItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating new goods replace
/// </summary>
public class CreateGoodsReplaceDto
{
    public int SupplierId { get; set; }
    public int StoreId { get; set; }
    public int? ReferenceReturnId { get; set; }
    public DateTime ReplaceDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Remarks { get; set; }
    public List<CreateGoodsReplaceItemDto> Items { get; set; } = new();
}

/// <summary>
/// DTO for creating goods replace items
/// </summary>
public class CreateGoodsReplaceItemDto
{
    public int ProductId { get; set; }
    public long UomId { get; set; }
    public string? BatchNo { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal Rate { get; set; }
    public int? ReferenceReturnItemId { get; set; }
    public string? RemarksLine { get; set; }
}
