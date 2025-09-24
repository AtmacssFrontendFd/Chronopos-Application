using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Defines selling price types for different customer segments or pricing strategies
/// Maps to the selling_price_types table in the database schema
/// </summary>
public class SellingPriceType
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ArabicName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool Status { get; set; } = true;

    // Audit fields
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}