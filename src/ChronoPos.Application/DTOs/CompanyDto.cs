namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for Company entity
/// </summary>
public class CompanyDto
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? LicenseNumber { get; set; }
    public int? NumberOfOwners { get; set; }
    public string? VatTrnNumber { get; set; }
    public string? PhoneNo { get; set; }
    public string? EmailOfBusiness { get; set; }
    public string? Website { get; set; }
    public string? KeyContactName { get; set; }
    public string? KeyContactMobNo { get; set; }
    public string? KeyContactEmail { get; set; }
    public decimal? LocationLatitude { get; set; }
    public decimal? LocationLongitude { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new Company
/// </summary>
public class CreateCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? LicenseNumber { get; set; }
    public int? NumberOfOwners { get; set; }
    public string? VatTrnNumber { get; set; }
    public string? PhoneNo { get; set; }
    public string? EmailOfBusiness { get; set; }
    public string? Website { get; set; }
    public string? KeyContactName { get; set; }
    public string? KeyContactMobNo { get; set; }
    public string? KeyContactEmail { get; set; }
    public decimal? LocationLatitude { get; set; }
    public decimal? LocationLongitude { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing Company
/// </summary>
public class UpdateCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    public string? LicenseNumber { get; set; }
    public int? NumberOfOwners { get; set; }
    public string? VatTrnNumber { get; set; }
    public string? PhoneNo { get; set; }
    public string? EmailOfBusiness { get; set; }
    public string? Website { get; set; }
    public string? KeyContactName { get; set; }
    public string? KeyContactMobNo { get; set; }
    public string? KeyContactEmail { get; set; }
    public decimal? LocationLatitude { get; set; }
    public decimal? LocationLongitude { get; set; }
    public string? Remarks { get; set; }
    public bool Status { get; set; }
}
