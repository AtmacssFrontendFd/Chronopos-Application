using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for ProductCombinationItem operations
/// </summary>
public class ProductCombinationItemDto
{
    public int Id { get; set; }
    
    [Required]
    public int ProductUnitId { get; set; }
    
    [Required]
    public int AttributeValueId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties for UI
    public string? ProductUnitSku { get; set; }
    public string? ProductUnitName { get; set; }
    public string? AttributeValueName { get; set; }
    public string? AttributeValueNameAr { get; set; }
    public string? AttributeName { get; set; }
    public string? AttributeNameAr { get; set; }
    
    // For UI binding
    public bool IsNew { get; set; } = true;
    
    // Display properties for UI
    public string ProductUnitDisplay => !string.IsNullOrEmpty(ProductUnitName) ? 
        $"{ProductUnitSku} - {ProductUnitName}" : ProductUnitSku ?? "Unknown";
    
    public string AttributeValueDisplay => !string.IsNullOrEmpty(AttributeValueName) ? 
        AttributeValueName : "Unknown";
    
    public string CreatedAtFormatted => CreatedAt.ToString("yyyy-MM-dd HH:mm");
}