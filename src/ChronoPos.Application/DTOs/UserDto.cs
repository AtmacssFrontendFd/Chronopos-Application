namespace ChronoPos.Application.DTOs;

/// <summary>
/// Data Transfer Object for User entity
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? PhoneNo { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? Dob { get; set; }
    public string? NationalityStatus { get; set; }
    public int RolePermissionId { get; set; }
    public string? RolePermissionName { get; set; }
    public int ShopId { get; set; }
    public bool ChangeAccess { get; set; }
    public int? ShiftTypeId { get; set; }
    public string? Address { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? UaeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PermissionCount { get; set; } // For display in UI
}

/// <summary>
/// DTO for creating a new user
/// </summary>
public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? PhoneNo { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? Dob { get; set; }
    public string? NationalityStatus { get; set; }
    public int RolePermissionId { get; set; }
    public int ShopId { get; set; }
    public bool ChangeAccess { get; set; }
    public int? ShiftTypeId { get; set; }
    public string? Address { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? UaeId { get; set; }
}

/// <summary>
/// DTO for updating an existing user
/// </summary>
public class UpdateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? PhoneNo { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? Dob { get; set; }
    public string? NationalityStatus { get; set; }
    public int RolePermissionId { get; set; }
    public int ShopId { get; set; }
    public bool ChangeAccess { get; set; }
    public int? ShiftTypeId { get; set; }
    public string? Address { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? UaeId { get; set; }
}

/// <summary>
/// DTO for updating user password
/// </summary>
public class UpdatePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
