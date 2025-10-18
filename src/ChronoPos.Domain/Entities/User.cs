using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Domain.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    public int Id { get; set; }
    
    public bool Deleted { get; set; } = false;
    
    public int? OwnerId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Role { get; set; }
    
    [StringLength(20)]
    public string? PhoneNo { get; set; }
    
    public decimal? Salary { get; set; }
    
    public DateTime? Dob { get; set; }
    
    public string? NationalityStatus { get; set; }
    
    public int RolePermissionId { get; set; }
    
    public int ShopId { get; set; }
    
    public bool ChangeAccess { get; set; } = false;
    
    public int? ShiftTypeId { get; set; }
    
    public string? Address { get; set; }
    
    public string? AdditionalDetails { get; set; }
    
    [StringLength(50)]
    public string? UaeId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
