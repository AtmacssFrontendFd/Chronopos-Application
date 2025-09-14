using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product in the Point of Sale system
/// </summary>
public class Product
{
    public int Id { get; set; }
    
    // Basic Information
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;  // Product code
    
    public int PLU { get; set; }  // Price Lookup Number
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(300)]
    public string? Description { get; set; }
    
    // Pricing
    [Required]
    public decimal Price { get; set; }
    
    public decimal Cost { get; set; } = 0;
    
    public decimal LastPurchasePrice { get; set; } = 0;
    
    public decimal? Markup { get; set; }
    
    // Tax Configuration
    public bool IsTaxInclusivePrice { get; set; } = true;
    
    public decimal TaxRate { get; set; } = 0;
    
    public decimal Excise { get; set; } = 0;
    
    // Category
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // Brand
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }
    
    // Inventory & Stock Control (Enhanced)
    public bool IsStockTracked { get; set; } = true;
    
    public bool AllowNegativeStock { get; set; } = false;
    
    public bool IsUsingSerialNumbers { get; set; } = false;
    
    // Stock Levels (Initial Setup)
    public decimal InitialStock { get; set; } = 0;
    
    public decimal MinimumStock { get; set; } = 0;
    
    public decimal MaximumStock { get; set; } = 0;
    
    public decimal ReorderLevel { get; set; } = 0;
    
    public decimal ReorderQuantity { get; set; } = 0;
    
    // Cost Information
    public decimal AverageCost { get; set; } = 0;
    
    public decimal LastCost { get; set; } = 0;
    
    // Legacy stock quantity (for compatibility)
    public int StockQuantity { get; set; } = 0;
    
    [StringLength(50)]
    public string? SKU { get; set; }
    
    // Measurement & Packaging
    public int UnitOfMeasurementId { get; set; } = 1; // Default to first UOM (pcs)
    public virtual UnitOfMeasurement? UnitOfMeasurement { get; set; }
    
    // Purchase and Selling Units (can be different from base UOM)
    public int? PurchaseUnitId { get; set; }
    public virtual UnitOfMeasurement? PurchaseUnit { get; set; }
    
    public int? SellingUnitId { get; set; }
    public virtual UnitOfMeasurement? SellingUnit { get; set; }
    
    // Product Grouping
    public int? ProductGroupId { get; set; }
    public string? Group { get; set; } // For backwards compatibility
    
    // Business Rules (Additional)
    public bool CanReturn { get; set; } = true;
    public bool IsGrouped { get; set; } = false;
    
    // Navigation properties for related entities
    public virtual ICollection<ProductBarcode> ProductBarcodes { get; set; } = new List<ProductBarcode>();
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductComment> ProductComments { get; set; } = new List<ProductComment>();
    public virtual ICollection<ProductTax> ProductTaxes { get; set; } = new List<ProductTax>();
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
    public virtual ICollection<StockAlert> StockAlerts { get; set; } = new List<StockAlert>();
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    
    // Visual
    public string? ImagePath { get; set; }
    
    public string? Color { get; set; } = "#FFC107";
    
    // Business Rules
    public bool IsDiscountAllowed { get; set; } = true;
    
    public decimal MaxDiscount { get; set; } = 100;
    
    public bool IsPriceChangeAllowed { get; set; } = true;
    
    public bool IsManufactureRequired { get; set; } = false;
    
    public bool IsService { get; set; } = false;
    
    public bool IsUsingDefaultQuantity { get; set; } = true;
    
    // Additional Properties
    public bool IsActive { get; set; } = true;
    
    public int? AgeRestriction { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Computed Properties
    public bool HasComments => ProductComments?.Any() == true;
    
    // Stock Status Methods
    private string GetStockStatus()
    {
        if (!IsStockTracked) return "Not Tracked";
        if (StockQuantity <= 0) return "Out of Stock";
        if (StockQuantity <= ReorderLevel) return "Low Stock";
        if (StockQuantity >= MaximumStock && MaximumStock > 0) return "Overstock";
        return "In Stock";
    }
    
    public bool IsLowStock => IsStockTracked && StockQuantity <= ReorderLevel && ReorderLevel > 0;
    
    public bool IsOutOfStock => IsStockTracked && StockQuantity <= 0;
    
    public bool IsOverstock => IsStockTracked && MaximumStock > 0 && StockQuantity >= MaximumStock;
}
