using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Defines payment types for different payment methods
/// Maps to the Payment_Options table in the database schema
/// </summary>
public class PaymentType
{
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string PaymentCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    public bool Status { get; set; } = true;

    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}