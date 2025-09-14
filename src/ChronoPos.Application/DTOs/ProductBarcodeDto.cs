using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProductBarcode operations
/// </summary>
public class ProductBarcodeDto
{
    public int Id { get; set; }
    
    public int ProductId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Barcode { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string BarcodeType { get; set; } = "ean";
    
    public DateTime CreatedAt { get; set; }
    
    // For UI binding
    public bool IsNew { get; set; } = true;
}