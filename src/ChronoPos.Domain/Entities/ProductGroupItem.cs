namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents an item (product/variant) within a product group
/// </summary>
public class ProductGroupItem
{
    /// <summary>
    /// Unique identifier for the product group item
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to ProductGroup
    /// </summary>
    public int ProductGroupId { get; set; }

    /// <summary>
    /// Navigation property to ProductGroup
    /// </summary>
    public virtual ProductGroup? ProductGroup { get; set; }

    /// <summary>
    /// Foreign key to Product (nullable to match DB schema)
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// Optional: Specific product unit/variant within the product
    /// </summary>
    public int? ProductUnitId { get; set; }

    /// <summary>
    /// Optional: Specific product combination/variant (references product_combinations table)
    /// Note: ProductCombination entity not yet implemented, storing ID only
    /// </summary>
    public int? ProductCombinationId { get; set; }

    /// <summary>
    /// Quantity of this product in the group
    /// </summary>
    public decimal Quantity { get; set; } = 1;

    /// <summary>
    /// Price adjustment for this specific item (override)
    /// Can be positive (markup) or negative (discount)
    /// </summary>
    public decimal PriceAdjustment { get; set; } = 0;

    /// <summary>
    /// Optional: Specific discount for this item (overrides group discount)
    /// </summary>
    public int? DiscountId { get; set; }

    /// <summary>
    /// Navigation property for discount
    /// </summary>
    public virtual Discount? Discount { get; set; }

    /// <summary>
    /// Optional: Tax type for this item
    /// </summary>
    public int? TaxTypeId { get; set; }

    /// <summary>
    /// Navigation property to TaxType
    /// </summary>
    public virtual TaxType? TaxType { get; set; }

    /// <summary>
    /// Optional: Selling price type for this item
    /// </summary>
    public long? SellingPriceTypeId { get; set; }

    /// <summary>
    /// Navigation property to SellingPriceType
    /// </summary>
    public virtual SellingPriceType? SellingPriceType { get; set; }

    /// <summary>
    /// Status of the item in the group
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Date when the item was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
