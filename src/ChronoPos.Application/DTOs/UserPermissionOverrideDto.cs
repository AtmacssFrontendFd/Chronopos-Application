using System.ComponentModel.DataAnnotations;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for UserPermissionOverride operations
/// </summary>
public class UserPermissionOverrideDto
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    
    public int PermissionId { get; set; }
    
    public bool IsAllowed { get; set; } = true;
    
    public string? Reason { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidTo { get; set; }
    
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public string? UserName { get; set; }
    
    public string? PermissionName { get; set; }
    
    // Display properties for UI binding
    public string UserDisplay => UserName ?? "-";
    public string PermissionDisplay => PermissionName ?? "-";
    public string IsAllowedDisplay => IsAllowed ? "Allowed" : "Denied";
    public string ReasonDisplay => Reason ?? "-";
    public string ValidFromDisplay => ValidFrom?.ToString("dd/MM/yyyy HH:mm") ?? "No Start Date";
    public string ValidToDisplay => ValidTo?.ToString("dd/MM/yyyy HH:mm") ?? "No End Date";
    public string CreatedAtFormatted => CreatedAt.ToString("dd/MM/yyyy HH:mm");
    public bool IsCurrentlyValid => 
        (!ValidFrom.HasValue || ValidFrom.Value <= DateTime.UtcNow) &&
        (!ValidTo.HasValue || ValidTo.Value >= DateTime.UtcNow);
    public string ValidityStatus => IsCurrentlyValid ? "Active" : "Inactive";
}

/// <summary>
/// DTO for creating a new user permission override
/// </summary>
public class CreateUserPermissionOverrideDto
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int PermissionId { get; set; }
    
    public bool IsAllowed { get; set; } = true;
    
    public string? Reason { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidTo { get; set; }
    
    public int? CreatedBy { get; set; }
}

/// <summary>
/// DTO for updating an existing user permission override
/// </summary>
public class UpdateUserPermissionOverrideDto
{
    public bool IsAllowed { get; set; } = true;
    
    public string? Reason { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidTo { get; set; }
}
