using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a company in the POS system
/// </summary>
public class Company
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(255)]
    public string CompanyName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? LogoPath { get; set; }
    
    [StringLength(100)]
    public string? LicenseNumber { get; set; }
    
    public int? NumberOfOwners { get; set; }
    
    [StringLength(100)]
    public string? VatTrnNumber { get; set; }
    
    [StringLength(20)]
    public string? PhoneNo { get; set; }
    
    [StringLength(255)]
    public string? EmailOfBusiness { get; set; }
    
    [StringLength(255)]
    public string? Website { get; set; }
    
    [StringLength(255)]
    public string? KeyContactName { get; set; }
    
    [StringLength(20)]
    public string? KeyContactMobNo { get; set; }
    
    [StringLength(255)]
    public string? KeyContactEmail { get; set; }
    
    public decimal? LocationLatitude { get; set; }
    
    public decimal? LocationLongitude { get; set; }
    
    public string? Remarks { get; set; }
    
    public bool Status { get; set; } = true; // 1 = Active, 0 = Inactive
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string? UpdatedBy { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    [StringLength(100)]
    public string? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<CompanySettings> CompanySettings { get; set; } = new List<CompanySettings>();
}
