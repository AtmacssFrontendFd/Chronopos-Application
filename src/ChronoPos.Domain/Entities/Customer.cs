using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a customer in the Point of Sale system
/// </summary>
public class Customer
{
    public int Id { get; set; }
    
    // Basic Information
    [StringLength(150)]
    public string CustomerFullName { get; set; } = string.Empty;
    
    [StringLength(150)]
    public string BusinessFullName { get; set; } = string.Empty;
    
    public bool IsBusiness { get; set; } = false;
    
    public int? BusinessTypeId { get; set; }
    
    public int? CustomerGroupId { get; set; }
    
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
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? UpdatedBy { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? DeletedAt { get; set; }
    
    public int? DeletedBy { get; set; }
    
    // Navigation Properties
    public BusinessType? BusinessType { get; set; }
    
    public CustomerGroup? CustomerGroup { get; set; }
    
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    
    public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
    
    /// <summary>
    /// Returns the display name for the customer
    /// </summary>
    public string DisplayName => IsBusiness ? BusinessFullName : CustomerFullName;
    
    /// <summary>
    /// Returns the primary contact mobile number
    /// </summary>
    public string PrimaryMobile => string.IsNullOrEmpty(MobileNo) ? ContactMobileNo : MobileNo;
    
    /// <summary>
    /// Returns the primary email address
    /// </summary>
    public string PrimaryEmail => string.IsNullOrEmpty(OfficialEmail) ? KeyContactEmail : OfficialEmail;
}
