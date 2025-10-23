using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Reservation operations
/// </summary>
public class ReservationDto
{
    public int Id { get; set; }
    
    public int CustomerId { get; set; }
    
    public int TableId { get; set; }
    
    public int NumberOfPersons { get; set; }
    
    [Required]
    public DateTime ReservationDate { get; set; }
    
    [Required]
    public TimeSpan ReservationTime { get; set; }
    
    public decimal DepositFee { get; set; } = 0;
    
    public int? PaymentTypeId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "waiting";
    
    public string? Notes { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Related entity information
    public string? CustomerName { get; set; }
    public string? CustomerMobile { get; set; }
    public string? TableNumber { get; set; }
    public string? PaymentTypeName { get; set; }
    
    // Display properties for UI binding
    public string DisplayName => $"Reservation #{Id}";
    
    public string CustomerDisplay => CustomerName ?? $"Customer #{CustomerId}";
    
    public string TableDisplay => TableNumber != null ? $"Table {TableNumber}" : $"Table #{TableId}";
    
    public string ReservationDateTime => $"{ReservationDate:dd/MM/yyyy} at {ReservationTime:hh\\:mm}";
    
    public string ReservationDateFormatted => ReservationDate.ToString("dd/MM/yyyy");
    
    public string ReservationTimeFormatted => ReservationTime.ToString(@"hh\:mm");
    
    public string StatusDisplay => Status switch
    {
        "waiting" => "Waiting",
        "confirmed" => "Confirmed",
        "cancelled" => "Cancelled",
        "checked_in" => "Checked In",
        "completed" => "Completed",
        _ => Status
    };
    
    public string DepositFeeDisplay => DepositFee > 0 ? $"{DepositFee:N2}" : "No deposit";
    
    public string PersonsDisplay => $"{NumberOfPersons} person{(NumberOfPersons != 1 ? "s" : "")}";
    
    public string PaymentTypeDisplay => PaymentTypeName ?? (PaymentTypeId.HasValue ? $"Payment #{PaymentTypeId}" : "Not paid");
    
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    
    public string NotesDisplay => !string.IsNullOrEmpty(Notes) ? Notes : "No notes";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return $"Reservation #{Id} - {CustomerDisplay} - {ReservationDateTime}";
    }
}

/// <summary>
/// DTO for creating a new reservation
/// </summary>
public class CreateReservationDto
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int TableId { get; set; }
    
    [Required]
    [Range(1, 100)]
    public int NumberOfPersons { get; set; }
    
    [Required]
    public DateTime ReservationDate { get; set; }
    
    [Required]
    public TimeSpan ReservationTime { get; set; }
    
    [Range(0, 999999.99)]
    public decimal DepositFee { get; set; } = 0;
    
    public int? PaymentTypeId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "waiting";
    
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing reservation
/// </summary>
public class UpdateReservationDto
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int TableId { get; set; }
    
    [Required]
    [Range(1, 100)]
    public int NumberOfPersons { get; set; }
    
    [Required]
    public DateTime ReservationDate { get; set; }
    
    [Required]
    public TimeSpan ReservationTime { get; set; }
    
    [Range(0, 999999.99)]
    public decimal DepositFee { get; set; } = 0;
    
    public int? PaymentTypeId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "waiting";
    
    public string? Notes { get; set; }
}
