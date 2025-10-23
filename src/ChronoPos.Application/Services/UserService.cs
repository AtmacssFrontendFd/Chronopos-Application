using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace ChronoPos.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetUsersWithRolesAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetUserWithRoleAsync(id);
        if (user == null || user.Deleted)
            return null;

        return MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || user.Deleted)
            return null;

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createDto)
    {
        // Validate email uniqueness
        if (await EmailExistsAsync(createDto.Email))
        {
            throw new InvalidOperationException($"Email '{createDto.Email}' already exists.");
        }

        // Hash password (basic implementation - should use proper hashing in production)
        var hashedPassword = HashPassword(createDto.Password);

        var user = new User
        {
            FullName = createDto.FullName.Trim(),
            Email = createDto.Email.Trim().ToLower(),
            Password = hashedPassword,
            Role = createDto.Role?.Trim(),
            PhoneNo = createDto.PhoneNo?.Trim(),
            Salary = createDto.Salary,
            Dob = createDto.Dob,
            NationalityStatus = createDto.NationalityStatus?.Trim(),
            RolePermissionId = createDto.RolePermissionId,
            ShopId = createDto.ShopId,
            ChangeAccess = createDto.ChangeAccess,
            ShiftTypeId = createDto.ShiftTypeId,
            Address = createDto.Address?.Trim(),
            AdditionalDetails = createDto.AdditionalDetails?.Trim(),
            UaeId = createDto.UaeId?.Trim(),
            Deleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Deleted)
        {
            throw new InvalidOperationException($"User with ID {id} not found.");
        }

        // Validate email uniqueness (excluding current user)
        if (await EmailExistsAsync(updateDto.Email, id))
        {
            throw new InvalidOperationException($"Email '{updateDto.Email}' already exists.");
        }

        user.FullName = updateDto.FullName.Trim();
        user.Email = updateDto.Email.Trim().ToLower();
        user.Role = updateDto.Role?.Trim();
        user.PhoneNo = updateDto.PhoneNo?.Trim();
        user.Salary = updateDto.Salary;
        user.Dob = updateDto.Dob;
        user.NationalityStatus = updateDto.NationalityStatus?.Trim();
        user.RolePermissionId = updateDto.RolePermissionId;
        user.ShopId = updateDto.ShopId;
        user.ChangeAccess = updateDto.ChangeAccess;
        user.ShiftTypeId = updateDto.ShiftTypeId;
        user.Address = updateDto.Address?.Trim();
        user.AdditionalDetails = updateDto.AdditionalDetails?.Trim();
        user.UaeId = updateDto.UaeId?.Trim();

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null || user.Deleted)
        {
            throw new InvalidOperationException($"User with ID {id} not found.");
        }

        user.Deleted = true;
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
    {
        return await _userRepository.EmailExistsAsync(email, excludeId);
    }

    public async Task<IEnumerable<UserDto>> GetUsersWithRolesAsync()
    {
        var users = await _userRepository.GetUsersWithRolesAsync();
        var allRoles = await _roleRepository.GetAllAsync();
        
        return users.Select(user =>
        {
            var dto = MapToDto(user);
            var role = allRoles.FirstOrDefault(r => r.RoleId == user.RolePermissionId);
            dto.RolePermissionName = role?.RoleName ?? "N/A";
            return dto;
        });
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId)
    {
        var users = await _userRepository.GetUsersByRoleAsync(roleId);
        return users.Select(MapToDto);
    }

    public async Task UpdatePasswordAsync(int userId, UpdatePasswordDto updatePasswordDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || user.Deleted)
        {
            throw new InvalidOperationException($"User with ID {userId} not found.");
        }

        // Verify current password
        if (!VerifyPassword(updatePasswordDto.CurrentPassword, user.Password))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        // Validate new password matches confirmation
        if (updatePasswordDto.NewPassword != updatePasswordDto.ConfirmPassword)
        {
            throw new InvalidOperationException("New password and confirmation do not match.");
        }

        // Hash and update password
        user.Password = HashPassword(updatePasswordDto.NewPassword);
        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<UserDto?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || user.Deleted)
            return null;

        if (!VerifyPassword(password, user.Password))
            return null;

        return MapToDto(user);
    }

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            PhoneNo = user.PhoneNo,
            Salary = user.Salary,
            Dob = user.Dob,
            NationalityStatus = user.NationalityStatus,
            RolePermissionId = user.RolePermissionId,
            ShopId = user.ShopId,
            ChangeAccess = user.ChangeAccess,
            ShiftTypeId = user.ShiftTypeId,
            Address = user.Address,
            AdditionalDetails = user.AdditionalDetails,
            UaeId = user.UaeId,
            CreatedAt = user.CreatedAt,
            PermissionCount = 0 // Will be populated when needed
        };
    }

    // Password hashing using SHA256 (matches CreateAdminWindow implementation)
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        // Hash the input password and compare with stored hash
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }
}
