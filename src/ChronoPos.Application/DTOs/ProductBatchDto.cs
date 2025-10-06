namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for product batch information
/// </summary>
public class ProductBatchDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public long UomId { get; set; }
    public string UomName { get; set; } = string.Empty;
    public decimal? CostPrice { get; set; }
    public decimal? LandedCost { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    
    // Calculated properties
    public int DaysToExpiry => ExpiryDate.HasValue ? (ExpiryDate.Value - DateTime.Now).Days : int.MaxValue;
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Now;
    public bool IsNearExpiry => DaysToExpiry <= 30 && DaysToExpiry > 0; // Within 30 days
    public decimal TotalValue => Quantity * (CostPrice ?? 0);
}

/// <summary>
/// DTO for creating or updating product batches
/// </summary>
public class CreateProductBatchDto
{
    public int ProductId { get; set; }
    public string BatchNo { get; set; } = string.Empty;
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal Quantity { get; set; }
    public long UomId { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? LandedCost { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for batch summary information
/// </summary>
public class ProductBatchSummaryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int TotalBatches { get; set; }
    public decimal TotalQuantity { get; set; }
    public int ExpiredBatches { get; set; }
    public int NearExpiryBatches { get; set; }
    public decimal TotalValue { get; set; }
}