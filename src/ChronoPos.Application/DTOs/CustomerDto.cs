using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Customer operations
/// </summary>
public class CustomerDto
{
    public int Id { get; set; }
    
    // Basic Information
    [StringLength(150)]
    public string CustomerFullName { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string BusinessFullName { get; set; } = string.Empty;
    
    public bool IsBusiness { get; set; } = false;
    
    public int? BusinessTypeId { get; set; }
    
    public string BusinessTypeName { get; set; } = string.Empty;
    
    public decimal CustomerBalanceAmount { get; set; } = 0;
    
    // Business Information
    [StringLength(50)]
    public string LicenseNo { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string TrnNo { get; set; } = string.Empty;
    
    // Contact Information
    [Required]
    [StringLength(20)]
    public string MobileNo { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string HomePhone { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string OfficePhone { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string ContactMobileNo { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(100)]
    public string OfficialEmail { get; set; } = string.Empty;
    
    // Credit Management
    public bool CreditAllowed { get; set; } = false;
    
    public decimal? CreditAmountMax { get; set; }
    
    public int? CreditDays { get; set; }
    
    [StringLength(150)]
    public string CreditReference1Name { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string CreditReference2Name { get; set; } = string.Empty;
    
    // Key Contacts
    [StringLength(150)]
    public string KeyContactName { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string KeyContactMobile { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(100)]
    public string KeyContactEmail { get; set; } = string.Empty;
    
    // Finance Contact
    [StringLength(150)]
    public string FinancePersonName { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string FinancePersonMobile { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(100)]
    public string FinancePersonEmail { get; set; } = string.Empty;
    
    // Payment Options
    public bool PostDatedChequesAllowed { get; set; } = false;
    
    // System Fields
    public int? ShopId { get; set; }
    
    public string Status { get; set; } = "Active";
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Address Information
    public List<CustomerAddressDto> Addresses { get; set; } = new List<CustomerAddressDto>();
    
    // Computed Properties
    public string DisplayName => IsBusiness ? BusinessFullName : CustomerFullName;
    
    public string PrimaryMobile => string.IsNullOrEmpty(MobileNo) ? ContactMobileNo : MobileNo;
    
    public string PrimaryEmail => string.IsNullOrEmpty(OfficialEmail) ? KeyContactEmail : OfficialEmail;
}

/// <summary>
/// Data Transfer Object for Customer Address operations
/// </summary>
public class CustomerAddressDto
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
    
    public bool IsBilling { get; set; } = false;
    
    public string Status { get; set; } = "Active";
    
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

/// <summary>
/// Data Transfer Object for Business Type operations
/// </summary>
public class BusinessTypeDto
{
    public int Id { get; set; }
    
    public string BusinessTypeName { get; set; } = string.Empty;
    
    public string BusinessTypeNameAr { get; set; } = string.Empty;
    
    public string Status { get; set; } = "Active";
}
