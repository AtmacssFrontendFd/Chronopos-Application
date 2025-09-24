namespace ChronoPos.Application.DTOs;

/// <summary>
/// DTO for PaymentType entity
/// </summary>
public class PaymentTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool Status { get; set; } = true;
    public int? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for creating a new payment type
/// </summary>
public class CreatePaymentTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool Status { get; set; } = true;
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing payment type
/// </summary>
public class UpdatePaymentTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PaymentCode { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public bool Status { get; set; } = true;
    public int? UpdatedBy { get; set; }
}