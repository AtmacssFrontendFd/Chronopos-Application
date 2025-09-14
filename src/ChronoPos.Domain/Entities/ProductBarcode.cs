using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a product barcode (matches product_barcodes table)
/// </summary>
public class ProductBarcode
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Barcode { get; set; } = string.Empty; // Changed from 'Value' to 'Barcode'
    
    [StringLength(20)]
    public string BarcodeType { get; set; } = "ean"; // Default to 'ean'
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Product Product { get; set; } = null!;
}
