using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for RestaurantTable operations
/// </summary>
public class RestaurantTableDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(10)]
    public string TableNumber { get; set; } = string.Empty;
    
    public int Capacity { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "available";
    
    [StringLength(50)]
    public string? Location { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public int ReservationCount { get; set; } = 0;
    
    // Display properties for UI binding
    public string DisplayName => $"Table {TableNumber}";
    
    public string CapacityDisplay => $"{Capacity} persons";
    
    public string StatusDisplay => Status switch
    {
        "available" => "Available",
        "reserved" => "Reserved",
        "occupied" => "Occupied",
        "cleaning" => "Cleaning",
        _ => Status
    };
    
    public string LocationDisplay => Location ?? "Not specified";
    
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    
    public string ReservationCountDisplay => $"{ReservationCount} reservation{(ReservationCount != 1 ? "s" : "")}";
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return $"Table {TableNumber}";
    }
}

/// <summary>
/// DTO for creating a new restaurant table
/// </summary>
public class CreateRestaurantTableDto
{
    [Required]
    [StringLength(10)]
    public string TableNumber { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 100)]
    public int Capacity { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "available";
    
    [StringLength(50)]
    public string? Location { get; set; }
}

/// <summary>
/// DTO for updating an existing restaurant table
/// </summary>
public class UpdateRestaurantTableDto
{
    [Required]
    [StringLength(10)]
    public string TableNumber { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 100)]
    public int Capacity { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "available";
    
    [StringLength(50)]
    public string? Location { get; set; }
}
