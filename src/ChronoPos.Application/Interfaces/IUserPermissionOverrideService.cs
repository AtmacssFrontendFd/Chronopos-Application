using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

public interface IUserPermissionOverrideService
{
    Task<IEnumerable<UserPermissionOverrideDto>> GetAllAsync();
    Task<UserPermissionOverrideDto?> GetByIdAsync(int id);
    Task<IEnumerable<UserPermissionOverrideDto>> GetByUserIdAsync(int userId);
    Task<IEnumerable<UserPermissionOverrideDto>> GetActiveByUserIdAsync(int userId);
    Task<UserPermissionOverrideDto> CreateAsync(CreateUserPermissionOverrideDto createDto);
    Task<IEnumerable<UserPermissionOverrideDto>> CreateBulkAsync(IEnumerable<CreateUserPermissionOverrideDto> createDtos);
    Task<UserPermissionOverrideDto> UpdateAsync(int id, UpdateUserPermissionOverrideDto updateDto);
    Task DeleteAsync(int id);
    Task DeleteByUserIdAsync(int userId);
    Task UpdateExpiredOverridesAsync();
}
