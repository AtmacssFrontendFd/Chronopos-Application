using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a shop location/store
/// </summary>
public class ShopLocation
{
    public int Id { get; set; }
    
    [Required]
    public int ShopId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string LocationType { get; set; } = string.Empty; // Retail/Warehouse/etc.
    
    [Required]
    [StringLength(100)]
    public string LocationName { get; set; } = string.Empty;
    
    public int? ManagerId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string? AddressLine2 { get; set; }
    
    [StringLength(100)]
    public string? Building { get; set; }
    
    [StringLength(100)]
    public string? Area { get; set; }
    
    [StringLength(20)]
    public string? PoBox { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    public int? StateId { get; set; }
    
    public int? CountryId { get; set; }
    
    [StringLength(20)]
    public string? LandlineNumber { get; set; }
    
    [StringLength(20)]
    public string? MobileNumber { get; set; }
    
    public decimal? LocationLatitude { get; set; }
    
    public decimal? LocationLongitude { get; set; }
    
    public bool CanSell { get; set; } = true;
    
    public int? LanguageId { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
}
