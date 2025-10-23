using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for Store information
/// </summary>
public class StoreDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    [StringLength(50)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(100)]
    public string? ManagerName { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsDefault { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    // Display properties for UI binding
    public string DisplayName => Name;
    public string AddressDisplay => Address ?? "-";
    public string PhoneNumberDisplay => PhoneNumber ?? "-";
    public string EmailDisplay => Email ?? "-";
    public string ManagerNameDisplay => ManagerName ?? "-";
    public string StatusDisplay => IsActive ? "Active" : "Inactive";
    public string DefaultDisplay => IsDefault ? "Default" : "-";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public string UpdatedAtFormatted => UpdatedAt.ToString("dd/MM/yyyy HH:mm");
    
    // Override ToString for ComboBox display
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// DTO for creating a new store
/// </summary>
public class CreateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

/// <summary>
/// DTO for updating an existing store
/// </summary>
public class UpdateStoreDto
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? ManagerName { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}
