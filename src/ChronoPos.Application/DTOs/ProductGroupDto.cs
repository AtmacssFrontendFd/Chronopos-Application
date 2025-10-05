namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for displaying product group information
/// </summary>
public class ProductGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int? DiscountId { get; set; }
    public string? DiscountName { get; set; }
    public int? TaxTypeId { get; set; }
    public string? TaxTypeName { get; set; }
    public long? PriceTypeId { get; set; }
    public string? PriceTypeName { get; set; }
    public string? SkuPrefix { get; set; }
    public string Status { get; set; } = "Active";
    public int ItemCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// DTO for creating a new product group
/// </summary>
public class CreateProductGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int? DiscountId { get; set; }
    public int? TaxTypeId { get; set; }
    public long? PriceTypeId { get; set; }
    public string? SkuPrefix { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for updating an existing product group
/// </summary>
public class UpdateProductGroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int? DiscountId { get; set; }
    public int? TaxTypeId { get; set; }
    public long? PriceTypeId { get; set; }
    public string? SkuPrefix { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for product group item information
/// </summary>
public class ProductGroupItemDto
{
    public int Id { get; set; }
    public int ProductGroupId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? ProductUnitId { get; set; }
    public string? ProductUnitName { get; set; }
    public decimal Quantity { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public decimal? PriceAdjustment { get; set; }
    public int? DiscountId { get; set; }
    public string? DiscountName { get; set; }
    public decimal ProductPrice { get; set; }
    public decimal CalculatedPrice { get; set; }
}

/// <summary>
/// DTO for creating a new product group item
/// </summary>
public class CreateProductGroupItemDto
{
    public int ProductGroupId { get; set; }
    public int ProductId { get; set; }
    public int? ProductUnitId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public decimal? PriceAdjustment { get; set; }
    public int? DiscountId { get; set; }
}

/// <summary>
/// DTO for detailed product group with all items
/// </summary>
public class ProductGroupDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public int? DiscountId { get; set; }
    public string? DiscountName { get; set; }
    public int? TaxTypeId { get; set; }
    public string? TaxTypeName { get; set; }
    public long? PriceTypeId { get; set; }
    public string? PriceTypeName { get; set; }
    public string? SkuPrefix { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<ProductGroupItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
}
