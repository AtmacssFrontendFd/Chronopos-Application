using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a customer address
/// </summary>
public class CustomerAddress
{
    public int Id { get; set; }
    
    public int CustomerId { get; set; }
    
    [StringLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [StringLength(255)]
    public string AddressLine2 { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string PoBox { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string Area { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string City { get; set; } = string.Empty;
    
    public int? StateId { get; set; }
    
    public int? CountryId { get; set; }
    
    [StringLength(255)]
    public string Landmark { get; set; } = string.Empty;
    
    public decimal? Latitude { get; set; }
    
    public decimal? Longitude { get; set; }
    
    public bool IsBilling { get; set; } = false;
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties
    public Customer Customer { get; set; } = null!;
    
    /// <summary>
    /// Returns the full address as a formatted string
    /// </summary>
    public string FullAddress
    {
        get
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(AddressLine1))
                parts.Add(AddressLine1);
            
            if (!string.IsNullOrEmpty(AddressLine2))
                parts.Add(AddressLine2);
            
            if (!string.IsNullOrEmpty(Area))
                parts.Add(Area);
            
            if (!string.IsNullOrEmpty(City))
                parts.Add(City);
            
            if (!string.IsNullOrEmpty(PoBox))
                parts.Add($"P.O. Box: {PoBox}");
            
            return string.Join(", ", parts);
        }
    }
}