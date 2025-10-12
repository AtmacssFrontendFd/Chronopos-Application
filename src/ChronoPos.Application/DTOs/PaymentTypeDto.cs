namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for PaymentType entity
/// </summary>
public class PaymentTypeDto
{
    public int Id { get; set; }
    public int? BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool Status { get; set; } = true;
    
    // Payment Configuration
    public bool ChangeAllowed { get; set; } = false;
    public bool CustomerRequired { get; set; } = false;
    public bool MarkTransactionAsPaid { get; set; } = true;
    public string? ShortcutKey { get; set; }
    public bool IsRefundable { get; set; } = true;
    public bool IsSplitAllowed { get; set; } = true;
    
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Display properties for UI binding
    public string NameArDisplay => NameAr ?? "-";
    public string StatusDisplay => Status ? "Active" : "Inactive";
    public string CreatedAtFormatted => CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-";
    public string ConfigurationSummary
    {
        get
        {
            var features = new List<string>();
            if (ChangeAllowed) features.Add("Change");
            if (CustomerRequired) features.Add("Customer Req");
            if (IsRefundable) features.Add("Refundable");
            if (IsSplitAllowed) features.Add("Split");
            return features.Any() ? string.Join(", ", features) : "Standard";
        }
    }
}

/// <summary>
/// DTO for creating a new payment type
/// </summary>
public class CreatePaymentTypeDto
{
    public int? BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool Status { get; set; } = true;
    public bool ChangeAllowed { get; set; } = false;
    public bool CustomerRequired { get; set; } = false;
    public bool MarkTransactionAsPaid { get; set; } = true;
    public string? ShortcutKey { get; set; }
    public bool IsRefundable { get; set; } = true;
    public bool IsSplitAllowed { get; set; } = true;
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing payment type
/// </summary>
public class UpdatePaymentTypeDto
{
    public int Id { get; set; }
    public int? BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool Status { get; set; } = true;
    public bool ChangeAllowed { get; set; } = false;
    public bool CustomerRequired { get; set; } = false;
    public bool MarkTransactionAsPaid { get; set; } = true;
    public string? ShortcutKey { get; set; }
    public bool IsRefundable { get; set; } = true;
    public bool IsSplitAllowed { get; set; } = true;
    public int? UpdatedBy { get; set; }
}