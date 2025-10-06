namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product group for bundling, combos, and kits
/// </summary>
public class ProductGroup
{
    /// <summary>
    /// Unique identifier for the product group
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the product group (e.g., "Burger Combo", "Starter Pack")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Arabic name of the product group
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// Detailed description of what this group includes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Arabic description
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Optional default discount applied to all items in the group
    /// </summary>
    public int? DiscountId { get; set; }

    /// <summary>
    /// Navigation property for discount
    /// </summary>
    public virtual Discount? Discount { get; set; }

    /// <summary>
    /// Optional default tax type for the group
    /// </summary>
    public int? TaxTypeId { get; set; }

    /// <summary>
    /// Navigation property for tax type
    /// </summary>
    public virtual TaxType? TaxType { get; set; }

    /// <summary>
    /// Optional reference to price type (e.g., MRP, Retail, Wholesale)
    /// </summary>
    public long? PriceTypeId { get; set; }

    /// <summary>
    /// Navigation property for selling price type
    /// </summary>
    public virtual SellingPriceType? PriceType { get; set; }

    /// <summary>
    /// SKU prefix used to auto-generate SKUs for bundled products
    /// </summary>
    public string? SkuPrefix { get; set; }

    /// <summary>
    /// Status of the product group (Active/Inactive)
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Date when the product group was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the product group was last modified
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// User ID who created this product group
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// Navigation property for user who created the group
    /// </summary>
    public virtual User? CreatedByUser { get; set; }

    /// <summary>
    /// User ID who last modified this product group
    /// </summary>
    public int? ModifiedBy { get; set; }

    /// <summary>
    /// Navigation property for user who modified the group
    /// </summary>
    public virtual User? ModifiedByUser { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date when the product group was deleted
    /// </summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>
    /// User ID who deleted this product group
    /// </summary>
    public int? DeletedBy { get; set; }

    /// <summary>
    /// Navigation property for user who deleted the group
    /// </summary>
    public virtual User? DeletedByUser { get; set; }

    /// <summary>
    /// Collection of products in this group
    /// </summary>
    public virtual ICollection<ProductGroupItem>? ProductGroupItems { get; set; }
}
