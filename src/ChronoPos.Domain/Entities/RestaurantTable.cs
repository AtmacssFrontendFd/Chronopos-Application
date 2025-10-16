using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a restaurant table in the system
/// </summary>
public class RestaurantTable
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(10)]
    public string TableNumber { get; set; } = string.Empty;
    
    public int Capacity { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "available"; // available, reserved, occupied, cleaning
    
    [StringLength(50)]
    public string? Location { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    
    /// <summary>
    /// Returns the display name for the table
    /// </summary>
    public string DisplayName => $"Table {TableNumber}";
    
    /// <summary>
    /// Returns formatted capacity
    /// </summary>
    public string CapacityDisplay => $"{Capacity} persons";
    
    /// <summary>
    /// Returns status display
    /// </summary>
    public string StatusDisplay => Status switch
    {
        "available" => "Available",
        "reserved" => "Reserved",
        "occupied" => "Occupied",
        "cleaning" => "Cleaning",
        _ => Status
    };
}
