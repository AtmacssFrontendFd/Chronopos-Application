using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Product operations
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    public int CategoryId { get; set; }
    
    public string CategoryName { get; set; } = string.Empty;
    
    public int? BrandId { get; set; }
    
    public string BrandName { get; set; } = string.Empty;
    
    public int StockQuantity { get; set; } = 0;
    
    [StringLength(50)]
    public string? SKU { get; set; }
    
    [StringLength(100)]
    public string? Barcode { get; set; }
    
    // Multiple barcodes support
    public List<ProductBarcodeDto> ProductBarcodes { get; set; } = new();

    public bool IsActive { get; set; } = true;    public decimal CostPrice { get; set; } = 0;
    
    public decimal? Markup { get; set; }
    
    public string? ImagePath { get; set; }
    
    public string? Color { get; set; } = "#FFC107"; // Default golden color

    // Tax & Attributes (requested to persist in Product table)
    public bool IsTaxInclusivePrice { get; set; } = true;
    public bool IsDiscountAllowed { get; set; } = true;
    public decimal MaxDiscount { get; set; } = 100;
    public bool IsPriceChangeAllowed { get; set; } = true;
    public bool IsService { get; set; } = false;
    public int? AgeRestriction { get; set; }

    // Selected tax types for ProductTaxes mapping
    public List<int> SelectedTaxTypeIds { get; set; } = new();
    
    // Selected discount IDs for ProductDiscounts mapping
    public List<int> SelectedDiscountIds { get; set; } = new();

    // Discount display information
    public List<DiscountDisplayDto> ActiveDiscounts { get; set; } = new();
    public string ActiveDiscountsDisplay { get; set; } = string.Empty;
    public bool HasActiveDiscounts => ActiveDiscounts.Any();
    
    /// <summary>
    /// Compact discount display for tables (e.g., "NewYear2025 +3")
    /// </summary>
    public string CompactDiscountsDisplay => DiscountDisplayDto.GetCompactDisplay(ActiveDiscounts);

    // Display-only: cached tax-inclusive price value (Price + taxes)
    public decimal TaxInclusivePriceValue { get; set; } = 0;
    
    // Unit of Measurement
    public int UnitOfMeasurementId { get; set; } = 1;
    public string UnitOfMeasurementName { get; set; } = "pcs";
    public string UnitOfMeasurementAbbreviation { get; set; } = "pcs";
    
    // Purchase and Selling Units (can be different from base UOM)
    public int? PurchaseUnitId { get; set; }
    public string PurchaseUnitName { get; set; } = string.Empty;
    
    public int? SellingUnitId { get; set; }
    public string SellingUnitName { get; set; } = string.Empty;
    
    // Product Grouping
    public int? ProductGroupId { get; set; }
    public string? Group { get; set; } // For backwards compatibility
    
    // Business Rules (Additional)
    public bool CanReturn { get; set; } = true;
    public bool IsGrouped { get; set; } = false;
    
    // Stock Control Properties
    public bool IsStockTracked { get; set; } = true;
    public bool AllowNegativeStock { get; set; } = false;
    public bool IsUsingSerialNumbers { get; set; } = false;
    public decimal InitialStock { get; set; } = 0;
    public decimal MinimumStock { get; set; } = 0;
    public decimal MaximumStock { get; set; } = 0;
    public decimal ReorderLevel { get; set; } = 0;
    public decimal ReorderQuantity { get; set; } = 0;
    public decimal AverageCost { get; set; } = 0;
    public decimal LastCost { get; set; } = 0;
    public int SelectedStoreId { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Display properties
    public string DisplayName => $"{Name} - ${Price:F2}";
    public string StockDisplay => $"{StockQuantity} items";
    public bool HasLowStock => StockQuantity < 10;
    
    // Override ToString to return the Name for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}
