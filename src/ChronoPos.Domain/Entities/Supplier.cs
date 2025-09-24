using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities;

[Table("supplier")]
public class Supplier
{
    [Key]
    [Column("supplier_id")]
    public long SupplierId { get; set; }

    [Column("shop_id")]
    public long? ShopId { get; set; }

    [Required]
    [Column("company_name")]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Column("logo_picture")]
    [MaxLength(255)]
    public string? LogoPicture { get; set; }

    [Column("license_number")]
    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    [Column("owner_name")]
    [MaxLength(100)]
    public string? OwnerName { get; set; }

    [Column("owner_mobile")]
    [MaxLength(20)]
    public string? OwnerMobile { get; set; }

    [Column("vat_trn_number")]
    [MaxLength(50)]
    public string? VatTrnNumber { get; set; }

    [Column("email")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required]
    [Column("address_line1")]
    [MaxLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;

    [Column("address_line2")]
    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    [Column("building")]
    [MaxLength(100)]
    public string? Building { get; set; }

    [Column("area")]
    [MaxLength(100)]
    public string? Area { get; set; }

    [Column("po_box")]
    [MaxLength(20)]
    public string? PoBox { get; set; }

    [Column("city")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Column("state")]
    [MaxLength(100)]
    public string? State { get; set; }

    [Column("country")]
    [MaxLength(100)]
    public string? Country { get; set; }

    [Column("website")]
    [MaxLength(100)]
    public string? Website { get; set; }

    [Column("key_contact_name")]
    [MaxLength(100)]
    public string? KeyContactName { get; set; }

    [Column("key_contact_mobile")]
    [MaxLength(20)]
    public string? KeyContactMobile { get; set; }

    [Column("key_contact_email")]
    [MaxLength(100)]
    public string? KeyContactEmail { get; set; }

    [Column("mobile")]
    [MaxLength(20)]
    public string? Mobile { get; set; }

    [Column("location_latitude")]
    public decimal? LocationLatitude { get; set; }

    [Column("location_longitude")]
    public decimal? LocationLongitude { get; set; }

    [Column("company_phone_number")]
    [MaxLength(20)]
    public string? CompanyPhoneNumber { get; set; }

    [Column("gstin")]
    [MaxLength(20)]
    public string? Gstin { get; set; }

    [Column("pan")]
    [MaxLength(20)]
    public string? Pan { get; set; }

    [Column("payment_terms")]
    [MaxLength(50)]
    public string? PaymentTerms { get; set; }

    [Column("opening_balance")]
    public decimal OpeningBalance { get; set; } = 0;

    [Column("balance_type")]
    [MaxLength(20)]
    public string BalanceType { get; set; } = "credit";

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    [Column("credit_limit")]
    public decimal CreditLimit { get; set; } = 0;

    [Column("created_by")]
    public long? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("deleted_by")]
    public long? DeletedBy { get; set; }

    // Computed properties
    public bool IsActive => Status == "Active" && DeletedAt == null;
    public string DisplayName => !string.IsNullOrEmpty(CompanyName) ? CompanyName : "Unknown Supplier";
    public string FullAddress => string.Join(", ", new[] { AddressLine1, AddressLine2, Building, Area, City, State, Country }
        .Where(s => !string.IsNullOrWhiteSpace(s)));
}