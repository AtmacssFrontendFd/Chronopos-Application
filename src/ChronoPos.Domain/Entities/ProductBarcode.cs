using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product barcode (matches product_barcodes table)
/// </summary>
public class ProductBarcode
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    /// <summary>
    /// Optional ProductUnit ID - allows barcodes to be specific to a product unit (UOM)
    /// </summary>
    public int? ProductUnitId { get; set; }
    
    /// <summary>
    /// Optional ProductGroup ID - allows barcodes to be associated with product groups
    /// </summary>
    public int? ProductGroupId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Barcode { get; set; } = string.Empty; // Changed from 'Value' to 'Barcode'
    
    [StringLength(20)]
    public string BarcodeType { get; set; } = "ean"; // Default to 'ean'
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ProductUnit? ProductUnit { get; set; }
}
