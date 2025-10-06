using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Defines a tax type (e.g., VAT, Sales Tax) with calculation behavior.
/// Replaces the previous Tax entity.
/// </summary>
public class TaxType
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Can represent either a percentage (e.g., 5.0000) or a fixed amount depending on IsPercentage
    public decimal Value { get; set; }

    public bool IsPercentage { get; set; } = true;

    // Whether this tax is included in the product price (tax inclusive pricing semantics)
    public bool IncludedInPrice { get; set; } = false;

    // Whether tax applies on purchasing side
    public bool AppliesToBuying { get; set; } = false;

    // Whether tax applies on selling side
    public bool AppliesToSelling { get; set; } = true;

    // Useful for chained calculations (tax-on-tax)
    public int CalculationOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    // Audit fields (optional linkage to User if needed later)
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
