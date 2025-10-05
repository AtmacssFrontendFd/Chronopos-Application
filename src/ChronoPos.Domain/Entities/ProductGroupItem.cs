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
    /// Foreign key to Product
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual Product? Product { get; set; }

    /// <summary>
    /// Optional: Specific product unit/variant within the product
    /// </summary>
    public int? ProductUnitId { get; set; }

    /// <summary>
    /// Quantity of this product in the group
    /// </summary>
    public decimal Quantity { get; set; } = 1;

    /// <summary>
    /// Display order/sequence of item in the group
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether this item is required in the group
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Price adjustment for this specific item (override)
    /// Can be positive (markup) or negative (discount)
    /// </summary>
    public decimal? PriceAdjustment { get; set; }

    /// <summary>
    /// Optional: Specific discount for this item (overrides group discount)
    /// </summary>
    public int? DiscountId { get; set; }

    /// <summary>
    /// Navigation property for discount
    /// </summary>
    public virtual Discount? Discount { get; set; }

    /// <summary>
    /// Date when the item was added to the group
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the item was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}
