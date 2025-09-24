namespace ChronoPos.Application.DTOs;

public class SupplierDto
{
    public long SupplierId { get; set; }
    public long? ShopId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoPicture { get; set; }
    public string? LicenseNumber { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerMobile { get; set; }
    public string? VatTrnNumber { get; set; }
    public string? Email { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? Building { get; set; }
    public string? Area { get; set; }
    public string? PoBox { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Website { get; set; }
    public string? KeyContactName { get; set; }
    public string? KeyContactMobile { get; set; }
    public string? KeyContactEmail { get; set; }
    public string? Mobile { get; set; }
    public decimal? LocationLatitude { get; set; }
    public decimal? LocationLongitude { get; set; }
    public string? CompanyPhoneNumber { get; set; }
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal OpeningBalance { get; set; } = 0;
    public string BalanceType { get; set; } = "credit";
    public string Status { get; set; } = "Active";
    public decimal CreditLimit { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed properties
    public string DisplayName => !string.IsNullOrEmpty(CompanyName) ? CompanyName : "Unknown Supplier";
    public string FullAddress => string.Join(", ", new[] { AddressLine1, AddressLine2, Building, Area, City, State, Country }
        .Where(s => !string.IsNullOrWhiteSpace(s)));
    public string PrimaryContact => !string.IsNullOrEmpty(KeyContactName) ? 
        $"{KeyContactName} ({KeyContactMobile})" : 
        (!string.IsNullOrEmpty(OwnerName) ? $"{OwnerName} ({OwnerMobile})" : "No contact");
    
    // Compatibility properties for existing ViewModels
    public bool IsActive 
    { 
        get => Status == "Active";
        set => Status = value ? "Active" : "Inactive";
    }
    
    // Compatibility properties for existing ViewModels
    public string ContactName 
    { 
        get => KeyContactName ?? OwnerName ?? string.Empty;
        set => KeyContactName = value;
    }
    
    public string PhoneNumber 
    { 
        get => CompanyPhoneNumber ?? Mobile ?? OwnerMobile ?? string.Empty;
        set => CompanyPhoneNumber = value;
    }
    
    public string Address 
    { 
        get => FullAddress;
        set => AddressLine1 = value;
    }
}