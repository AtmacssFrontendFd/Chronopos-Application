using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Defines payment types for different payment methods
/// Maps to the Payment_Types table in the database schema
/// </summary>
public class PaymentType
{
    public int Id { get; set; }

    public int? BusinessId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string PaymentCode { get; set; } = string.Empty;

    [StringLength(255)]
    public string? NameAr { get; set; }

    public bool Status { get; set; } = true;

    // Payment Configuration
    public bool ChangeAllowed { get; set; } = false;
    
    public bool CustomerRequired { get; set; } = false;
    
    public bool MarkTransactionAsPaid { get; set; } = true;
    
    [StringLength(10)]
    public string? ShortcutKey { get; set; }
    
    public bool IsRefundable { get; set; } = true;
    
    public bool IsSplitAllowed { get; set; } = true;

    // Audit fields
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}