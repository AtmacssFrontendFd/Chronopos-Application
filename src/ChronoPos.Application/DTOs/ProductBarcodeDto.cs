using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProductBarcode operations
/// </summary>
public class ProductBarcodeDto
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
    public string Barcode { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string BarcodeType { get; set; } = "ean";
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties for UI
    public string? ProductUnitName { get; set; }
    public string? ProductUnitSku { get; set; }
    
    // For UI binding
    public bool IsNew { get; set; } = true;
}