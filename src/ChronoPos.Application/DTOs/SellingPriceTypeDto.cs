namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for SellingPriceType entity
/// </summary>
public class SellingPriceTypeDto
{
    public long Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Status { get; set; } = true;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Display Properties
    public string ArabicNameDisplay => string.IsNullOrWhiteSpace(ArabicName) ? "-" : ArabicName;
    public string DescriptionDisplay => string.IsNullOrWhiteSpace(Description) ? "-" : Description;
    public string StatusDisplay => Status ? "Active" : "Inactive";
    public string CreatedAtFormatted => CreatedAt.ToString("MMM dd, yyyy");
}

/// <summary>
/// DTO for creating a new selling price type
/// </summary>
public class CreateSellingPriceTypeDto
{
    public string TypeName { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Status { get; set; } = true;
    public long? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing selling price type
/// </summary>
public class UpdateSellingPriceTypeDto
{
    public long Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Status { get; set; } = true;
    public long? UpdatedBy { get; set; }
}