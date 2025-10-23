using ChronoPos.Domain.Entities;

namespace ChronoPos.Desktop.Models;

/// <summary>
/// Extended ProductUnit model for UI display with attribute values
/// </summary>
public class ProductUnitWithAttributes : ProductUnit
{
    /// <summary>
    /// Comma-separated string of all attribute values for this product unit
    /// </summary>
    public string AttributeValues { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of combinations for this product unit
    /// </summary>
    public int CombinationCount { get; set; }
    
    /// <summary>
    /// Constructor from ProductUnit entity
    /// </summary>
    /// <param name="productUnit">Source ProductUnit entity</param>
    public ProductUnitWithAttributes(ProductUnit productUnit)
    {
        Id = productUnit.Id;
        ProductId = productUnit.ProductId;
        UnitId = productUnit.UnitId;
        Sku = productUnit.Sku;
        QtyInUnit = productUnit.QtyInUnit;
        CostOfUnit = productUnit.CostOfUnit;
        PriceOfUnit = productUnit.PriceOfUnit;
        SellingPriceId = productUnit.SellingPriceId;
        PriceType = productUnit.PriceType;
        DiscountAllowed = productUnit.DiscountAllowed;
        IsBase = productUnit.IsBase;
        CreatedAt = productUnit.CreatedAt;
        UpdatedAt = productUnit.UpdatedAt;
        Product = productUnit.Product;
        Unit = productUnit.Unit;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public ProductUnitWithAttributes()
    {
    }
}