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
    
    public int StockQuantity { get; set; } = 0;
    
    [StringLength(50)]
    public string? SKU { get; set; }
    
    [StringLength(100)]
    public string? Barcode { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public decimal CostPrice { get; set; } = 0;
    
    public decimal? Markup { get; set; }
    
    public string? ImagePath { get; set; }
    
    public string? Color { get; set; } = "#FFC107"; // Default golden color
    
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
}
