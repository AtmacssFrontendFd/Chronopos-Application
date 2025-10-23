using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a restaurant table reservation
/// </summary>
public class Reservation
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
    public string Status { get; set; } = "waiting"; // waiting, confirmed, cancelled, checked_in, completed
    
    public string? Notes { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Customer Customer { get; set; } = null!;
    
    public virtual RestaurantTable Table { get; set; } = null!;
    
    public virtual PaymentType? PaymentType { get; set; }
    
    /// <summary>
    /// Returns formatted reservation date and time
    /// </summary>
    public string ReservationDateTime => $"{ReservationDate:dd/MM/yyyy} at {ReservationTime:hh\\:mm}";
    
    /// <summary>
    /// Returns status display
    /// </summary>
    public string StatusDisplay => Status switch
    {
        "waiting" => "Waiting",
        "confirmed" => "Confirmed",
        "cancelled" => "Cancelled",
        "checked_in" => "Checked In",
        "completed" => "Completed",
        _ => Status
    };
    
    /// <summary>
    /// Returns formatted deposit fee
    /// </summary>
    public string DepositFeeDisplay => DepositFee > 0 ? $"{DepositFee:N2}" : "No deposit";
    
    /// <summary>
    /// Returns persons display
    /// </summary>
    public string PersonsDisplay => $"{NumberOfPersons} person{(NumberOfPersons != 1 ? "s" : "")}";
}
